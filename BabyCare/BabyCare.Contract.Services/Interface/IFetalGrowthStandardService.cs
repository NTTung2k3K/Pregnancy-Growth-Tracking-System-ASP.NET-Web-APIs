using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.FetalGrowthStandardModelView;
using System.Threading.Tasks;
using BabyCare.Core;
namespace BabyCare.Contract.Services.Interface
{
    public interface IFetalGrowthStandardService
    {
        Task<ApiResult<BasePaginatedList<FetalGrowthStandardModelView>>> GetAllFetalGrowthStandardsAsync(int pageNumber, int pageSize);
        Task<ApiResult<FetalGrowthStandardModelView>> GetFetalGrowthStandardByIdAsync(int id);
        Task<ApiResult<FetalGrowthStandardModelView>> GetFetalGrowthStandardByWeekAsync(int week);

        Task<ApiResult<object>> AddFetalGrowthStandardAsync(CreateFetalGrowthStandardModelView model);
        Task<ApiResult<object>> UpdateFetalGrowthStandardAsync(int id, UpdateFetalGrowthStandardModelView model);
        Task<ApiResult<object>> DeleteFetalGrowthStandardAsync(int id);
    }
}
