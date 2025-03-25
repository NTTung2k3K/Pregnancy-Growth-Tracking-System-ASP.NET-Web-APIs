using BabyCare.Core.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BabyCare.Contract.Repositories.Entity
{
    public class Child : BaseEntity
    {
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]

        public string Name { get; set; }
        public int? FetalGender { get; set; }
        public DateTime DueDate { get; set; }

        public string? PhotoUrl { get; set; }
        public string? BloodType { get; set; }
        //public int? PregnancyWeekAtBirth { get; set; }
        public bool IsGenerateSampleAppointments { get; set; }

        public virtual ApplicationUsers User { get; set; }
        public virtual ICollection<GrowthChart> GrowthCharts { get; set; }

        public virtual ICollection<AppointmentChild> AppointmentChildren { get; set; }

        public virtual ICollection<FetalGrowthRecord>? FetalGrowthRecords { get; set; }
    }

}
