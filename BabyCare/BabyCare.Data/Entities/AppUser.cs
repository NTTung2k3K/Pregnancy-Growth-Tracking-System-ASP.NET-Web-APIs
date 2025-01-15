using Microsoft.AspNetCore.Identity;

namespace TKFamilyFinance.API.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }

    }
}
