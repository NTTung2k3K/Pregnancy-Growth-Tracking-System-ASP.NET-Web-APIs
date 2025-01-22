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
      
        [ForeignKey("AppointmentTemplateId")]
        public int AppointmentTemplateId { get; set; }
        public int AppointmentSlot { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Name { get; set; }
        public int? Status { get; set; }
        public decimal? Fee { get; set; }
        public string? Notes { get; set; }
        public virtual AppointmentTemplates AppointmentTemplate { get; set; }
        public virtual ICollection<AppointmentUser> AppointmentUsers { get; set; }

        public virtual Reminder Reminder { get; set; }

        public virtual ICollection<AppointmentChild> AppointmentChildren { get; set; }
    }

}
