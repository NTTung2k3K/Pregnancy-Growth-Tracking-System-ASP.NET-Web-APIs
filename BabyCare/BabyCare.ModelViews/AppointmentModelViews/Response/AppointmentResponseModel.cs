using BabyCare.ModelViews.AppointmentTemplateModelViews.Response;
using BabyCare.ModelViews.ChildModelView;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.AppointmentModelViews.Response
{
    public class AppointmentResponseModel
    {
        public int Id { get; set; }
        public int AppointmentSlot { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public string? Status { get; set; }
        public decimal? Fee { get; set; }
        public string? Notes { get; set; }
        public virtual UserModelViews.Response.UserResponseModel User { get; set; }
        public virtual List<UserModelViews.Response.EmployeeResponseModel> Doctors { get; set; }
        public virtual ATResponseModel AppointmentTemplate { get; set; }
        public virtual List<ChildModelView.ChildModelView> Childs {  get; set; }

        //public virtual Reminder Reminder { get; set; }
    }
}
