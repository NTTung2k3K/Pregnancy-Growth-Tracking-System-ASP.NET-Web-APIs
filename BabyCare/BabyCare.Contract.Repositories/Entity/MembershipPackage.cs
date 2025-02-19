using BabyCare.Core.Base;

namespace BabyCare.Contract.Repositories.Entity
{
    public class MembershipPackage : BaseEntity
    {
        public string PackageName { get; set; }
        public string? Description { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Duration { get; set; }
        public int? Status { get; set; }
        public int? PackageLevel { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Price { get; set; }

        public int? ShowPriority { get; set; }


        public int MaxRecordAdded { get; set; } // -1 = Unlimited
        public int MaxAppointmentCanBooking { get; set; } // -1 = Unlimited

        public int MaxGrowthChartShares { get; set; } // -1 = Unlimited
        public bool HasGenerateAppointments { get; set; }   // 0 nếu không hỗ trợ
        public bool HasStandardDeviationAlerts { get; set; }
        public bool HasViewGrowthChart { get; set; }


        public virtual ICollection<UserMembership> UserMemberships { get; set; }
    }

}
