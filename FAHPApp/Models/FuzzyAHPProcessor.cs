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
    }
} 