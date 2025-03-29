using BabyCare.Core.Base;

namespace BabyCare.Contract.Repositories.Entity
{
    public class FetalGrowthStandard : BaseEntity
    {
        public int Week { get; set; }
        public int Gender { get; set; }
        public string? GestationalAge { get; set; }
        public float MinWeight { get; set; }
        public float MaxWeight { get; set; }
        public float AverageWeight { get; set; }
        public float MinHeight { get; set; }
        public float MaxHeight { get; set; }
        public float AverageHeight { get; set; }
        public float HeadCircumference { get; set; }
        public float AbdominalCircumference { get; set; }
        public int? FetalHeartRate { get; set; }


        public virtual ICollection<FetalGrowthRecord> FetalGrowthRecords { get; set; }
    }

}
