using Microsoft.AspNetCore.Http;

namespace BabyCare.ModelViews.UserModelViews.Request
{
    public class CreateEmployeeRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public int Gender { get; set; }
        public IFormFile? Image { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public Guid RoleId { get; set; }
    }
}
