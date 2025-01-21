using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.BlogModelViews
{
    public class UpdateBlogModelView
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public Guid? AuthorId { get; set; }
        public int? LikesCount { get; set; }
        public int? ViewCount { get; set; }
        public string? Status { get; set; }
        public string? Sources { get; set; }
        public string? Thumbnail { get; set; }
        public int? BlogTypeId { get; set; }
        public bool? IsFeatured
        {
            get; set;
        }
    }
}
