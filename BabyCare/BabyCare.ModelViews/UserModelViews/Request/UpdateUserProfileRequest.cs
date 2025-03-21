﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.UserModelViews.Request
{
    public class UpdateUserProfileRequest
    {
        public Guid Id { get; set; }
        public IFormFile? Image { get; set; }
        public string? FullName { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public int? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
