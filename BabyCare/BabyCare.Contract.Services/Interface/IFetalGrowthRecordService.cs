using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.FetalGrowthRecordModelView;
using BabyCare.Core;

namespace BabyCare.Contract.Services.Interface
{
    public interface IFetalGrowthRecordService
    {
        Task<ApiResult<BasePaginatedList<FetalGrowthRecordModelView>>> GetAllFetalGrowthRecordsAsync(int pageNumber, int pageSize, int? childId, int? weekOfPregnancy);
        Task<ApiResult<FetalGrowthRecordModelView>> GetFetalGrowthRecordByIdAsync(int id);
        Task<ApiResult<object>> AddFetalGrowthRecordAsync(CreateFetalGrowthRecordModelView model);
        Task<ApiResult<object>> UpdateFetalGrowthRecordAsync(int id, UpdateFetalGrowthRecordModelView model);
        Task<ApiResult<object>> DeleteFetalGrowthRecordAsync(int id);
    }
}
