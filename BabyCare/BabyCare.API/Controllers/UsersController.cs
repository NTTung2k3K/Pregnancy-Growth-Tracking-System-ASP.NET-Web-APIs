using Microsoft.AspNetCore.Mvc;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core.Base;
using BabyCare.ModelViews.UserModelViews.Response;

namespace XuongMayBE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService) {
            _userService = userService;
        }
        
    }
}
