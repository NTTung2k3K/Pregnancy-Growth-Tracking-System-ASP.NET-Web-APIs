using BabyCare.Core.APIResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Services.Interface
{
    public interface IAIChildService
    {
        Task<ApiResult<string>> GetAIResponseAsync(string question, Guid userId, int childId);
    }
}