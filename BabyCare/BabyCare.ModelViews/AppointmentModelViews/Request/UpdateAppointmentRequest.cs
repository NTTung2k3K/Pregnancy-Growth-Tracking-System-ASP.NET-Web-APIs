using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
    public class UpdateAppointmentRequest
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int ChildId { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public int Status { get; set; }
        public int AppointmentSlot { get; set; }
        public string? AppointmentType { get; set; }
        public string? Notes { get; set; }
    }
}
