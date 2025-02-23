using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.ModelViews.MembershipPackageModelViews.Request
{
    public class CreateMPRequest
    {
        public string PackageName { get; set; }
        public string? Description { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Duration { get; set; }
        public PackageStatus? Status { get; set; }
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
