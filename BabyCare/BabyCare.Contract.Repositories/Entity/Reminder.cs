using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class Reminder : BaseEntity
    {
        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]

        public string? ReminderType { get; set; }
        public DateTime ReminderDate { get; set; }
        public DateTime? SentDate { get; set; }
        public bool IsSent { get; set; } = false;

        public virtual Appointment Appointment { get; set; }
    }

}
