using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class FetalGrowthRecord : BaseEntity
    {
        public int ChildId { get; set; }

        [ForeignKey("ChildId")]

        public int WeekOfPregnancy { get; set; }
        public float Weight { get; set; }
        public float Height { get; set; }
        public DateTime RecordedAt { get; set; }
        public int? GrowChartsID { get; set; }
        public float? HeadCircumference { get; set; }
        public float? AbdominalCircumference { get; set; }
        public int? FetalHeartRate { get; set; }

        [ForeignKey("GrowChartsID")]

        public string? HealthCondition { get; set; }

        public virtual Child Child { get; set; }
        public virtual FetalGrowthStandard FetalGrowthStandard { get; set; }
        public virtual GrowthChart? GrowthChart { get; set; }

        public virtual Alert? Alert { get; set; }
    }

}
