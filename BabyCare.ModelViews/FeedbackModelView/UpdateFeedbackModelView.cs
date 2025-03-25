namespace BabyCare.ModelViews.FeedbackModelView
{
    public class UpdateFeedbackModelView
    {
        public string? Description { get; set; }
        public int? GrowthChartsID { get; set; }
        public int? ParentFeedbackID { get; set; }
        public int? Rating { get; set; }
        public string? FeedbackType { get; set; }
        public string? Status { get; set; }
        public bool? IsAnonymous { get; set; }
    }
}
