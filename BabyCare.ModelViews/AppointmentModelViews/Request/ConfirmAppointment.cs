﻿
namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
    public class ConfirmAppointment
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int AppointmentSlot { get; set; }
        public string? Notes { get; set; }
        public string? Description { get; set; }
    }
}
