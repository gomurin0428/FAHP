namespace FAHP.Shared.Models
{
    /// <summary>
    /// 2つの基準間の重要度比較エントリ。
    /// </summary>
    public sealed class ComparisonEntry
    {
        public string CriterionA { get; set; }
        public string CriterionB { get; set; }
        /// <summary>
        /// サトティスケール (1-9)。CriterionA が CriterionB よりどれだけ重要かを表す。
        /// </summary>
        public int Value { get; set; } = 1;
        
        public ComparisonEntry(string criterionA, string criterionB)
        {
            CriterionA = criterionA;
            CriterionB = criterionB;
        }
        
        public ComparisonEntry() { }
    }
}     