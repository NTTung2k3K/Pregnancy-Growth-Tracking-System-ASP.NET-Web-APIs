using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
    public class CreateAppointmentRequest
    {
        public Guid UserId { get; set; }
        public int ChildId { get; set; }
        public int AppointmentTemplateId { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public int AppointmentSlot { get; set; }
        public string? Notes { get; set; }

    }
}
