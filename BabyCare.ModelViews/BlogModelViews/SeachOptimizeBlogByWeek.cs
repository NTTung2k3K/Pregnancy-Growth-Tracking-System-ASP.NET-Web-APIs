using BabyCare.ModelViews.AppointmentModelViews.Request;

namespace BabyCare.ModelViews.BlogModelViews
{
    public class SeachOptimizeBlogByWeek : SearchOptimizeRequest
    {
        public int? BlogTypeId { get; set; }
        public int? Week { get; set; }
    }
}
