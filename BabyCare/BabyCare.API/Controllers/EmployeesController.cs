using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.Base;
using BabyCare.ModelViews.AuthModelViews.Response;
using BabyCare.ModelViews.UserModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IUserService _userService;
        public EmployeesController(IUserService userService)
        {
            _userService = userService;
        }
        #region Employee
        // POST: api/Auth/create-employee
        [HttpPost("create-employee")]
        public async Task<IActionResult> CreateEmployee([FromForm] CreateEmployeeRequest request)
        {
            try
            {
                var result = await _userService.CreateEmployee(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<UserLoginResponseModel>(ex.Message));
            }
        }
        // POST: api/Auth/update-employee-profile
        [HttpPut("update-employee-profile")]
        public async Task<IActionResult> UpdateEmployeeProfile([FromForm] UpdateEmployeeProfileRequest request)
        {
            try
            {
                var result = await _userService.UpdateEmployeeProfile(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/update-employee-status
        [HttpPut("update-employee-status")]
        public async Task<IActionResult> UpdateEmployeeStatus([FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                var result = await _userService.UpdateEmployeeStatus(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/delete-employee
        [HttpDelete("delete-employee")]
        public async Task<IActionResult> DeleteEmployee([FromQuery] DeleteUserRequest request)
        {
            try
            {
                var result = await _userService.DeleteEmployee(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/get-doctor-pagination
        [HttpGet("get-doctor-pagination")]
        public async Task<IActionResult> GetDoctorPagination([FromQuery] BaseSearchRequest request)
        {
            try
            {
                var result = await _userService.GetDoctorPagination(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        // POST: api/Auth/get-employee-by-id
        [HttpGet("get-employee-by-id")]
        public async Task<IActionResult> GetEmployeeById([FromQuery] Guid Id)
        {
            try
            {
                var result = await _userService.GetEmployeeById(Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<EmployeeResponseModel>(ex.Message));
            }
        }

        #endregion
    }
}
