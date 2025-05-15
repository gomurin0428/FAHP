using System;

namespace FAHP.Shared.Models
{
    /// <summary>
    /// 三角形ファジィ数 (l, m, u)。
    /// </summary>
    public readonly struct TriangularFuzzyNumber
    {
        public readonly double L;
        public readonly double M;
        public readonly double U;

        public TriangularFuzzyNumber(double l, double m, double u)
        {
            L = l;
            M = m;
            U = u;
        }

        public static TriangularFuzzyNumber One => new TriangularFuzzyNumber(1.0, 1.0, 1.0);

        public double Defuzzify() => (L + M + U) / 3.0;

        public static TriangularFuzzyNumber operator +(in TriangularFuzzyNumber a, in TriangularFuzzyNumber b)
            => new TriangularFuzzyNumber(a.L + b.L, a.M + b.M, a.U + b.U);

        public static TriangularFuzzyNumber operator /(in TriangularFuzzyNumber a, double scalar)
            => new TriangularFuzzyNumber(a.L / scalar, a.M / scalar, a.U / scalar);

        public static TriangularFuzzyNumber operator *(in TriangularFuzzyNumber a, in TriangularFuzzyNumber b)
            => new TriangularFuzzyNumber(a.L * b.L, a.M * b.M, a.U * b.U);

        public static TriangularFuzzyNumber Pow(in TriangularFuzzyNumber a, double exponent)
            => new TriangularFuzzyNumber(Math.Pow(a.L, exponent), Math.Pow(a.M, exponent), Math.Pow(a.U, exponent));
    }
}     