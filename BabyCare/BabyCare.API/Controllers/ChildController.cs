using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.ChildModelView;
using BabyCare.ModelViews.UserModelViews.Response;
using BabyCare.Services.Service;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildController : ControllerBase
    {
        private readonly IChildService _childService;

        public ChildController(IChildService childService)
        {
            _childService = childService;
        }

        /// <summary>
        ///     Get all children with pagination and optional filters
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<BasePaginatedList<ChildModelView>>> GetAllChildren(
            [FromQuery] int? id,
            [FromQuery] string? name,
            [FromQuery] DateTime? dueDate,
            [FromQuery] string? bloodType,
            [FromQuery] string? pregnancyStage,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var result = await _childService.GetAllChildAsync(pageNumber, pageSize, id, name, dueDate, bloodType, pregnancyStage);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpGet("get-child-by-user-id")]
        public async Task<IActionResult> GetChildByUserId([FromQuery]Guid id)
        {
            try
            {
                var result = await _childService.GetChildByUserId(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        /// <summary>
        ///     Get a child by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ChildModelView>> GetChildById(int id)
        {
            try
            {
                var result = await _childService.GetChildByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Create a new child
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<string>> CreateChild([FromForm] CreateChildModelView model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _childService.AddChildAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Update a child's details
        /// </summary>
        [HttpPut("update/{id}")]
        public async Task<ActionResult<string>> UpdateChild(int id, [FromBody] UpdateChildModelView model)
        {
            try
            {
                var result = await _childService.UpdateChildAsync(id, model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Delete a child
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<string>> DeleteChild(int id)
        {
            try
            {
                var result = await _childService.DeleteChildAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
    }

}
