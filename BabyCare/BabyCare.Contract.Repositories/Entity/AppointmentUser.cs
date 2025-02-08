using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class AppointmentUser : BaseEntity
    {


        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }

        public Guid? DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public virtual ApplicationUsers? Doctor { get; set; }


        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUsers User { get; set; }

        public DateTime? AssignedTime { get; set; }
        public Guid AssignedBy { get; set; }
        public string? Description { get; set; }

        public string? Reason { get; set; }
    }
}
