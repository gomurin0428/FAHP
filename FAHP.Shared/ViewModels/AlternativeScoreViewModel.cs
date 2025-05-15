namespace FAHP.Shared.ViewModels
{
    public sealed class AlternativeScoreViewModel
    {
        public string Alternative { get; set; }
        public double Score { get; set; }
        
        public AlternativeScoreViewModel(string alternative, double score)
        {
            Alternative = alternative;
            Score = score;
        }
        
        public AlternativeScoreViewModel() { }
    }
}     