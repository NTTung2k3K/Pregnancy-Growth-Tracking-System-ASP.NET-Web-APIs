using Microsoft.AspNetCore.Http;

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
