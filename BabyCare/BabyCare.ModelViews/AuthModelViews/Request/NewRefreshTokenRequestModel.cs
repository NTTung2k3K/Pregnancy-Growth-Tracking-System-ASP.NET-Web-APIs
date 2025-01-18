using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.AuthModelViews.Request
{
    public class NewRefreshTokenRequestModel
    {
        public Guid Id { get; set; }
        public string RefreshToken { get; set; }

    }
}
