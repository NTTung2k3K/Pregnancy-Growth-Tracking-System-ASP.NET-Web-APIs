using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.BlogTypeModelView
{
    public class UpdateBlogTypeModelView
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Thumbnail { get; set; }
    }
}
