using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.ModelViews.AppointmentTemplateModelViews.Request
{
    public class CreateATRequest
    {
        public string Name { get; set; }
        public int DaysFromBirth { get; set; }
        public string Description { get; set; }
        public IFormFile? Image { get; set; }

    }
}
