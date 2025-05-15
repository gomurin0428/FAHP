namespace FAHP.Shared.ViewModels
{
    public sealed class WeightResultViewModel
    {
        public string Criterion { get; set; }
        public double Weight { get; set; }
        
        public WeightResultViewModel(string criterion, double weight)
        {
            Criterion = criterion;
            Weight = weight;
        }
        
        public WeightResultViewModel() { }
    }
}     