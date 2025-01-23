using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class Child : BaseEntity
    {
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]

        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? FetalGender { get; set; }
        public string? PregnancyStage { get; set; }
        public float? WeightEstimate { get; set; }
        public float? HeightEstimate { get; set; }
        public DateTime DueDate { get; set; }

        public string? DeliveryPlan { get; set; }
        public string? Complications { get; set; }
        public string? PhotoUrl { get; set; }
        public string? BloodType { get; set; }
        public string? PregnancyWeekAtBirth { get; set; }
        public bool IsGenerateSampleAppointments { get; set; }

        public virtual ApplicationUsers User { get; set; }

        public virtual ICollection<AppointmentChild> AppointmentChildren { get; set; }

        public virtual ICollection<FetalGrowthRecord> FetalGrowthRecords { get; set; }
    }

}
