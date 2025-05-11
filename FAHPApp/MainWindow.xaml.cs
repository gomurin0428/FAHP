using System.Text;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FAHPApp.ViewModels;

namespace FAHPApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // コンボボックスに表示するスケール候補
        private static readonly string[] _scaleOptions =
        {
            "9", "7", "5", "3", "1", "1/3", "1/5", "1/7", "1/9"
        };

        // 信頼度（スプレッド幅）の選択肢 (対称 ±delta)
        private static readonly double[] _confidenceOptions =
        {
            0.0, 0.25, 0.5, 0.75, 1.0
        };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void CriteriaMatrix_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // 'Criterion' 列は行ラベルなので編集不可にする
            if (e.PropertyName == "Criterion" && e.Column is DataGridTextColumn txt)
            {
                txt.IsReadOnly = true;
                return;
            }

            // それ以外のセルは文字列を表示 (ファジィ数 "(l,m,u)" 表示) させるため、デフォルト列をそのまま利用する。
            // ユーザー編集は BeginningEdit でインターセプトし、独自ダイアログを表示する。
            if (e.Column is DataGridTextColumn textCol)
            {
                textCol.IsReadOnly = false; // 編集自体は BeginEdit でキャンセルする
                textCol.Binding = new Binding(e.PropertyName)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
            }
            return;
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
            return;
        }

        /// <summary>
        /// 対角および下三角セルの編集を禁止します。
        /// </summary>
        private void CriteriaMatrix_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.DisplayIndex == 0) return; // 行ラベル列は常に編集不可

            int rowIndex = e.Row.GetIndex();
            int colIndex = e.Column.DisplayIndex - 1; // 先頭列 (Criterion) を除いた実際の列番号

            if (colIndex <= rowIndex)
            {
                e.Cancel = true;
                return;
            }

            // デフォルト編集をキャンセルし、ダイアログを表示
            e.Cancel = true;

            // 対象セルを取得
            if (sender is not DataGrid grid) return;
            if (grid.Items[rowIndex] is not System.Data.DataRowView rowView) return;
            string columnName = e.Column.Header?.ToString() ?? string.Empty;

            string? currentVal = rowView.Row[columnName]?.ToString();

            var dlg = new FuzzyInputDialog(_scaleOptions, _confidenceOptions, currentVal)
            {
                Owner = this
            };
            if (dlg.ShowDialog() == true)
            {
                var (l, m, u) = dlg.ToTriangular();
                rowView.Row[columnName] = $"({l:0.###},{m:0.###},{u:0.###})";
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
            if (sender is not DataGrid grid) return;
            if (grid.Items[rowIndex] is not System.Data.DataRowView rowView) return;
            string columnName = e.Column.Header?.ToString() ?? string.Empty;
            string? currentVal = rowView.Row[columnName]?.ToString();
            var dlg = new FuzzyInputDialog(_scaleOptions, _confidenceOptions, currentVal)
            {
                Owner = this
            };
            if (dlg.ShowDialog() == true)
            {
                var (l, m, u) = dlg.ToTriangular();
                rowView.Row[columnName] = $"({l:0.###},{m:0.###},{u:0.###})";
            }
        }
    }
}