namespace BabyCare.ModelViews.GrowthChartModelView
{
    public class CreateGrowthChartModelView
    {
        public string Status { get; set; }
        public bool IsShared { get; set; } = false;
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public string? Question { get; set; }
        public int ViewCount { get; set; }
    }
}
