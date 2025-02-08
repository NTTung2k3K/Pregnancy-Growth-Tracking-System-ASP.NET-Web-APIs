using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.GrowthChartModelView;
using System.Threading.Tasks;
using BabyCare.Core;
using BabyCare.ModelViews.AppointmentModelViews.Request;
using BabyCare.ModelViews.AppointmentModelViews.Response;
using BabyCare.ModelViews.FeedbackModelView;

namespace BabyCare.Contract.Services.Interface
{
    public interface IGrowthChartService
    {
        Task<ApiResult<BasePaginatedList<GrowthChartModelView>>> GetAllGrowthChartsAsync(int pageNumber, int pageSize);
        Task<ApiResult<List<GrowthChartModelView>>> GetAllGrowthChartsAdminAsync();

        Task<ApiResult<GrowthChartModelView>> GetGrowthChartByIdAsync(int id);
        Task<ApiResult<GrowthChartCreateResponse>> AddGrowthChartAsync(CreateGrowthChartModelView model);
        Task<ApiResult<object>> UpdateGrowthChartAsync(int id, UpdateGrowthChartModelView model);
        Task<ApiResult<object>> DeleteGrowthChartAsync(int id);
        Task<ApiResult<object>> UpdateGrowthChartStatusByUserAsync(UpdateGrowChartByUser model);
        Task<ApiResult<object>> UpdateGrowthChartStatusByAdminAsync(UpdateGrowChartByAdmin model);
        Task<ApiResult<BasePaginatedList<GrowthChartModelView>>> GetGrowthChartPagination(SearchOptimizeRequest request);
        Task<ApiResult<BasePaginatedList<GrowthChartModelView>>> GetMyGrowthChartPagination(ModelViews.AppointmentModelViews.Request.SearchAppointmentByUserId request);

        Task<ApiResult<object>> UpdateView(UpdateViewRequest request);

        ApiResult<List<GrowthChartStatusResponse>> GetGrowthChartsStatusHandler(bool isAdminUpdate);
    }
}
