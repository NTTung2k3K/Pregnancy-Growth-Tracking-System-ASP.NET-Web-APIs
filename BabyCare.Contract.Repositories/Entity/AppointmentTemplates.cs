using BabyCare.Core.Base;

namespace BabyCare.Contract.Repositories.Entity
{
    public class AppointmentTemplates : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }   
        public int DaysFromBirth {  get; set; }
        public int Status { get; set; }
        public decimal? Fee { get; set; }

        public string Description { get; set; }
        public string Image {  get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }

    }
}
