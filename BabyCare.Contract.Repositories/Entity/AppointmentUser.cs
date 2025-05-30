﻿using BabyCare.Core.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BabyCare.Contract.Repositories.Entity
{
    public class AppointmentUser : BaseEntity
    {


        public int AppointmentId { get; set; }

        public virtual Appointment Appointment { get; set; }

        public Guid? DoctorId { get; set; }
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
