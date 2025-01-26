using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.GrowthChartModelView;
using System.Threading.Tasks;
using BabyCare.Core;

namespace BabyCare.Contract.Services.Interface
{
    public interface IGrowthChartService
    {
        Task<ApiResult<BasePaginatedList<GrowthChartModelView>>> GetAllGrowthChartsAsync(int pageNumber, int pageSize);
        Task<ApiResult<GrowthChartModelView>> GetGrowthChartByIdAsync(int id);
        Task<ApiResult<object>> AddGrowthChartAsync(CreateGrowthChartModelView model);
        Task<ApiResult<object>> UpdateGrowthChartAsync(int id, UpdateGrowthChartModelView model);
        Task<ApiResult<object>> DeleteGrowthChartAsync(int id);
    }
}
