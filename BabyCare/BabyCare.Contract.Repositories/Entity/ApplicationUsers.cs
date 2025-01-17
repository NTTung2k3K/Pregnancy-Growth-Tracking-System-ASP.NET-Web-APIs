﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabyCare.Core.Utils;

namespace BabyCare.Contract.Repositories.Entity
{
    public class ApplicationUsers : IdentityUser<Guid>
    {
        public string? FullName { get; set; }
        public string? Image { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public DateTime? DueDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
        public string? RefreshToken { get; set; }
        public DateTimeOffset RefreshTokenExpiryTime { get; set; }
        

        public ApplicationUsers()
        {
            CreatedTime = CoreHelper.SystemTimeNow;
            LastUpdatedTime = CreatedTime;
        }
        public virtual ICollection<ApplicationUserRoles> UserRoles { get; set; }

        public virtual ICollection<UserMembership> UserMemberships { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }

        public virtual ICollection<Blog> Blogs { get; set; }

        public virtual ICollection<Child> Children { get; set; }

        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }
}
