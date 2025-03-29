using Microsoft.AspNetCore.Identity;
using BabyCare.Core.Utils;

namespace BabyCare.Contract.Repositories.Entity
{
    public class ApplicationUsers : IdentityUser<Guid>
    {
        public string? FullName { get; set; }
        public string? Image { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public int? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? LastUpdatedBy { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
        public string? RefreshToken { get; set; }
        public DateTimeOffset RefreshTokenExpiryTime { get; set; }
        public int? Status { get; set; }

        

        public ApplicationUsers()
        {
            CreatedTime = CoreHelper.SystemTimeNow;
            LastUpdatedTime = CreatedTime;
        }
        public virtual ICollection<ApplicationUserRoles> UserRoles { get; set; }

        public virtual ICollection<UserMembership> UserMemberships { get; set; }

        public virtual ICollection<Blog> Blogs { get; set; }

        public virtual ICollection<Child> Children { get; set; }

        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<AppointmentUser> AppointmentUsers { get; set; }

    }
}
