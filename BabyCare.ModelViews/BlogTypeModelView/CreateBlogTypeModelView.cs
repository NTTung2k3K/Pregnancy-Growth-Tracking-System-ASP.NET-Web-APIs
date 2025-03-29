
namespace BabyCare.ModelViews.BlogTypeModelView
{
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel.DataAnnotations;

    public class CreateBlogTypeModelView
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; }

        public string? Description { get; set; }

        public IFormFile? Thumbnail { get; set; }
    }

}
