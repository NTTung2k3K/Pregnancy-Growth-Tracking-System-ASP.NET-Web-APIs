using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.FetalGrowthRecordModelView;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BabyCare.Services.Service
{
    public class FetalGrowthRecordService : IFetalGrowthRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public FetalGrowthRecordService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
        }

        public async Task<ApiResult<object>> AddFetalGrowthRecordAsync(CreateFetalGrowthRecordModelView model)
        {
            // Check if the record already exists for the given ChildId and WeekOfPregnancy
            var existingRecord = await _unitOfWork.GetRepository<FetalGrowthRecord>()
                .Entities
                .FirstOrDefaultAsync(r => r.ChildId == model.ChildId && r.WeekOfPregnancy == model.WeekOfPregnancy && !r.DeletedTime.HasValue);

            if (existingRecord != null)
            {
                return new ApiErrorResult<object>("Fetal growth record already exists for the given child and week.");
            }

            FetalGrowthRecord newRecord = _mapper.Map<FetalGrowthRecord>(model);

            newRecord.CreatedBy = model.ChildId.ToString();  // Assuming CreatedBy is ChildId for this example
            newRecord.CreatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<FetalGrowthRecord>().InsertAsync(newRecord);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Fetal growth record added successfully.");
        }

        public async Task<ApiResult<object>> UpdateFetalGrowthRecordAsync(int id, UpdateFetalGrowthRecordModelView model)
        {
            if (id <= 0)
            {
                return new ApiErrorResult<object>("Please provide a valid Fetal Growth Record ID.");
            }

            var existingRecord = await _unitOfWork.GetRepository<FetalGrowthRecord>().Entities
                .FirstOrDefaultAsync(r => r.Id == id && !r.DeletedTime.HasValue);

            if (existingRecord == null)
            {
                return new ApiErrorResult<object>("Fetal Growth Record not found or already deleted.");
            }

            bool isUpdated = false;

            // Check and update fields if necessary
            if (model.WeekOfPregnancy.HasValue && model.WeekOfPregnancy != existingRecord.WeekOfPregnancy)
            {
                existingRecord.WeekOfPregnancy = model.WeekOfPregnancy.Value;
                isUpdated = true;
            }

            if (model.Weight.HasValue && model.Weight != existingRecord.Weight)
            {
                existingRecord.Weight = model.Weight.Value;
                isUpdated = true;
            }

            if (model.Height.HasValue && model.Height != existingRecord.Height)
            {
                existingRecord.Height = model.Height.Value;
                isUpdated = true;
            }

            if (model.RecordedAt.HasValue && model.RecordedAt != existingRecord.RecordedAt)
            {
                existingRecord.RecordedAt = model.RecordedAt.Value;
                isUpdated = true;
            }

            if (model.GrowChartsID.HasValue && model.GrowChartsID != existingRecord.GrowChartsID)
            {
                existingRecord.GrowChartsID = model.GrowChartsID.Value;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.HealthCondition) && model.HealthCondition != existingRecord.HealthCondition)
            {
                existingRecord.HealthCondition = model.HealthCondition;
                isUpdated = true;
            }

            if (isUpdated)
            {
                existingRecord.LastUpdatedBy = model.HealthCondition;  // Assuming HealthCondition as the user here (could be modified)
                existingRecord.LastUpdatedTime = DateTimeOffset.UtcNow;

                await _unitOfWork.GetRepository<FetalGrowthRecord>().UpdateAsync(existingRecord);
                await _unitOfWork.SaveAsync();

                return new ApiSuccessResult<object>("Fetal Growth Record updated successfully.");
            }

            return new ApiErrorResult<object>("No changes detected to update.");
        }

        public async Task<ApiResult<object>> DeleteFetalGrowthRecordAsync(int id)
        {
            if (id <= 0)
            {
                return new ApiErrorResult<object>("Please provide a valid Fetal Growth Record ID.");
            }

            var existingRecord = await _unitOfWork.GetRepository<FetalGrowthRecord>().Entities
                .FirstOrDefaultAsync(r => r.Id == id && !r.DeletedTime.HasValue);

            if (existingRecord == null)
            {
                return new ApiErrorResult<object>("Fetal Growth Record not found or already deleted.");
            }

            existingRecord.DeletedTime = DateTimeOffset.UtcNow;
            existingRecord.DeletedBy = existingRecord.ChildId.ToString(); // Track deletion (assumed as ChildId)

            await _unitOfWork.GetRepository<FetalGrowthRecord>().UpdateAsync(existingRecord);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Fetal Growth Record deleted successfully.");
        }

        public async Task<ApiResult<BasePaginatedList<FetalGrowthRecordModelView>>> GetAllFetalGrowthRecordsAsync(int pageNumber, int pageSize, int? childId, int? weekOfPregnancy)
        {
            IQueryable<FetalGrowthRecord> recordQuery = _unitOfWork.GetRepository<FetalGrowthRecord>().Entities
                .AsNoTracking()
                .Where(r => !r.DeletedTime.HasValue);

            if (childId != null)
                recordQuery = recordQuery.Where(r => r.ChildId == childId);

            if (weekOfPregnancy != null)
                recordQuery = recordQuery.Where(r => r.WeekOfPregnancy == weekOfPregnancy);

            recordQuery = recordQuery.OrderByDescending(r => r.RecordedAt);

            int totalCount = await recordQuery.CountAsync();

            List<FetalGrowthRecord> paginatedRecords = await recordQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<FetalGrowthRecordModelView> recordModelViews = _mapper.Map<List<FetalGrowthRecordModelView>>(paginatedRecords);
            var result = new BasePaginatedList<FetalGrowthRecordModelView>(recordModelViews, totalCount, pageNumber, pageSize);

            return new ApiSuccessResult<BasePaginatedList<FetalGrowthRecordModelView>>(result);
        }

        public async Task<ApiResult<FetalGrowthRecordModelView>> GetFetalGrowthRecordByIdAsync(int id)
        {
            if (id <= 0)
            {
                return new ApiErrorResult<FetalGrowthRecordModelView>("Please provide a valid Fetal Growth Record ID.");
            }

            var record = await _unitOfWork.GetRepository<FetalGrowthRecord>().Entities
                .FirstOrDefaultAsync(r => r.Id == id && !r.DeletedTime.HasValue);

            if (record == null)
            {
                return new ApiErrorResult<FetalGrowthRecordModelView>("Fetal Growth Record not found.");
            }

            FetalGrowthRecordModelView recordModelView = _mapper.Map<FetalGrowthRecordModelView>(record);
            return new ApiSuccessResult<FetalGrowthRecordModelView>(recordModelView);
        }
    }
}
