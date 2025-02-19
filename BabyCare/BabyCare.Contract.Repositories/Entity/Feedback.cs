using BabyCare.Core.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BabyCare.Contract.Repositories.Entity
{
    public class Feedback : BaseEntity
    {
        [ForeignKey("UserId")]
        public Guid UserId { get; set; }


        public string Description { get; set; }
        public int GrowthChartsID { get; set; }
        public int? ResponseFeedbackId { get; set; }

        public int Rating { get; set; }
        public string? FeedbackType { get; set; }
        public int Status { get; set; }
        public bool IsAnonymous { get; set; } = false;

        public virtual ApplicationUsers User { get; set; }
        public virtual GrowthChart GrowthChart { get; set; }
        public virtual Feedback? ResponseFeedback { get; set; }
        public virtual ICollection<Feedback> ResponseFeedbacks { get; set; } = new HashSet<Feedback>();

    }

}
