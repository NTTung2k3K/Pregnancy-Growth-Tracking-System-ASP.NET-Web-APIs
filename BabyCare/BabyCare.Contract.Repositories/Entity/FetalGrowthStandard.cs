using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class FetalGrowthStandard : BaseEntity
    {
        public string? GestationalAge { get; set; }
        public float MinWeight { get; set; }
        public float MaxWeight { get; set; }
        public float AverageWeight { get; set; }
        public float MinHeight { get; set; }
        public float MaxHeight { get; set; }
        public float AverageHeight { get; set; }
        public float HeadCircumference { get; set; }
        public float AbdominalCircumference { get; set; }

        public virtual ICollection<FetalGrowthRecord> FetalGrowthRecords { get; set; }
    }

}
