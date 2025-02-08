using BabyCare.ModelViews.AppointmentTemplateModelViews.Response;
using BabyCare.ModelViews.ChildModelView;
using BabyCare.ModelViews.UserModelViews.Response;
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
        public DateTime AppointmentDate { get; set; }
        public string? Status { get; set; }
        public decimal? Fee { get; set; }
        public string? Notes { get; set; }
        public string? Result { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; }

        public virtual UserModelViews.Response.UserResponseModel User { get; set; }
        public virtual List<UserModelViews.Response.EmployeeResponseModel> Doctors { get; set; }
        public virtual ATResponseModel AppointmentTemplate { get; set; }
        public virtual List<ChildModelView.ChildModelView> Childs { get; set; }

        //public virtual Reminder Reminder { get; set; }
    }
    public class AppointmentResponseModelV2
    {
        public int Id { get; set; }
        public int AppointmentSlot { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Status { get; set; }
        public decimal? Fee { get; set; }
        public string? Notes { get; set; }
        public string? Result { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; }

        public virtual UserModelViews.Response.UserResponseModel User { get; set; }
        public virtual List<AppoinmentUserResponseModel> AppoinmentUsers { get; set; }
        public virtual ATResponseModel AppointmentTemplate { get; set; }
        public virtual List<ChildModelViewAddeRecords> Childs {  get; set; }
         
        //public virtual Reminder Reminder { get; set; }
    }
    public class AppoinmentUserResponseModel
    {
        public EmployeeResponseModel Doctor { get; set; }
        public DateTime? AssignedTime { get; set; }
        public Guid AssignedBy { get; set; }
        public string? Description { get; set; }
    }
    public class ChildModelViewAddeRecords : ChildModelView.ChildModelView
    {
        public List<FetalGrowthRecordModelViewAddedStandards>  FetalGrowthRecordModelViews { get; set; }
    }
    public class FetalGrowthRecordModelViewAddedStandards : FetalGrowthRecordModelView.FetalGrowthRecordModelView
    {
       public FetalGrowthStandardModelView.FetalGrowthStandardModelView FetalGrowthStandardModelView { get; set; }
    }
}
