using BabyCare.Core.APIResponse;
using BabyCare.Core;
using BabyCare.ModelViews.RoleModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabyCare.ModelViews.BlogTypeModelView;

namespace BabyCare.Contract.Services.Interface
{
    public interface IBlogTypeService
    {
        Task<ApiResult<BasePaginatedList<BlogTypeModelView>>> GetAllBlogTypeAsync(int pageNumber, int pageSize, int? id, string? name);
        Task<ApiResult<object>> AddBlogTypeAsync(CreateBlogTypeModelView model);
        Task<ApiResult<object>> UpdateBlogTypeAsync(int id, UpdateBlogTypeModelView model);
        Task<ApiResult<object>> DeleteBlogTypeAsync(int id);
        Task<ApiResult<BlogTypeModelView>> GetBlogTypeByIdAsync(int id);
    }
}
