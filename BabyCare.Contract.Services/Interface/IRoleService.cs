using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.RoleModelViews;

namespace BabyCare.Contract.Services.Interface
{
    public interface IRoleService
    {
        Task<ApiResult<BasePaginatedList<RoleModelView>>> GetAllRoleAsync(int pageNumber, int pageSize, string? id, string? name);
		Task<ApiResult<object>> AddRoleAsync(CreateRoleModelView model);
		Task<ApiResult<object>> UpdateRoleAsync(string id, UpdatedRoleModelView model);
		Task<ApiResult<object>> DeleteRoleAsync(string id);
        Task<ApiResult<RoleModelView>> GetRoleByIdAsync(string id);

    }
}
