using System.Text;
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

            // それ以外はコンボボックス列に置き換える
            var combo = new DataGridComboBoxColumn
            {
                Header = e.Column.Header,
                ItemsSource = _scaleOptions,
                SelectedItemBinding = new Binding(e.PropertyName)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                }
            };
            e.Column = combo;
        }

        private void AltMatrix_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Alternative" && e.Column is DataGridTextColumn txt)
            {
                txt.IsReadOnly = true;
                return;
            }

            var combo = new DataGridComboBoxColumn
            {
                Header = e.Column.Header,
                ItemsSource = _scaleOptions,
                SelectedItemBinding = new Binding(e.PropertyName)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                }
            };
            e.Column = combo;
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
            }
        }
    }
}