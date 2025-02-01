using BabyCare.Core.APIResponse;
using BabyCare.Core;
using BabyCare.ModelViews.BlogTypeModelView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabyCare.ModelViews.BlogModelViews;

namespace BabyCare.Contract.Services.Interface
{
    public interface IBlogService
    {
        Task<ApiResult<BasePaginatedList<BlogModelView>>> GetAllBlogAsync(
            int pageNumber,
            int pageSize,
            int? id ,
            string? title,
            string? status,
            bool? isFeatured);
        Task<ApiResult<object>> AddBlogAsync(CreateBlogModelView model);
        Task<ApiResult<object>> UpdateBlogAsync(int id, UpdateBlogModelView model);
        Task<ApiResult<object>> DeleteBlogAsync(int id);
        Task<ApiResult<BlogModelView>> GetBlogByIdAsync(int id);
        Task<ApiResult<List<BlogModelViewAddedType>>> GetBlogByWeekAsync(int week);

    }
}
