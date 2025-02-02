using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.ModelViews.BlogModelViews
{
    public class CreateBlogModelView
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "AuthorId is required.")]
        public Guid AuthorId { get; set; }

        public int? LikesCount { get; set; } = 0;

        public int? Week { get; set; }
        public int? ViewCount { get; set; } = 0;

        public string? Status { get; set; }

        public string? Sources { get; set; }

        public IFormFile Thumbnail { get; set; }

        [Required(ErrorMessage = "BlogTypeId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BlogTypeId must be a positive integer.")]
        public int BlogTypeId { get; set; }

        public bool IsFeatured { get; set; }
    }
}
