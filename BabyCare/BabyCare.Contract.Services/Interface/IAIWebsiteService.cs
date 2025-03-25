using BabyCare.Core.APIResponse;
using System.Threading.Tasks;

namespace BabyCare.Contract.Services.Interface
{
    public interface IAIWebsiteService
    {
        Task<ApiResult<string>> GetWebsiteAIResponseAsync(string question);
    }
}
