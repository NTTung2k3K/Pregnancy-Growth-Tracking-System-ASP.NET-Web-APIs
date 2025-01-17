using Microsoft.AspNetCore.Mvc;
using BabyCare.ModelViews.AuthModelViews;
using BabyCare.Contract.Services.Interface;
using BabyCare.ModelViews.UserModelViews.Request;

namespace XuongMayBE.API.Controllers
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
                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        [HttpPost("new_account")]
        public async Task<IActionResult> Register()
        {
            return Ok();
        }

    }
}
