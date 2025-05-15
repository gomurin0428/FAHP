namespace FAHP.Shared.Models
{
    /// <summary>
    /// 1 つの基準に対する候補同士のペアワイズ比較エントリ。
    /// </summary>
    public sealed class AlternativeComparisonEntry
    {
        /// <summary>
        /// 対象となる基準名。
        /// </summary>
        public string Criterion { get; set; }

        public string AlternativeA { get; set; }
        public string AlternativeB { get; set; }

        /// <summary>
        /// サトティ 1–9 スケール。AlternativeA が AlternativeB よりどれだけ優れているか。
        /// </summary>
        public int Value { get; set; } = 1;
        
        public AlternativeComparisonEntry(string criterion, string alternativeA, string alternativeB)
        {
            Criterion = criterion;
            AlternativeA = alternativeA;
            AlternativeB = alternativeB;
        }
        
        public AlternativeComparisonEntry() { }
    }
}     