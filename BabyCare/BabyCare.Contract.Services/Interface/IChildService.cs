using BabyCare.Core.APIResponse;
using BabyCare.Core;
using BabyCare.ModelViews.BlogModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabyCare.ModelViews.ChildModelView;

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
        Task<ApiResult<ChildModelView>> GetChildByIdAsync(int id);
        Task<ApiResult<List<ChildModelView>>> GetChildByUserId(Guid id);

    }
}
