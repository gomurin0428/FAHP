using System;
using System.Collections.Generic;
using System.Linq;

namespace FAHPApp.Models
{
    /// <summary>
    /// ファジィAHP計算ユーティリティ。
    /// </summary>
    public static class FuzzyAHPProcessor
    {
        /// <summary>
        /// 与えられた三角形ファジィ数のペアワイズ比較行列から基準の重みを計算します。
        /// </summary>
        /// <param name="matrix">n×n のペアワイズ比較行列。</param>
        /// <returns>正規化された重み (合計 1)。</returns>
        public static double[] CalculateWeights(TriangularFuzzyNumber[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("行列は正方でなければなりません。", nameof(matrix));

            // 1. 各行の幾何平均を計算
            var gms = new TriangularFuzzyNumber[n];
            for (int i = 0; i < n; i++)
            {
                double prodL = 1, prodM = 1, prodU = 1;
                for (int j = 0; j < n; j++)
                {
                    var v = matrix[i, j];
                    prodL *= v.L;
                    prodM *= v.M;
                    prodU *= v.U;
                }
                double exponent = 1.0 / n;
                gms[i] = new TriangularFuzzyNumber(Math.Pow(prodL, exponent), Math.Pow(prodM, exponent), Math.Pow(prodU, exponent));
            }

            // 2. 行ごとの幾何平均を合計
            var sumGM = new TriangularFuzzyNumber(gms.Sum(g => g.L), gms.Sum(g => g.M), gms.Sum(g => g.U));

            // 3. 重みを計算
            var weights = new double[n];
            for (int i = 0; i < n; i++)
            {
                var wFuzzy = new TriangularFuzzyNumber(gms[i].L / sumGM.U, gms[i].M / sumGM.M, gms[i].U / sumGM.L);
                weights[i] = wFuzzy.Defuzzify();
            }

            // 4. 正規化
            double total = weights.Sum();
            for (int i = 0; i < n; i++)
            {
                weights[i] /= total;
            }
            return weights;
        }

        /// <summary>
        /// サトティ・1-9 スケールを三角形ファジィ数に変換します。
        /// </summary>
        public static TriangularFuzzyNumber ToTriangular(int scale)
        {
            if (scale < 1 || scale > 9) throw new ArgumentOutOfRangeException(nameof(scale));
            return scale switch
            {
                1 => new TriangularFuzzyNumber(1, 1, 1),
                9 => new TriangularFuzzyNumber(9, 9, 9),
                _ => new TriangularFuzzyNumber(scale - 1, scale, scale + 1)
            };
        }

        /// <summary>
        /// 逆数側の三角形ファジィ数を取得します。
        /// </summary>
        public static TriangularFuzzyNumber Reciprocal(in TriangularFuzzyNumber t)
            => new(1.0 / t.U, 1.0 / t.M, 1.0 / t.L);

        /// <summary>
        /// 基準行列と各基準ごとの候補行列から総合スコアを計算します。
        /// </summary>
        /// <param name="criteriaMatrix">基準 (n × n) 行列。</param>
        /// <param name="alternativeMatrices">各基準ごとの候補行列 (n 個, いずれも m × m)。インデックス順は criteriaMatrix の行/列順と対応。</param>
        /// <returns>候補のスコア (合計 1)。</returns>
        public static double[] CalculateAlternativeScores(
            TriangularFuzzyNumber[,] criteriaMatrix,
            IReadOnlyList<TriangularFuzzyNumber[,]> alternativeMatrices)
        {
            int n = criteriaMatrix.GetLength(0);
            if (n != criteriaMatrix.GetLength(1))
                throw new ArgumentException("criteriaMatrix は正方でなければなりません。", nameof(criteriaMatrix));
            if (alternativeMatrices.Count != n)
                throw new ArgumentException("alternativeMatrices の数が criteriaMatrix のサイズと一致していません。", nameof(alternativeMatrices));

            // 基準重み
            var criteriaWeights = CalculateWeights(criteriaMatrix);

            // 候補数 m を取得
            int m = alternativeMatrices[0].GetLength(0);
            var altWeightsPerCriterion = new double[n][];
            for (int k = 0; k < n; k++)
            {
                var matrix = alternativeMatrices[k];
                if (matrix.GetLength(0) != m || matrix.GetLength(1) != m)
                    throw new ArgumentException($"alternativeMatrices[{k}] のサイズが不一致です。", nameof(alternativeMatrices));
                altWeightsPerCriterion[k] = CalculateWeights(matrix);
            }

            // 総合スコア
            var scores = new double[m];
            for (int j = 0; j < m; j++)
            {
                double s = 0;
                for (int k = 0; k < n; k++)
                {
                    s += criteriaWeights[k] * altWeightsPerCriterion[k][j];
                }
                scores[j] = s;
            }

            // 正規化 (念のため)
            double total = scores.Sum();
            if (total > 0)
            {
                for (int j = 0; j < m; j++)
                {
                    scores[j] /= total;
                }
            }
            return scores;
        }

        /// <summary>
        /// デファジィ化（重心法）後の行列を用いて一貫性比率 (Consistency Ratio; CR) を計算します。
        /// Saaty の CI/CR 定義に基づきます。
        /// </summary>
        /// <param name="matrix">n×n のペアワイズ比較行列 (TriangularFuzzyNumber)。</param>
        /// <returns>CR 値。n &lt; 3 の場合は 0 を返します。</returns>
        public static double CalculateConsistencyRatio(TriangularFuzzyNumber[,] matrix)
        {
            int n = matrix.GetLength(0);
            if (n != matrix.GetLength(1))
                throw new ArgumentException("行列は正方でなければなりません。", nameof(matrix));

            if (n < 3) return 0.0;

            // 1. デファジィ化したクリスプ行列 A を作成
            var a = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    a[i, j] = matrix[i, j].Defuzzify();
                }
            }

            // 2. 幾何平均法で固有ベクトル w を近似
            var w = new double[n];
            for (int i = 0; i < n; i++)
            {
                double prod = 1.0;
                for (int j = 0; j < n; j++) prod *= a[i, j];
                w[i] = Math.Pow(prod, 1.0 / n);
            }
            double sumW = w.Sum();
            for (int i = 0; i < n; i++) w[i] /= sumW;

            // 3. λ_max を推定: λ_i = (A * w)_i / w_i
            double lambdaSum = 0.0;
            for (int i = 0; i < n; i++)
            {
                double rowDot = 0.0;
                for (int j = 0; j < n; j++) rowDot += a[i, j] * w[j];
                lambdaSum += rowDot / w[i];
            }
            double lambdaMax = lambdaSum / n;

            // 4. CI, CR
            double ci = (lambdaMax - n) / (n - 1);
            double ri = GetRandomIndex(n);
            return ri <= 0 ? 0.0 : ci / ri;
        }

        // Saaty のランダム指数 (RI) – n = 1..15
        private static double GetRandomIndex(int n) => n switch
        {
            1 => 0.0,
            2 => 0.0,
            3 => 0.58,
            4 => 0.90,
            5 => 1.12,
            6 => 1.24,
            7 => 1.32,
            8 => 1.41,
            9 => 1.45,
            10 => 1.49,
            11 => 1.51,
            12 => 1.48,
            13 => 1.56,
            14 => 1.57,
            15 => 1.59,
            _ => 1.59 // n>15 はおおよそ 1.59 とみなす
        };
    }
} 