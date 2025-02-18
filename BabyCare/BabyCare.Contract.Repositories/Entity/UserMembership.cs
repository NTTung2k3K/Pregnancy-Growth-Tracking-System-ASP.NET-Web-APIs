using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class UserMembership : BaseEntity
    {
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]

        public int PackageId { get; set; }

        [ForeignKey("PackageId")]

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
        public bool IsActive => Status == "Active" && DateTime.Now <= EndDate;
        public int GrowthChartShareCount { get; set; } = 0;
        public int AppointmentBookingCount { get; set; } = 0;

        public int AddedRecordCount { get; set; } = 0;

        public virtual ApplicationUsers User { get; set; }
        public virtual MembershipPackage Package { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
    }

}
