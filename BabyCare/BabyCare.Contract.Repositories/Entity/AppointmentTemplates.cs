using BabyCare.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Repositories.Entity
{
    public class AppointmentTemplates : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }   
        public int DaysFromBirth {  get; set; }
        public string Description { get; set; }
        public string Image {  get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }

    }
}
