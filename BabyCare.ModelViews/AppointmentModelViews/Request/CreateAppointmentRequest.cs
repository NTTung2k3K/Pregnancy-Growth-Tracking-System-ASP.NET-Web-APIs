
namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
    public class CreateAppointmentRequest
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        // BUG: make it be a list 
        public List<int> ChildIds { get; set; }
        public int AppointmentTemplateId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int AppointmentSlot { get; set; }
        public string? Notes { get; set; }
        public bool IsDoctorCreate { get; set; }
        public string? Description { get; set; }

    }
}
