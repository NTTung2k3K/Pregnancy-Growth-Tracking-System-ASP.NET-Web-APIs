using BabyCare.Core.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BabyCare.Contract.Repositories.Entity
{
    public class Payment : BaseEntity
    {
        public int MembershipId { get; set; }

        [ForeignKey("MembershipId")]

        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string? Status { get; set; }

        public virtual UserMembership Membership { get; set; }
    }

}
