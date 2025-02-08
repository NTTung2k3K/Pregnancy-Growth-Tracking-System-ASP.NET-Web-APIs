using BabyCare.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.ModelViews.UserModelViews.Request
{
    public class UpdateUserStatusRequest
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
    }
}
