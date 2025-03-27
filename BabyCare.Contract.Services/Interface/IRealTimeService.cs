using BabyCare.Contract.Repositories.Entity;
using BabyCare.Core.APIResponse;
using System.Threading.Tasks;

namespace BabyCare.Contract.Services.Interface
{
    public interface IRealTimeService
    {
        Task SendMessage(string channel, string message, Guid senderId);
        Task<ApiResult<string>> CheckUserRole(Guid userId);
    }
}

