using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.BlogTypeModelView;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogTypeController : ControllerBase
    {
        private readonly IBlogTypeService _blogTypeService;

        public BlogTypeController(IBlogTypeService blogTypeService)
        {
            _blogTypeService = blogTypeService;
        }

        /// <summary>
        ///     Get all blog types with pagination and optional filters
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<BasePaginatedList<BlogTypeModelView>>> GetAllBlogTypes
            ([FromQuery] int? id, [FromQuery] string? name, int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                var result = await _blogTypeService.GetAllBlogTypeAsync(pageNumber, pageSize, id, name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Get a blog type by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogTypeModelView>> GetBlogTypeById(int id)
        {
            try
            {
                var blogType = await _blogTypeService.GetBlogTypeByIdAsync(id);
                return Ok(blogType);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Create a new blog type
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<string>> CreateBlogType([FromForm] CreateBlogTypeModelView model)
        {
            try
            {
                var result = await _blogTypeService.AddBlogTypeAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Update a blog type
        /// </summary>
        [HttpPut("update/{id}")]
        public async Task<ActionResult<string>> UpdateBlogType(int id, [FromForm] UpdateBlogTypeModelView model)
        {
            try
            {
                var result = await _blogTypeService.UpdateBlogTypeAsync(id, model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Delete a blog type
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<string>> DeleteBlogType(int id)
        {
            try
            {
                var result = await _blogTypeService.DeleteBlogTypeAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
    }
}
