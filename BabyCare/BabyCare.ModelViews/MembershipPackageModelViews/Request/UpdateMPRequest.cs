using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.ModelViews.MembershipPackageModelViews.Request
{
    public class UpdateMPRequest
    {
        public int Id { get; set; }
        public string PackageName { get; set; }
        public string? Description { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Duration { get; set; }
        public PackageStatus? Status { get; set; }
        public PackageLevel? PackageLevel { get; set; }
        public IFormFile? ImageUrl { get; set; }
        public decimal? Discount { get; set; }
        public int? ShowPriority { get; set; }
    }
}
