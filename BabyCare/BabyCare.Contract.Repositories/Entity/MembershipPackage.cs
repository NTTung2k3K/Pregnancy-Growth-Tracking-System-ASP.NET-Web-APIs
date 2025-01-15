using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class MembershipPackage : BaseEntity
    {
        public string PackageName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string? Status { get; set; }
        public string? PackageLevel { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Discount { get; set; }
        public int? ShowPriority { get; set; }

        public virtual ICollection<UserMembership> UserMemberships { get; set; }
    }

}
