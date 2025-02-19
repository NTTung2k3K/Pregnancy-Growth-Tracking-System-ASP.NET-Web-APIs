using Microsoft.AspNetCore.Http;

namespace BabyCare.ModelViews.BlogTypeModelView
{
    public class UpdateBlogTypeModelView
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Thumbnail { get; set; }
    }
}
