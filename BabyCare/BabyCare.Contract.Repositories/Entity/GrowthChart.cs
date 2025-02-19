using BabyCare.Core.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BabyCare.Contract.Repositories.Entity
{
    public class GrowthChart : BaseEntity
    {
        public int Status { get; set; }
        public string Question { get; set; }
        public string Topic { get; set; }

        public int ViewCount { get; set; }


        [ForeignKey("ChildId")]
        public int ChildId { get; set; }
        public virtual Child Child { get; set; }

        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }

}
