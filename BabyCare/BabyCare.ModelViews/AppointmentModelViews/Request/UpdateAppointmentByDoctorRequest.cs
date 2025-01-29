using BabyCare.ModelViews.ChildModelView;
using BabyCare.ModelViews.FetalGrowthRecordModelView;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.AppointmentModelViews.Request
{
   
    public class UpdateAppointmentByDoctorRequest
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public List<CreateFetalGrowthRecordModelView> ChildsUpdated { get; set; }
        public string Name { get; set; }
        public decimal Fee { get; set; }
        public int Status { get; set; }
        public string? Notes { get; set; }
        public string? Result { get; set; }
        public string? Description { get; set; }

    }
}
