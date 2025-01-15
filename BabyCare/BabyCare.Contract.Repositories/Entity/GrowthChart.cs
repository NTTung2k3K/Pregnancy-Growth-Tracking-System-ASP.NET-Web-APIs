using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class GrowthChart : BaseEntity
    {
        public string Status { get; set; }
        public bool IsShared { get; set; } = false;
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public string? Question { get; set; }
        public int ViewCount { get; set; }

        public virtual ICollection<FetalGrowthRecord> FetalGrowthRecords { get; set; }

        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }

}
