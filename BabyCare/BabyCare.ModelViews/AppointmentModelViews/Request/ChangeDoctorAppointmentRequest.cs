using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
    public class ChangeDoctorAppointmentRequest
    {
        public Guid DoctorId { get; set; }
        public int AppointmentId { get; set; }
        public string Reason { get; set; }
    }
}
