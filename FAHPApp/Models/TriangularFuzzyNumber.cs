using System;

namespace FAHPApp.Models
{
    /// <summary>
    /// 三角形ファジィ数 (l, m, u)。
    /// </summary>
    public readonly record struct TriangularFuzzyNumber(double L, double M, double U)
    {
        public static TriangularFuzzyNumber One => new(1.0, 1.0, 1.0);

        public double Defuzzify() => (L + M + U) / 3.0;

        public static TriangularFuzzyNumber operator +(in TriangularFuzzyNumber a, in TriangularFuzzyNumber b)
            => new(a.L + b.L, a.M + b.M, a.U + b.U);

        public static TriangularFuzzyNumber operator /(in TriangularFuzzyNumber a, double scalar)
            => new(a.L / scalar, a.M / scalar, a.U / scalar);

        public static TriangularFuzzyNumber operator *(in TriangularFuzzyNumber a, in TriangularFuzzyNumber b)
            => new(a.L * b.L, a.M * b.M, a.U * b.U);

        public static TriangularFuzzyNumber Pow(in TriangularFuzzyNumber a, double exponent)
            => new(Math.Pow(a.L, exponent), Math.Pow(a.M, exponent), Math.Pow(a.U, exponent));
    }
} 