using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.MembershipPackageModelViews.Response
{
    public class VNPayCallbackResponse
    {
        public int PackageId { get; set; }
        public Guid UserId { get; set; }
        public string PaymentMethod { get; set; }
        public bool IsSuccess { get; set; }
        public string Description { get; set; }

    }
}
