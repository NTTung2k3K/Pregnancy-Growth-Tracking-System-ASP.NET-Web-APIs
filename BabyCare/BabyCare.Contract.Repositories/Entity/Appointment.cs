using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class Appointment : BaseEntity
    {
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]

        public Guid DoctorId { get; set; }

        public string? ClinicAddress { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? AppointmentType { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public int? Duration { get; set; }
        public decimal? Fee { get; set; }
        public bool IsUrgent { get; set; } = false;

        public virtual ApplicationUsers User { get; set; }
        public virtual ApplicationUsers Doctor { get; set; }

        public virtual Reminder Reminder { get; set; }

        public virtual ICollection<AppointmentChild> AppointmentChildren { get; set; }
    }

}
