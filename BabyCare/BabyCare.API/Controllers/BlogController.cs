using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.BlogModelViews;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        /// <summary>
        ///     Get all blogs with pagination and optional filters
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<BasePaginatedList<BlogModelView>>> GetAllBlogs
            ([FromQuery] int? id, [FromQuery] string? title, [FromQuery] string? status, [FromQuery] bool? isFeatured, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var result = await _blogService.GetAllBlogAsync(pageNumber, pageSize, id, title, status, isFeatured);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Get a blog by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogModelView>> GetBlogById(int id)
        {
            try
            {
                var result = await _blogService.GetBlogByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Create a new blog
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<string>> CreateBlog([FromQuery] CreateBlogModelView model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _blogService.AddBlogAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Update a blog
        /// </summary>
        [HttpPut("update/{id}")]
        public async Task<ActionResult<string>> UpdateBlog(int id, [FromQuery] UpdateBlogModelView model)
        {
            try
            {
                var result = await _blogService.UpdateBlogAsync(id, model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Delete a blog
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<string>> DeleteBlog(int id)
        {
            try
            {
                var result = await _blogService.DeleteBlogAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
    }
}
