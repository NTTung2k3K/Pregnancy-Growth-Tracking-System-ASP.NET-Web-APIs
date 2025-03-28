﻿using Microsoft.AspNetCore.Http;

namespace BabyCare.ModelViews.AppointmentTemplateModelViews.Request
{
    public class UpdateATRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DaysFromBirth { get; set; }
        public decimal Fee { get; set; }

        public int Status { get; set; }
        public string Description { get; set; }
        public IFormFile?  Image { get; set; }
    }
}
