using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using BabyCare.ModelViews.UserModelViews.Response;

namespace BabyCare.ModelViews.UserMembershipModelView.Response
{
    public class UserMembershipResponse
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
        public int GrowthChartShareCount { get; set; }
        public int AddedRecordCount { get; set; }

        public UserResponseModel User { get; set; }
        public MPResponseModel Package { get; set; }

    }
}
