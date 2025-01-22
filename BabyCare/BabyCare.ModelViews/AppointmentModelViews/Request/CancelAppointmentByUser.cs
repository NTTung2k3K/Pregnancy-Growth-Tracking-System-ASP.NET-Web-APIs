using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
    public class CancelAppointmentByUser
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
    }
}
