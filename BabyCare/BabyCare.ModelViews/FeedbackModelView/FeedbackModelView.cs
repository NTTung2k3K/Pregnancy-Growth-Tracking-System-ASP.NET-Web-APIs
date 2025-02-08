using BabyCare.ModelViews.GrowthChartModelView;
using BabyCare.ModelViews.UserModelViews.Response;
using Firebase.Auth;

namespace BabyCare.ModelViews.FeedbackModelView
{

    public class FeedbackModelViewForAdmin
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public string Status { get; set; }
        public string? FeedbackType { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
        public UserModelViews.Response.UserResponseModel UserResponseModel { get; set; }
        public GrowthChartModelView.GrowthChartModelView GrowthChartModelView { get; set; }
    }
        public class FeedbackModelView
    {
        public int Id { get; set; }
        //public Guid UserId { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public string? FeedbackType { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreatedTime { get; set; }

        public UserResponseModel User { get; set; }
        //public FeedbackModelView? ResponseFeedback { get; set; }

        public List<FeedbackModelView> ResponseFeedbacks { get; set; } = new List<FeedbackModelView>();
    }
}
