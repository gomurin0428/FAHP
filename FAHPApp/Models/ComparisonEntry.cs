namespace FAHPApp.Models
{
    /// <summary>
    /// 2つの基準間の重要度比較エントリ。
    /// </summary>
    public sealed class ComparisonEntry
    {
        public required string CriterionA { get; init; }
        public required string CriterionB { get; init; }
        /// <summary>
        /// サトティスケール (1-9)。CriterionA が CriterionB よりどれだけ重要かを表す。
        /// </summary>
        public int Value { get; set; } = 1;
    }
} 