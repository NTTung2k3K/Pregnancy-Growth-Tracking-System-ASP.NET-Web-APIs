using BabyCare.Contract.Repositories.Entity;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.UserMessage;
using System.Threading.Tasks;

namespace BabyCare.Contract.Services.Interface
{
    public interface IRealTimeService
    {
        Task SendMessage(string channel, string message, Guid senderId, Guid receiverId);
        Task<List<ChatMessageModelView>> GetMessageHistory(Guid senderId, Guid receiverId);
        Task<ApiResult<string>> CheckUserRole(Guid userId);
    }
}

