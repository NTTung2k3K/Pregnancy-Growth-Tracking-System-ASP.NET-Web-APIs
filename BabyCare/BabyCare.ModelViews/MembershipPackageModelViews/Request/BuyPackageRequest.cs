using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.MembershipPackageModelViews.Request
{
    public class BuyPackageRequest
    {
        public Guid UserId { get; set; }
        public int PackageId { get; set; }
    }
}
