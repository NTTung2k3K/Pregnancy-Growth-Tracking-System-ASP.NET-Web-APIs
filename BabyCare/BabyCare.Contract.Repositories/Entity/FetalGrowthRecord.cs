using BabyCare.Core.Base;
using System.ComponentModel.DataAnnotations.Schema;

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
        [ForeignKey("FetalGrowthStandard")]
        public int? FetalGrowthStandardId { get; set; }
        public float? HeadCircumference { get; set; }
        public float? AbdominalCircumference { get; set; }
        public int? FetalHeartRate { get; set; }

      

        public string? HealthCondition { get; set; }

        public virtual Child Child { get; set; }
        public virtual FetalGrowthStandard FetalGrowthStandard { get; set; }

        public virtual Alert? Alert { get; set; }
    }

}
