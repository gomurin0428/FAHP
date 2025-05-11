using System;
using System.Windows;

namespace FAHPApp
{
    /// <summary>
    /// 代表値と信頼度（スプレッド幅）を選択させるダイアログ。
    /// </summary>
    public partial class FuzzyInputDialog : Window
    {
        public double RepresentativeValue { get; private set; }
        public double Spread { get; private set; }

        public FuzzyInputDialog(string[] repOptions, double[] confOptions, string? currentValue = null)
        {
            InitializeComponent();

            RepCombo.ItemsSource = repOptions;
            ConfCombo.ItemsSource = confOptions;

            // 既定値
            if (!string.IsNullOrWhiteSpace(currentValue))
            {
                RepCombo.SelectedItem = currentValue;
            }
            else
            {
                RepCombo.SelectedIndex = 4; // "1"
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

            RepresentativeValue = ParseRepText(repText);
            Spread = spread;
            DialogResult = true;
        }

        private static double ParseRepText(string txt)
        {
            txt = txt.Trim();
            if (txt.Contains('/'))
            {
                var parts = txt.Split('/');
                if (parts.Length == 2 && double.TryParse(parts[0], out var num) && double.TryParse(parts[1], out var den) && den != 0)
                {
                    return num / den;
                }
            }
            if (double.TryParse(txt, out var v))
            {
                return v;
            }
            return 1.0;
        }

        public (double L, double M, double U) ToTriangular()
        {
            double l = Math.Max(RepresentativeValue - Spread, 0.0001);
            double u = RepresentativeValue + Spread;
            return (l, RepresentativeValue, u);
        }
    }
} 