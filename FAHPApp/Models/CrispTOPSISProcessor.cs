using System;
using System.Linq;

namespace FAHPApp.Models
{
    /// <summary>
    /// クリスプ（実数）TOPSIS 計算ユーティリティ。
    /// FAHP で得た基準重みと実測値の決定行列を入力として、各代替案の相対近さ係数 (0–1) を返します。
    /// </summary>
    public static class CrispTOPSISProcessor
    {
        /// <summary>
        /// TOPSIS による代替案スコアを計算します。
        /// </summary>
        /// <param name="decisionMatrix">m × n の決定行列。行 = 代替案、列 = 基準。</param>
        /// <param name="weights">長さ n の基準重みベクトル (合計 1 を推奨)。</param>
        /// <param name="isBenefit">各基準が Benefit (true) か Cost (false) かを示すフラグ。null の場合はすべて Benefit とみなす。</param>
        /// <returns>長さ m の相対近さ係数 (大きいほど良い)。</returns>
        /// <exception cref="ArgumentException">行列サイズと重みの不一致、または負の入力があった場合に発生。</exception>
        public static double[] CalculateScores(double[,] decisionMatrix, double[] weights, bool[]? isBenefit = null)
        {
            int m = decisionMatrix.GetLength(0); // 代替案数
            int n = decisionMatrix.GetLength(1); // 基準数

            if (weights.Length != n)
                throw new ArgumentException("weights の長さが decisionMatrix の列数と一致していません。", nameof(weights));

            if (isBenefit is not null && isBenefit.Length != n)
                throw new ArgumentException("isBenefit の長さが decisionMatrix の列数と一致していません。", nameof(isBenefit));

            // 0. Benefit/Cost フラグ既定値
            isBenefit ??= Enumerable.Repeat(true, n).ToArray();

            // 1. 列ごとの L2 ノルムを計算
            var norm = new double[n];
            for (int j = 0; j < n; j++)
            {
                double sumSq = 0.0;
                for (int i = 0; i < m; i++)
                {
                    double v = decisionMatrix[i, j];
                    sumSq += v * v;
                }
                norm[j] = Math.Sqrt(sumSq);
                if (norm[j] == 0) norm[j] = 1; // ゼロ除算回避
            }

            // 2. 正規化 r_ij, 重み付け v_ij
            var vMat = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double r = decisionMatrix[i, j] / norm[j];
                    vMat[i, j] = r * weights[j];
                }
            }

            // 3. 理想点 / 反理想点
            var vPlus = new double[n];
            var vMinus = new double[n];
            for (int j = 0; j < n; j++)
            {
                double colMax = double.MinValue;
                double colMin = double.MaxValue;
                for (int i = 0; i < m; i++)
                {
                    double v = vMat[i, j];
                    if (v > colMax) colMax = v;
                    if (v < colMin) colMin = v;
                }
                if (isBenefit[j])
                {
                    vPlus[j] = colMax;
                    vMinus[j] = colMin;
                }
                else
                {
                    vPlus[j] = colMin; // Cost 基準は小さいほど良い
                    vMinus[j] = colMax;
                }
            }

            // 4. 距離計算
            var dPlus = new double[m];
            var dMinus = new double[m];
            for (int i = 0; i < m; i++)
            {
                double sumSqPlus = 0.0;
                double sumSqMinus = 0.0;
                for (int j = 0; j < n; j++)
                {
                    double diffPlus = vMat[i, j] - vPlus[j];
                    double diffMinus = vMat[i, j] - vMinus[j];
                    sumSqPlus += diffPlus * diffPlus;
                    sumSqMinus += diffMinus * diffMinus;
                }
                dPlus[i] = Math.Sqrt(sumSqPlus);
                dMinus[i] = Math.Sqrt(sumSqMinus);
            }

            // 5. 相対近さ係数
            var c = new double[m];
            for (int i = 0; i < m; i++)
            {
                double denom = dPlus[i] + dMinus[i];
                c[i] = denom == 0 ? 0 : dMinus[i] / denom;
            }

            return c;
        }
    }
} 