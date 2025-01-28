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
    public class AvailableSlotResponseModel
    {
        public DateTime Date { get; set; }
        public List<int> Slots { get; set; }
    }
}
