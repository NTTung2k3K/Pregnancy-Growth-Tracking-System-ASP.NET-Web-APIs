using Microsoft.AspNetCore.Http;

namespace BabyCare.ModelViews.ChildModelView
{
    public class UpdateChildModelView
    {
        public Guid? UserId { get; set; }
        public string? Name { get; set; }
        public int? FetalGender { get; set; }
        public DateTime? DueDate { get; set; }
        public IFormFile? PhotoUrl { get; set; }
        public string? BloodType { get; set; }
        public bool IsGenerateSampleAppointments { get; set; }


    }
}
