using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FAHP.Shared.ViewModels; // ViewModels は共有ライブラリ経由で参照

namespace FAHPWebApp3
{
    /// <summary>
    /// OpenSilver 版のメインページ。
    /// </summary>
    public partial class MainPage : Page
    {
        // ユーザーに提示するレベル選択肢（1,3,5,7,9）
        private static readonly string[] _scaleOptions =
        {
            "9", "7", "5", "3", "1"
        };

        // デルタ（信頼幅）候補 (0〜0.75)。±δ を乗算する相対幅として扱う
        private static readonly double[] _confidenceOptions =
        {
            0.0, 0.25, 0.5, 0.75
        };

        public MainPage()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void CriteriaMatrix_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // 'Criterion' 列は行ラベルなので編集不可
            if (e.PropertyName == "Criterion" && e.Column is DataGridTextColumn txt)
            {
                txt.IsReadOnly = true;
                return;
            }

            if (e.Column is DataGridTextColumn textCol)
            {
                textCol.IsReadOnly = false;
                textCol.Binding = new Binding(e.PropertyName)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
            }
        }

        private void AltMatrix_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Alternative" && e.Column is DataGridTextColumn txt)
            {
                txt.IsReadOnly = true;
                return;
            }

            if (e.Column is DataGridTextColumn textCol)
            {
                textCol.IsReadOnly = false;
                textCol.Binding = new Binding(e.PropertyName)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
            }
        }

        /// <summary>
        /// 対角および下三角セルの編集を禁止します。
        /// </summary>
        private void CriteriaMatrix_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.DisplayIndex == 0) return; // 行ラベル列は編集不可

            int rowIndex = e.Row.GetIndex();
            int colIndex = e.Column.DisplayIndex - 1; // 先頭列を除く
            if (colIndex <= rowIndex)
            {
                e.Cancel = true;
                return;
            }

            // デフォルト編集をキャンセルし、ダイアログを表示
            e.Cancel = true;
            if (e.Row.DataContext is not System.Data.DataRowView rowView) return;
            string columnName = e.Column.Header?.ToString() ?? string.Empty;
            string? currentVal = rowView.Row[columnName]?.ToString();

            var dlg = new FuzzyInputDialog(_scaleOptions, _confidenceOptions, currentVal);
            if (dlg.ShowDialog() == true)
            {
                var (l, m, u) = dlg.ToTriangular();
                rowView.Row[columnName] = FormatTriangularForDisplay(l, m, u);

                // 対称セルも更新
                DataTable tbl = rowView.Row.Table;
                string rowHeader = rowView.Row["Criterion"].ToString() ?? string.Empty;
                DataRow? symmetricRow = null;
                foreach (DataRow r in tbl.Rows)
                {
                    if ((r["Criterion"].ToString() ?? string.Empty) == columnName)
                    {
                        symmetricRow = r;
                        break;
                    }
                }
                if (symmetricRow is not null && !string.IsNullOrEmpty(rowHeader))
                {
                    symmetricRow[rowHeader] = FormatTriangularForDisplay(1.0 / u, 1.0 / m, 1.0 / l);
                }
            }
        }

        private void AltMatrix_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.DisplayIndex == 0) return; // 行ラベル列

            int rowIndex = e.Row.GetIndex();
            int colIndex = e.Column.DisplayIndex - 1; // Alternative 列を除外
            if (colIndex <= rowIndex)
            {
                e.Cancel = true;
                return;
            }

            e.Cancel = true;
            if (e.Row.DataContext is not System.Data.DataRowView rowView) return;
            string columnName = e.Column.Header?.ToString() ?? string.Empty;
            string? currentVal = rowView.Row[columnName]?.ToString();

            var dlg = new FuzzyInputDialog(_scaleOptions, _confidenceOptions, currentVal);
            if (dlg.ShowDialog() == true)
            {
                var (l, m, u) = dlg.ToTriangular();
                rowView.Row[columnName] = FormatTriangularForDisplay(l, m, u);

                DataTable tbl = rowView.Row.Table;
                string rowHeader = rowView.Row["Alternative"].ToString() ?? string.Empty;
                DataRow? symmetricRow = null;
                foreach (DataRow r in tbl.Rows)
                {
                    if ((r["Alternative"].ToString() ?? string.Empty) == columnName)
                    {
                        symmetricRow = r;
                        break;
                    }
                }
                if (symmetricRow is not null && !string.IsNullOrEmpty(rowHeader))
                {
                    symmetricRow[rowHeader] = FormatTriangularForDisplay(1.0 / u, 1.0 / m, 1.0 / l);
                }
            }
        }

        /// <summary>
        /// 比率値の三角形ファジィ数を UI 表示用文字列に変換します。
        /// (1,1,1) の場合は (5,5,5) と表示。
        /// </summary>
        private static string FormatTriangularForDisplay(double l, double m, double u)
        {
            bool approxEqual(double a, double b) => Math.Abs(a - b) < 1e-6;
            if (approxEqual(l, 1.0) && approxEqual(m, 1.0) && approxEqual(u, 1.0))
            {
                return "(5,5,5)";
            }

            static int RatioToNearestLevel(double ratio)
            {
                int[] levels = { 1, 3, 5, 7, 9 };
                double LevelToRatio(int lvl) => Math.Pow(2.0, (lvl - 5) / 2.0);
                return levels
                    .Select(lvl => new { Level = lvl, Ratio = LevelToRatio(lvl) })
                    .OrderBy(x => Math.Abs(x.Ratio - ratio))
                    .First().Level;
            }

            int lLvl = RatioToNearestLevel(l);
            int mLvl = RatioToNearestLevel(m);
            int uLvl = RatioToNearestLevel(u);
            return $"({lLvl},{mLvl},{uLvl})";
        }
    }
}
