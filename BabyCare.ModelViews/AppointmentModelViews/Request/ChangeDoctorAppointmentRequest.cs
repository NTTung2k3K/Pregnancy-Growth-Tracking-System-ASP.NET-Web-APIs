
namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
    public class ChangeDoctorAppointmentRequest
    {
        public Guid DoctorId { get; set; }
        public int AppointmentId { get; set; }
        public string Reason { get; set; }
    }
}
