using BabyCare.Core.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BabyCare.Contract.Repositories.Entity
{
    public class AppointmentChild : BaseEntity
    {
        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]

        public int? ChildId { get; set; }

        [ForeignKey("ChildId")]

        public string? Description { get; set; }

        public virtual Appointment Appointment { get; set; }
        public virtual Child Child { get; set; }
    }

}
