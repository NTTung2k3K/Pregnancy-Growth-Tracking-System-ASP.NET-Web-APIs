using BabyCare.ModelViews.FeedbackModelView;

namespace BabyCare.ModelViews.GrowthChartModelView
{
    public class GrowthChartModelView
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Topic { get; set; }
        public DateTimeOffset CreatedTime { get; set; }


        public string? Question { get; set; }
        public int ViewCount { get; set; } 
        public AppointmentModelViews.Response.ChildModelViewAddeRecords childModelView { get; set; }
        public List<FeedbackModelView.FeedbackModelView> feedbackModelViews { get; set; }
    }
}
