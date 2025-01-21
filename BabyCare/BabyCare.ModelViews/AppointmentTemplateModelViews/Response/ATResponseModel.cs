using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.AppointmentTemplateModelViews.Response
{
    public class ATResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DaysFromBirth { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Image { get; set; }
    }
}
