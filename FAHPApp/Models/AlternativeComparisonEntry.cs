namespace FAHPApp.Models
{
    /// <summary>
    /// 1 つの基準に対する候補同士のペアワイズ比較エントリ。
    /// </summary>
    public sealed class AlternativeComparisonEntry
    {
        /// <summary>
        /// 対象となる基準名。
        /// </summary>
        public required string Criterion { get; init; }

        public required string AlternativeA { get; init; }
        public required string AlternativeB { get; init; }

        /// <summary>
        /// サトティ 1–9 スケール。AlternativeA が AlternativeB よりどれだけ優れているか。
        /// </summary>
        public int Value { get; set; } = 1;
    }
} 