using BabyCare.ModelViews.UserModelViews;

namespace BabyCare.Contract.Services.Interface
{
    public interface IUserService
    {
        Task<IList<UserResponseModel>> GetAll();
    }
}
