using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.AuthModelViews.Request
{
    public class UserLoginGoogleRequest
    {
        public string Email { get; set; } = string.Empty;
        public bool Email_verified { get; set; }
        public string Family_name { get; set; } = string.Empty;
        public string Given_name { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string Sub { get; set; } = string.Empty;

    }
}
