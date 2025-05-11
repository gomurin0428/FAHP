using System.Collections.ObjectModel;
using System.Data;
using FAHPApp.Models;

namespace FAHPApp.ViewModels
{
    /// <summary>
    /// 1 つの基準に紐づく候補比較行列を保持する ViewModel。
    /// </summary>
    public sealed class CriterionTabViewModel : ViewModelBase
    {
        public CriterionTabViewModel(string criterion, string[] alternatives)
        {
            Criterion = criterion;
            AlternativeMatrix = BuildAlternativeMatrix(alternatives);
        }

        /// <summary>
        /// タブヘッダーとして表示する基準名。
        /// </summary>
        public string Criterion { get; }

        /// <summary>
        /// 当該基準に対する候補間比較行列 DataView。
        /// </summary>
        public DataView AlternativeMatrix { get; }

        // 旧 UI 互換用 (未使用になっても既存コードへの影響を避ける)
        public ObservableCollection<AlternativeComparisonEntry> Comparisons { get; } = new();

        /// <summary>
        /// 当該基準に対する候補間比較行列の一貫性比率 (CR)。
        /// </summary>
        private double _consistencyRatio;
        public double ConsistencyRatio
        {
            get => _consistencyRatio;
            set => SetProperty(ref _consistencyRatio, value);
        }

        private static DataView BuildAlternativeMatrix(string[] alternatives)
        {
            var table = new DataTable();

            table.Columns.Add("Alternative", typeof(string));
            foreach (var alt in alternatives)
            {
                table.Columns.Add(alt, typeof(string));
            }

            for (int i = 0; i < alternatives.Length; i++)
            {
                var row = table.NewRow();
                row["Alternative"] = alternatives[i];
                for (int j = 0; j < alternatives.Length; j++)
                {
                    row[alternatives[j]] = "(5,5,5)"; // 初期値（等しい）
                }
                table.Rows.Add(row);
            }

            return table.DefaultView;
        }
    }
} 