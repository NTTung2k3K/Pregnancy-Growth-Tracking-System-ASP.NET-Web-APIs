using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.RoleModelViews;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{
    //[Authorize(Roles = SystemConstant.Role.ADMIN)]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        ///     Get all roles with pagination and optional filters
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<BasePaginatedList<RoleModelView>>> GetAllRoles
            ([FromQuery] string? id, [FromQuery] string? name, int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                var result = await _roleService.GetAllRoleAsync(pageNumber, pageSize, id, name);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Get a role by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleModelView>> GetRoleById(string id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Create a new role
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<string>> CreateRole([FromQuery] CreateRoleModelView model)
        {
            try
            {
                var result = await _roleService.AddRoleAsync(model);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Update a role
        /// </summary>
        [HttpPut("update/{id}")]
        public async Task<ActionResult<string>> UpdateRole(string id, [FromQuery] UpdatedRoleModelView model)
        {
            try
            {
                var result = await _roleService.UpdateRoleAsync(id, model);

                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        /// <summary>
        ///     Delete a role
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<string>> DeleteRole(string id)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(id);

                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
    }
}
