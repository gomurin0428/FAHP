using System;
using System.Windows;
using System.Linq;

namespace FAHPApp
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
                MessageBox.Show(this, "代表値と信頼度を選択してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RepresentativeValue = LevelToRatio(ParseLevel(repText));
            Delta = spread;
            DialogResult = true;
        }

        /// <summary>
        /// レベル文字列 ("1","3",...) を int に変換します。
        /// 何らかの理由でパース出来ない場合は 5 とみなします。
        /// </summary>
        private static int ParseLevel(string txt)
        {
            txt = txt.Trim();
            if (int.TryParse(txt, out var lvl) && (lvl == 1 || lvl == 3 || lvl == 5 || lvl == 7 || lvl == 9))
            {
                return lvl;
            }
            // 旧バージョンとの互換性: "1/3" 等が来た場合は逆数を計算して最も近いレベルにマッピング
            if (txt.Contains('/'))
            {
                var parts = txt.Split('/');
                if (parts.Length == 2 && double.TryParse(parts[0], out var num) && double.TryParse(parts[1], out var den) && den != 0)
                {
                    double ratio = num / den;
                    // 1/9〜1/3 を 3,5,7,9 の逆数と仮定
                    if (Math.Abs(ratio - 0.25) < 0.05) return 9; // 1/4 ≈ 1/4
                    if (Math.Abs(ratio - 0.5) < 0.05) return 7;
                    if (Math.Abs(ratio - 1.0) < 0.05) return 5;
                    if (Math.Abs(ratio - 2.0) < 0.1) return 3;
                }
            }
            return 5;
        }

        /// <summary>
        /// レベル (1,3,5,7,9) を比率値へ変換します。中央 5 → 1。<br/>
        /// ratio = base^{(level-5)/2} として指数マッピングを採用します。
        /// </summary>
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

        /// <summary>
        /// セル値文字列から代表レベル ("1","3"...) を抽出します。
        /// "(5,5,5)" などの形式にも対応。
        /// </summary>
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
                    // 端数がある場合は四捨五入して文字列化
                    if (double.TryParse(mid, out var mVal))
                    {
                        // 中央値から最も近いレベルを返す
                        int[] levels = { 1, 3, 5, 7, 9 };
                        int nearest = levels.OrderBy(lvl => Math.Abs(lvl - mVal)).First();
                        return nearest.ToString();
                    }
                }
            }
            // 直接数値の場合
            if (int.TryParse(raw, out var lvl2) && (lvl2 == 1 || lvl2 == 3 || lvl2 == 5 || lvl2 == 7 || lvl2 == 9))
            {
                return raw;
            }
            return null;
        }
    }
} 