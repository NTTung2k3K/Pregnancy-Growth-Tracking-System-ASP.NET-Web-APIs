namespace BabyCare.ModelViews.GrowthChartModelView
{
    public class GrowthChartModelView
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public bool IsShared { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public string? Question { get; set; }
        public int ViewCount { get; set; }
    }
}
