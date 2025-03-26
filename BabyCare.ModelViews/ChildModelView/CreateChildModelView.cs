using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

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


        [Required]
        public DateTime DueDate { get; set; }
        public IFormFile? PhotoUrl { get; set; }

        public string? BloodType { get; set; }


        public bool IsGenerateSampleAppointments {  get; set; }
    }

}
