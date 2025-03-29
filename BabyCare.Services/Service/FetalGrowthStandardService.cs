using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.FetalGrowthStandardModelView;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace BabyCare.Services.Service
{
    public class FetalGrowthStandardService : IFetalGrowthStandardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FetalGrowthStandardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResult<BasePaginatedList<FetalGrowthStandardModelView>>> GetAllFetalGrowthStandardsAsync(int pageNumber, int pageSize)
        {
            IQueryable<FetalGrowthStandard> query = _unitOfWork.GetRepository<FetalGrowthStandard>().Entities
                .AsNoTracking()
                .OrderByDescending(x => x.LastUpdatedTime)
                .Where(f => !f.DeletedTime.HasValue);

            int totalCount = await query.CountAsync();

            List<FetalGrowthStandard> list = await query
                .OrderByDescending(f => f.CreatedTime) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<FetalGrowthStandardModelView> modelList = _mapper.Map<List<FetalGrowthStandardModelView>>(list);

            return new ApiSuccessResult<BasePaginatedList<FetalGrowthStandardModelView>>(
                new BasePaginatedList<FetalGrowthStandardModelView>(modelList, totalCount, pageNumber, pageSize));
        }


        public async Task<ApiResult<FetalGrowthStandardModelView>> GetFetalGrowthStandardByIdAsync(int id)
        {
            var entity = await _unitOfWork.GetRepository<FetalGrowthStandard>().Entities
                .FirstOrDefaultAsync(f => f.Id == id && !f.DeletedTime.HasValue);

            if (entity == null)
            {
                return new ApiErrorResult<FetalGrowthStandardModelView>("Fetal growth standard not found.");
            }

            var model = _mapper.Map<FetalGrowthStandardModelView>(entity);
            return new ApiSuccessResult<FetalGrowthStandardModelView>(model);
        }

        public async Task<ApiResult<object>> AddFetalGrowthStandardAsync(CreateFetalGrowthStandardModelView model)
        {
            var existingWeek = await _unitOfWork.GetRepository<FetalGrowthStandard>().Entities.Where(x => x.Week == model.Week && x.Gender == model.Gender).ToListAsync();
            if (existingWeek.Count > 0)
            {
                return new ApiErrorResult<object>("Fetal growth standard has existing");

            }
            var entity = _mapper.Map<FetalGrowthStandard>(model);
            entity.CreatedTime = DateTime.Now;

            await _unitOfWork.GetRepository<FetalGrowthStandard>().InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Fetal growth standard created successfully.");
        }

        public async Task<ApiResult<object>> UpdateFetalGrowthStandardAsync(int id, UpdateFetalGrowthStandardModelView model)
        {
            var entity = await _unitOfWork.GetRepository<FetalGrowthStandard>().Entities
                .FirstOrDefaultAsync(f => f.Id == id && !f.DeletedTime.HasValue);
            
            if (entity == null)
            {
                return new ApiErrorResult<object>("Fetal growth standard not found.");
            }
            if (model.Week != entity.Week)
            {
                var existingWeek = await _unitOfWork.GetRepository<FetalGrowthStandard>().Entities.Where(x => x.Week == model.Week && x.Gender == model.Gender).ToListAsync();
                if (existingWeek.Count > 0)
                {
                    return new ApiErrorResult<object>("Fetal growth standard has existing.");

                }
            }
            _mapper.Map(model, entity);
            entity.LastUpdatedTime = DateTime.Now;

            await _unitOfWork.GetRepository<FetalGrowthStandard>().UpdateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Fetal growth standard updated successfully.");
        }

        public async Task<ApiResult<object>> DeleteFetalGrowthStandardAsync(int id)
        {
            var entity = await _unitOfWork.GetRepository<FetalGrowthStandard>().Entities
                .FirstOrDefaultAsync(f => f.Id == id && !f.DeletedTime.HasValue);

            if (entity == null)
            {
                return new ApiErrorResult<object>("Fetal growth standard not found.");
            }

            entity.DeletedTime = DateTime.Now;
            await _unitOfWork.GetRepository<FetalGrowthStandard>().UpdateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Fetal growth standard deleted successfully.");
        }

        public async Task<ApiResult<FetalGrowthStandardModelView>> GetFetalGrowthStandardByWeekAsync(int week, int gender)
        {
            var entity = await _unitOfWork.GetRepository<FetalGrowthStandard>().Entities
                 .FirstOrDefaultAsync(f => f.Week == week && f.Gender == gender &&!f.DeletedTime.HasValue);

            if (entity == null)
            {
                return new ApiErrorResult<FetalGrowthStandardModelView>("Fetal growth standard not found.");
            }

            var model = _mapper.Map<FetalGrowthStandardModelView>(entity);
            return new ApiSuccessResult<FetalGrowthStandardModelView>(model);
        }
    }
}
