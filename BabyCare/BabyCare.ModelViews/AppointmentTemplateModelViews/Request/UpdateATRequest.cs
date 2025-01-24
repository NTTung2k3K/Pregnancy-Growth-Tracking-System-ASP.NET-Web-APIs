using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.ModelViews.AppointmentTemplateModelViews.Request
{
    public class UpdateATRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DaysFromBirth { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public IFormFile  Image { get; set; }
    }
}
