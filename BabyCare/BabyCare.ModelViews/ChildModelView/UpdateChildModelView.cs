using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.ChildModelView
{
    public class UpdateChildModelView
    {
        public Guid? UserId { get; set; }
        public string? Name { get; set; }
        public int? FetalGender { get; set; }
        public string? PregnancyStage { get; set; }
        public float? WeightEstimate { get; set; }
        public float? HeightEstimate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? DeliveryPlan { get; set; }
        public string? Complications { get; set; }
        public IFormFile? PhotoUrl { get; set; }
        public string? BloodType { get; set; }
        public int? PregnancyWeekAtBirth { get; set; }
        public bool IsGenerateSampleAppointments { get; set; }


    }
}
