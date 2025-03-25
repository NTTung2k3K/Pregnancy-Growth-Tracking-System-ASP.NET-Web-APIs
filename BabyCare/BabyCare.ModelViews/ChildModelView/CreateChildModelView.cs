using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.ChildModelView
{
    public class CreateChildModelView
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        

        //[MaxLength(10, ErrorMessage = "Fetal gender cannot exceed 10 characters.")]
        public int? FetalGender { get; set; }

        [MaxLength(50, ErrorMessage = "Pregnancy stage cannot exceed 50 characters.")]
        public string? PregnancyStage { get; set; }


        [Required]
        public DateTime DueDate { get; set; }

        [MaxLength(100, ErrorMessage = "Delivery plan cannot exceed 100 characters.")]
        public string? DeliveryPlan { get; set; }

        [MaxLength(200, ErrorMessage = "Complications cannot exceed 200 characters.")]
        public string? Complications { get; set; }
        public IFormFile? PhotoUrl { get; set; }

        [MaxLength(3, ErrorMessage = "Blood type cannot exceed 3 characters.")]
        public string? BloodType { get; set; }

    


        public bool IsGenerateSampleAppointments {  get; set; }
    }

}
