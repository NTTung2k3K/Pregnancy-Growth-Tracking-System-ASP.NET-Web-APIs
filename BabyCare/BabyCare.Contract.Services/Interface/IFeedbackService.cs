using BabyCare.Core.APIResponse;
using BabyCare.Core;
using BabyCare.ModelViews.FeedbackModelView;

namespace BabyCare.Contract.Services.Interface
{
    public interface IFeedbackService
    {
        Task<ApiResult<List<FeedbackModelViewForAdmin>>> GetAllFeedbackAdminAsync();

        Task<ApiResult<BasePaginatedList<FeedbackModelView>>> GetAllFeedbackAsync(int pageNumber, int pageSize, int? growthChartId, string? status);
        Task<ApiResult<FeedbackModelView>> GetFeedbackByIdAsync(int id);
        Task<ApiResult<object>> AddFeedbackAsync(CreateFeedbackModelView model);
        Task<ApiResult<object>> UpdateFeedbackAsync(int id, UpdateFeedbackModelView model);
        Task<ApiResult<object>> DeleteFeedbackAsync(int id);
        Task<ApiResult<object>> BlockFeedbackAsync(BanFeedbackRequest request);
        Task<ApiResult<object>> GetFeedbacksWithPagination(int growthChartId, int? pageIndex, int? pageSize);
        Task<ApiResult<object>> GetFeedbacksWithPaginationAdmin(int growthChartId, int? pageIndex, int? pageSize);



    }
}
