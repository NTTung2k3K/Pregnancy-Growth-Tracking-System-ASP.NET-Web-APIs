using BabyCare.Core;
using BabyCare.ModelViews.RoleModelViews;

namespace BabyCare.Contract.Services.Interface
{
    public interface IRoleService
    {
        Task<BasePaginatedList<RoleModelView>> GetAllRoleAsync(int pageNumber, int pageSize, string? id, string? name);
		Task<string> AddRoleAsync(CreateRoleModelView model);
		Task<string> UpdateRoleAsync(string id, UpdatedRoleModelView model);
		Task<string> DeleteRoleAsync(string id);
        Task<RoleModelView?> GetRoleByIdAsync(string id);

    }
}
