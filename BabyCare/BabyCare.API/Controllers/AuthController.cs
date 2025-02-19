using Microsoft.AspNetCore.Mvc;
using BabyCare.Contract.Services.Interface;
using BabyCare.ModelViews.AuthModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Request;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService) 
        { 
            _userService = userService;
        }

        [HttpPost("user-login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestModel request)
        {
            try
            {

                var result = await _userService.UserLogin(request);
                if (!result.IsSuccessed)
                {
                    return StatusCode((int)result.StatusCode, result);
                }

                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/user-register
        [HttpPost("user-register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequestModel request)
        {
            try
            {
                var result = await _userService.UserRegister(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/confirm-user-register
        [HttpPost("confirm-user-register")]
        public async Task<IActionResult> ConfirmUserRegister([FromBody] ConfirmUserRegisterRequest request)
        {
            try
            {
                var result = await _userService.ConfirmUserRegister(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/forgot-password
        [HttpPost("user-forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var result = await _userService.ForgotPassword(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/reset-password
        [HttpPost("user-reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel request)
        {
            try
            {
                var result = await _userService.ResetPassword(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] NewRefreshTokenRequestModel request)
        {
            try
            {
                var result = await _userService.RefreshToken(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpPost("user-login-google")]
        public async Task<IActionResult> UserLoginGoogle([FromBody] UserLoginGoogleRequest request)
        {
            try
            {
                var result = await _userService.UserLoginGoogle(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        #region Auth Employee
        // POST: api/Auth/employee-login
        [HttpPost("employee-login")]
        public async Task<IActionResult> EmployeeLogin([FromBody] EmployeeLoginRequestModel request)
        {
            try
            {
                var result = await _userService.EmployeeLogin(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/employee-forgot-password
        [HttpPost("employee-forgot-password")]
        public async Task<IActionResult> EmployeeForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var result = await _userService.EmployeeForgotPassword(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        // POST: api/Auth/employee-reset-password
        [HttpPost("employee-reset-password")]
        public async Task<IActionResult> EmployeeResetPassword([FromBody] ResetPasswordRequestModel request)
        {
            try
            {
                var result = await _userService.EmployeeResetPassword(request);
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
