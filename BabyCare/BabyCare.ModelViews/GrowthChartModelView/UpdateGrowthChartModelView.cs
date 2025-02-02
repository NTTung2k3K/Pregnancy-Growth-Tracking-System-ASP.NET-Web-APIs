namespace BabyCare.ModelViews.GrowthChartModelView
{
    public class UpdateGrowChartByAdmin
    {
        public int GrowthChartId { get; set; }

        public int Status { get; set; }
    }
    public class UpdateGrowChartByUser
    {
        public int GrowthChartId { get; set; }
        public Guid UserId { get; set; }
        public int Status { get; set; }
    }
    public class UpdateGrowthChartModelView
    {
        public string Status { get; set; }
        public bool IsShared { get; set; } = false;
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public string? Question { get; set; }
        public int ViewCount { get; set; }
    }
}
