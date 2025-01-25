namespace BabyCare.ModelViews.FeedbackModelView
{
    public class FeedbackModelView
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; }
        public int GrowthChartsID { get; set; }
        public string GrowthChartName { get; set; } // Assuming you want to include the name of the GrowthChart
        public int? ParentFeedbackID { get; set; }
        public string ParentFeedbackDescription { get; set; } // For referencing the parent feedback's description
        public int Rating { get; set; }
        public string? FeedbackType { get; set; }
        public string? Status { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string CreatedBy { get; set; }
    }
}
