using Microsoft.AspNetCore.Mvc;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core.Base;
using BabyCare.ModelViews.UserModelViews.Response;
using BabyCare.ModelViews.UserModelViews.Request;
using BabyCare.Core;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService) {
            _userService = userService;
        }
        #region User
        // POST: api/Auth/update-user-profile
        [HttpPut("update-user-profile")]
        public async Task<IActionResult> UpdateUserProfile([FromForm] UpdateUserProfileRequest request)
        {
            try
            {
                var result = await _userService.UpdateUserProfile(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/get-user-pagination
        [HttpGet("get-user-pagination")]
        public async Task<IActionResult> GetUserPagination([FromQuery] BaseSearchRequest request)
        {
            try
            {
                var result = await _userService.GetUserPagination(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<UserResponseModel>>(ex.Message));
            }
        }
        // POST: api/Auth/get-user-by-id
        [HttpGet("get-user-by-id")]
        public async Task<IActionResult> GetUserById([FromQuery] Guid Id)
        {
            try
            {
                var result = await _userService.GetUserById(Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BabyCare.ModelViews.UserModelViews.Response.UserResponseModel>(ex.Message));
            }
        }
        // POST: api/Auth/delete-user
        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUser([FromQuery] DeleteUserRequest request)
        {
            try
            {
                var result = await _userService.DeleteUser(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/update-user-status
        [HttpPut("update-user-status")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                var result = await _userService.UpdateUserStatus(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        #endregion
    }
}
