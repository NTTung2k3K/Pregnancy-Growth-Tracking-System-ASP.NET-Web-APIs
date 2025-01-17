using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.ModelViews.UserModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;

namespace BabyCare.Contract.Services.Interface
{
    public interface IUserService
    {
        Task<ApiResult<UserLoginResponseModel>> UserLogin(UserLoginRequestModel request);
    }
}
