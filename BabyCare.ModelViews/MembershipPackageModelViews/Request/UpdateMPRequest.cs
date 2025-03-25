using Microsoft.AspNetCore.Http;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.ModelViews.MembershipPackageModelViews.Request
{
    public class UpdateMPRequest
    {
        public int Id { get; set; }
        public string PackageName { get; set; }
        public string? Description { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Duration { get; set; }
        public int? Status { get; set; }
        public PackageLevel? PackageLevel { get; set; }
        public IFormFile? ImageUrl { get; set; }
        public decimal? Discount { get; set; }
        public int? ShowPriority { get; set; }

        public int MaxRecordAdded { get; set; } // -1 = Unlimited

        public int MaxGrowthChartShares { get; set; } // -1 = Unlimited
        public bool HasGenerateAppointments { get; set; }   // 0 nếu không hỗ trợ
        public bool HasStandardDeviationAlerts { get; set; }
        public bool HasViewGrowthChart { get; set; }
        public int MaxAppointmentCanBooking { get; set; } // -1 = Unlimited

    }
}
