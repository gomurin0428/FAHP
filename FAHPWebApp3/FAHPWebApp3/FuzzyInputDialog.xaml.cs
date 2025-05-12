using System;
using System.Linq;
using System.Windows;

namespace FAHPWebApp3
{
    /// <summary>
    /// 代表値と信頼度（スプレッド幅）を選択させるダイアログ。
    /// </summary>
    public partial class FuzzyInputDialog : Window
    {
        public double RepresentativeValue { get; private set; }
        public double Delta { get; private set; }

        public FuzzyInputDialog(string[] repOptions, double[] confOptions, string? currentValue = null)
        {
            InitializeComponent();

            RepCombo.ItemsSource = repOptions;
            ConfCombo.ItemsSource = confOptions;

            // 既定値設定
            if (!string.IsNullOrWhiteSpace(currentValue))
            {
                string? levelStr = ExtractRepresentativeLevel(currentValue);
                if (levelStr is not null && Array.IndexOf(repOptions, levelStr) >= 0)
                {
                    RepCombo.SelectedItem = levelStr;
                }
                else
                {
                    RepCombo.SelectedIndex = 2; // デフォルト "5"
                }
            }
            else
            {
                RepCombo.SelectedIndex = 2; // デフォルト "5"
            }
            // スプレッド 0 をデフォルト
            ConfCombo.SelectedIndex = 0;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (RepCombo.SelectedItem is not string repText || ConfCombo.SelectedItem is not double spread)
            {
                MessageBox.Show("代表値と信頼度を選択してください。", "入力エラー", MessageBoxButton.OK);
                return;
            }

            RepresentativeValue = LevelToRatio(ParseLevel(repText));
            Delta = spread;
            _dialogResult = true;
            this.Close();
        }

        private static int ParseLevel(string txt)
        {
            txt = txt.Trim();
            if (int.TryParse(txt, out var lvl) && (lvl == 1 || lvl == 3 || lvl == 5 || lvl == 7 || lvl == 9))
            {
                return lvl;
            }
            if (txt.Contains('/'))
            {
                var parts = txt.Split('/');
                if (parts.Length == 2 && double.TryParse(parts[0], out var num) && double.TryParse(parts[1], out var den) && den != 0)
                {
                    double ratio = num / den;
                    if (Math.Abs(ratio - 0.25) < 0.05) return 9; // 1/4 に近い
                    if (Math.Abs(ratio - 0.5) < 0.05) return 7;
                    if (Math.Abs(ratio - 1.0) < 0.05) return 5;
                    if (Math.Abs(ratio - 2.0) < 0.1) return 3;
                }
            }
            return 5;
        }

        private static double LevelToRatio(int level, double @base = 2.0)
        {
            return Math.Pow(@base, (level - 5) / 2.0);
        }

        public (double L, double M, double U) ToTriangular()
        {
            double l = RepresentativeValue * (1 - Delta);
            double u = RepresentativeValue * (1 + Delta);
            return (l, RepresentativeValue, u);
        }

        private static string? ExtractRepresentativeLevel(string raw)
        {
            raw = raw.Trim();
            if (raw.StartsWith("(") && raw.EndsWith(")") && raw.Count(c => c == ',') == 2)
            {
                var inner = raw.Trim('(', ')');
                var parts = inner.Split(',');
                if (parts.Length == 3)
                {
                    string mid = parts[1].Trim();
                    if (double.TryParse(mid, out var mVal))
                    {
                        int[] levelSet = { 1, 3, 5, 7, 9 };
                        foreach (var lvl in levelSet)
                        {
                            if (Math.Abs(mVal - lvl) < 0.001)
                            {
                                return lvl.ToString();
                            }
                        }

                        int[] levels = { 1, 3, 5, 7, 9 };
                        int nearest = levels
                            .Select(lvl => new { Level = lvl, Ratio = LevelToRatio(lvl) })
                            .OrderBy(x => Math.Abs(x.Ratio - mVal))
                            .First().Level;
                        return nearest.ToString();
                    }
                }
            }
            if (int.TryParse(raw, out var lvl2) && (lvl2 == 1 || lvl2 == 3 || lvl2 == 5 || lvl2 == 7 || lvl2 == 9))
            {
                return raw;
            }
            return null;
        }

        // --- OpenSilver では Window.ShowDialog が無いので簡易実装 ---
        private bool _dialogResult = false;
        public bool ShowDialog()
        {
            this.Show();
            // Show は非同期のため厳密なモーダルにはならないが、呼び出し側では true/false を利用したいので暫定対応
            return _dialogResult;
        }

        // DialogResult プロパティの代替 (参照用)
        public bool DialogResult => _dialogResult;
    }
} 