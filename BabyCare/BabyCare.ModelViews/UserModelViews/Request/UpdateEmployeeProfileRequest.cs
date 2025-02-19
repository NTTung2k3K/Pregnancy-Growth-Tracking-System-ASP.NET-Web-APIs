using Microsoft.AspNetCore.Http;

namespace BabyCare.ModelViews.UserModelViews.Request
{
    public class UpdateEmployeeProfileRequest
    {
        public Guid Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }


        public IFormFile? Image { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public int Gender { get; set; }

    }
}
