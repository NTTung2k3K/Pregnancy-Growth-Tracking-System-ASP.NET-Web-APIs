
namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
    public class UpdateAppointmentRequest
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int ChildId { get; set; }
        public string Name { get; set; }
        public decimal Fee { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int Status { get; set; }
        public int AppointmentSlot { get; set; }
        public int AppointmentTemplateId { get; set; }
        public string? Notes { get; set; }
        public bool IsDoctorUpdate { get; set; }
        public string? Result { get; set; }
        public string? Description { get; set; }
    }
}
