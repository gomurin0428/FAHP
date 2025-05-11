using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using FAHPApp.Models;

namespace FAHPApp.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private string _criteriaInput = string.Empty;
        private string _alternativesInput = string.Empty;

        public string CriteriaInput
        {
            get => _criteriaInput;
            set
            {
                if (SetProperty(ref _criteriaInput, value))
                {
                    // 自動的に CanExecute の更新
                    _generateComparisonsCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public string AlternativesInput
        {
            get => _alternativesInput;
            set
            {
                if (SetProperty(ref _alternativesInput, value))
                {
                    _generateComparisonsCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<ComparisonEntry> Comparisons { get; } = new();
        // 行列入力用 DataView（DataGrid へバインド）
        private DataView? _criteriaMatrix;
        /// <summary>
        /// 基準間比較行列を表す <see cref="System.Data.DataView"/>。UI はこれを DataGrid にバインドします。
        /// 行列セルは Saaty 1–9 スケールの整数値を想定します。
        /// </summary>
        public DataView? CriteriaMatrix
        {
            get => _criteriaMatrix;
            private set => SetProperty(ref _criteriaMatrix, value);
        }

        // UI 表示用: 基準ごとに分割したタブ コレクション
        public ObservableCollection<CriterionTabViewModel> AlternativeComparisonTabs { get; } = new();

        // 計算用に平坦なリストも保持（既存ロジックを温存）
        public ObservableCollection<AlternativeComparisonEntry> AlternativeComparisons { get; } = new();

        public ObservableCollection<WeightResultViewModel> Results { get; } = new();
        public ObservableCollection<AlternativeScoreViewModel> AlternativeResults { get; } = new();

        private double _criteriaConsistencyRatio;
        /// <summary>
        /// デファジィ化行列に基づく一貫性比率 (CR)。UI へ表示する。
        /// </summary>
        public double CriteriaConsistencyRatio
        {
            get => _criteriaConsistencyRatio;
            private set => SetProperty(ref _criteriaConsistencyRatio, value);
        }

        // 追加: 候補スコア全体の一貫性比率 (各基準 CR を重み付き平均)
        private double _alternativeConsistencyRatio;
        public double AlternativeConsistencyRatio
        {
            get => _alternativeConsistencyRatio;
            private set => SetProperty(ref _alternativeConsistencyRatio, value);
        }

        private RelayCommand? _generateComparisonsCommand;
        public RelayCommand GenerateComparisonsCommand => _generateComparisonsCommand ??= new RelayCommand(GenerateComparisons, CanGenerate);

        private RelayCommand? _computeCommand;
        public RelayCommand ComputeCommand => _computeCommand ??= new RelayCommand(ComputeScores, CanCompute);

        private bool CanGenerate() => !string.IsNullOrWhiteSpace(CriteriaInput) && !string.IsNullOrWhiteSpace(AlternativesInput);

        private bool CanCompute() => CriteriaMatrix is not null && AlternativeComparisons.Count > 0;

        private void GenerateComparisons()
        {
            Comparisons.Clear();
            AlternativeComparisonTabs.Clear();
            AlternativeComparisons.Clear();
            Results.Clear();
            AlternativeResults.Clear();

            var criteria = CriteriaInput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(s => s.Trim())
                                         .Where(s => !string.IsNullOrEmpty(s))
                                         .ToArray();
            var alternatives = AlternativesInput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(s => s.Trim())
                                                .Where(s => !string.IsNullOrEmpty(s))
                                                .ToArray();

            // 0. 行列 (DataTable) 生成
            CriteriaMatrix = BuildCriteriaMatrix(criteria);

            // 1. 基準用比較エントリ (既存ロジック温存。UI では非表示になるが計算条件の CanExecute 等に使用)
            for (int i = 0; i < criteria.Length; i++)
            {
                for (int j = i + 1; j < criteria.Length; j++)
                {
                    Comparisons.Add(new ComparisonEntry
                    {
                        CriterionA = criteria[i],
                        CriterionB = criteria[j],
                        Value = 1
                    });
                }
            }

            // 2. 各基準ごとの候補用比較エントリ
            foreach (var criterion in criteria)
            {
                var tabVm = new CriterionTabViewModel(criterion, alternatives);

                // 従来のリスト形式エントリ生成は不要になったが、既存ロジック互換のため残す (値は行列 UI と同期しない)
                for (int i = 0; i < alternatives.Length; i++)
                {
                    for (int j = i + 1; j < alternatives.Length; j++)
                    {
                        AlternativeComparisons.Add(new AlternativeComparisonEntry
                        {
                            Criterion = criterion,
                            AlternativeA = alternatives[i],
                            AlternativeB = alternatives[j],
                            Value = 1
                        });
                    }
                }

                AlternativeComparisonTabs.Add(tabVm);
            }

            _computeCommand?.RaiseCanExecuteChanged();

            CriteriaConsistencyRatio = 0.0;
            AlternativeConsistencyRatio = 0.0;
        }

        private void ComputeScores()
        {
            Results.Clear();
            AlternativeResults.Clear();

            var criteria = CriteriaInput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(s => s.Trim())
                                         .Where(s => !string.IsNullOrEmpty(s))
                                         .ToArray();
            var alternatives = AlternativesInput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(s => s.Trim())
                                                 .Where(s => !string.IsNullOrEmpty(s))
                                                 .ToArray();

            int n = criteria.Length;
            int m = alternatives.Length;

            // --- 1. 基準行列作成 (DataTable -> TriangularFuzzyNumber[,]) ---
            var criteriaMatrix = new TriangularFuzzyNumber[n, n];
            for (int i = 0; i < n; i++)
            {
                criteriaMatrix[i, i] = TriangularFuzzyNumber.One;
            }

            if (CriteriaMatrix is not null)
            {
                var table = CriteriaMatrix.Table;
                for (int i = 0; i < n; i++)
                {
                    for (int j = i + 1; j < n; j++)
                    {
                        object? raw = table.Rows[i][criteria[j]];
                        var t = ParseScaleToTFN(raw?.ToString());
                        criteriaMatrix[i, j] = t;
                        criteriaMatrix[j, i] = FuzzyAHPProcessor.Reciprocal(t);
                    }
                }
            }

            // --- 2. 候補行列（基準ごと）作成 ---
            var altMatrices = new TriangularFuzzyNumber[n][,];
            var altCrArray = new double[n];
            for (int k = 0; k < n; k++)
            {
                var mat = new TriangularFuzzyNumber[m, m];
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        mat[i, j] = i == j ? TriangularFuzzyNumber.One : TriangularFuzzyNumber.One;
                    }
                }

                // DataTable から値を取得 (上三角のみ)
                var tabVm = AlternativeComparisonTabs[k];
                var table = tabVm.AlternativeMatrix.Table;
                for (int i = 0; i < m; i++)
                {
                    for (int j = i + 1; j < m; j++)
                    {
                        object? raw = table.Rows[i][alternatives[j]];
                        var t = ParseScaleToTFN(raw?.ToString());
                        mat[i, j] = t;
                        mat[j, i] = FuzzyAHPProcessor.Reciprocal(t);
                    }
                }

                altMatrices[k] = mat;

                // 一貫性比率 (CR) を計算し、タブ ViewModel へ反映
                double altCr = FuzzyAHPProcessor.CalculateConsistencyRatio(mat);
                AlternativeComparisonTabs[k].ConsistencyRatio = Math.Round(altCr, 4);
                altCrArray[k] = altCr;
            }

            // --- 3. 計算 ---
            var criteriaWeights = FuzzyAHPProcessor.CalculateWeights(criteriaMatrix);
            var cr = FuzzyAHPProcessor.CalculateConsistencyRatio(criteriaMatrix);
            CriteriaConsistencyRatio = Math.Round(cr, 4);

            // 3.1 候補側 CR の統合 (重み付き平均)
            double altCrIntegrated = 0.0;
            for (int k = 0; k < n; k++)
            {
                altCrIntegrated += criteriaWeights[k] * altCrArray[k];
            }
            AlternativeConsistencyRatio = Math.Round(altCrIntegrated, 4);

            // --- 4. TOPSIS 用決定行列を構築 (列: 基準, 行: 代替案) ---
            var decisionMatrix = new double[m, n];
            for (int k = 0; k < n; k++)
            {
                // 各基準ごとに候補重み (altWeights) を計算
                var altWeights = FuzzyAHPProcessor.CalculateWeights(altMatrices[k]); // m 要素

                for (int j = 0; j < m; j++)
                {
                    decisionMatrix[j, k] = altWeights[j];
                }
            }

            // --- 5. TOPSIS により相対近さ係数 (Closeness) を計算 ---
            var closeness = CrispTOPSISProcessor.CalculateScores(decisionMatrix, criteriaWeights);

            // 結果を ViewModel へ反映
            for (int i = 0; i < n; i++)
            {
                Results.Add(new WeightResultViewModel
                {
                    Criterion = criteria[i],
                    Weight = Math.Round(criteriaWeights[i], 4)
                });
            }

            for (int j = 0; j < m; j++)
            {
                AlternativeResults.Add(new AlternativeScoreViewModel
                {
                    Alternative = alternatives[j],
                    Score = Math.Round(closeness[j], 4)
                });
            }
        }

        // ローカル関数: DataTable を生成
        static DataView BuildCriteriaMatrix(string[] criteria)
        {
            var table = new DataTable();

            // 行ヘッダー列
            table.Columns.Add("Criterion", typeof(string));

            // 列ヘッダー (各基準) - 文字列型に変更
            foreach (var c in criteria)
            {
                table.Columns.Add(c, typeof(string));
            }

            for (int i = 0; i < criteria.Length; i++)
            {
                var row = table.NewRow();
                row["Criterion"] = criteria[i];

                for (int j = 0; j < criteria.Length; j++)
                {
                    row[criteria[j]] = "(5,5,5)"; // 既定は 等しい (レベル5)
                }

                table.Rows.Add(row);
            }

            return table.DefaultView;
        }

        /// <summary>
        /// UI で入力されたスケール文字列 ("9", "1/3" など) を三角形ファジィ数へ変換します。
        /// </summary>
        private static TriangularFuzzyNumber ParseScaleToTFN(string? raw)
        {
            string text = (raw ?? "1").Trim();

            // 1. (l,m,u) 形式の場合
            if (text.StartsWith("(") && text.EndsWith(")") && text.Count(c => c == ',') == 2)
            {
                var inner = text.Trim('(', ')');
                var parts = inner.Split(',');
                if (parts.Length == 3 &&
                    double.TryParse(parts[0].Trim(), out var lVal) &&
                    double.TryParse(parts[1].Trim(), out var mVal) &&
                    double.TryParse(parts[2].Trim(), out var uVal))
                {
                    // レベル値 (1,3,5,7,9) で入力されている場合は比率へ変換
                    bool IsLevel(double v) => Math.Abs(v - 1) < 0.0001 || Math.Abs(v - 3) < 0.0001 || Math.Abs(v - 5) < 0.0001 || Math.Abs(v - 7) < 0.0001 || Math.Abs(v - 9) < 0.0001;

                    if (IsLevel(lVal) && IsLevel(mVal) && IsLevel(uVal))
                    {
                        static double LevelToRatio(double level, double @base = 2.0)
                            => Math.Pow(@base, (level - 5.0) / 2.0);

                        return new TriangularFuzzyNumber(LevelToRatio(lVal), LevelToRatio(mVal), LevelToRatio(uVal));
                    }

                    // それ以外はそのまま比率値とみなす
                    return new TriangularFuzzyNumber(lVal, mVal, uVal);
                }
            }

            double value;
            if (text.Contains('/'))
            {
                var parts = text.Split('/');
                if (parts.Length == 2 && int.TryParse(parts[0], out int num) && int.TryParse(parts[1], out int den) && den != 0)
                {
                    value = (double)num / den;
                }
                else
                {
                    value = 1;
                }
            }
            else if (!double.TryParse(text, out value))
            {
                value = 1;
            }

            // value >=1 はそのまま, <1 は逆数を計算
            if (value >= 1)
            {
                int scaleInt = (int)Math.Round(Math.Clamp(value, 1, 9));
                return FuzzyAHPProcessor.ToTriangular(scaleInt);
            }
            else
            {
                int recipScale = (int)Math.Round(Math.Clamp(1 / value, 1, 9));
                var baseT = FuzzyAHPProcessor.ToTriangular(recipScale);
                return FuzzyAHPProcessor.Reciprocal(baseT);
            }
        }
    }
} 