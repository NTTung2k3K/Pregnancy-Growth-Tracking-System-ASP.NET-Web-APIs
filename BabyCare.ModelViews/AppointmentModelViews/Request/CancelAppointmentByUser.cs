
namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
    public class CancelAppointmentByUser
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
    }
}
