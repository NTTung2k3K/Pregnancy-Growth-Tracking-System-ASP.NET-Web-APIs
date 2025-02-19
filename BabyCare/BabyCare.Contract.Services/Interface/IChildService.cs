using BabyCare.Core.APIResponse;
using BabyCare.Core;
using BabyCare.ModelViews.ChildModelView;
using BabyCare.ModelViews.AppointmentModelViews.Response;

namespace BabyCare.Contract.Services.Interface
{
    public interface IChildService
    {
        Task<ApiResult<BasePaginatedList<ChildModelView>>> GetAllChildAsync(
    int pageNumber,
    int pageSize,
    int? id,           // Filter by the user's ID
    string? name,           // Filter by child's name
    DateTime? dateOfBirth,  // Filter by child's date of birth
    string? bloodType,      // Filter by child's blood type
    string? pregnancyStage  // Filter by child's pregnancy stage
);
        Task<ApiResult<object>> AddChildAsync(CreateChildModelView model);
        Task<ApiResult<object>> UpdateChildAsync(int id, UpdateChildModelView model);
        Task<ApiResult<object>> DeleteChildAsync(int id);
        Task<ApiResult<ChildModelViewAddeRecords>> GetChildByIdAsync(int id);
        Task<ApiResult<List<ChildModelView>>> GetChildByUserId(Guid id);
        Task<ApiResult<BasePaginatedList<ChildModelView>>> GetChildByUserIdPagination(SearchChildByUserId request);

    }
}
