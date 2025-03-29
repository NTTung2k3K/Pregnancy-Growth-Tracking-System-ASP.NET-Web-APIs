using Microsoft.AspNetCore.Http;

namespace BabyCare.ModelViews.BlogModelViews
{
    public class UpdateBlogModelView
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public Guid? AuthorId { get; set; }
        //public int? LikesCount { get; set; }
        //public int? ViewCount { get; set; }
        public int? Week { get; set; }

        public int? Status { get; set; }
        public string? Sources { get; set; }
        public IFormFile? Thumbnail { get; set; }
        public int? BlogTypeId { get; set; }
        //public bool? IsFeatured { get; set; }
    }
}
