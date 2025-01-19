using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.MembershipPackageModelViews.Response
{
    public class MPStatusHandleResponseModel
    {
        public List<MPStatusResponseModel> Status { get; set; }
        public List<MPStatusResponseModel> PackageLevel { get; set; }

    }
    public class MPStatusResponseModel
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }
}
