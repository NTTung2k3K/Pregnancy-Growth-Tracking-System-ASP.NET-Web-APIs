﻿using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.GrowthChartModelView;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BabyCare.Services.Service
{
    public class GrowthChartService : IGrowthChartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GrowthChartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResult<BasePaginatedList<GrowthChartModelView>>> GetAllGrowthChartsAsync(int pageNumber, int pageSize)
        {
            IQueryable<GrowthChart> query = _unitOfWork.GetRepository<GrowthChart>().Entities
                .AsNoTracking()
                .Where(g => !g.DeletedTime.HasValue)
                .OrderByDescending(g => g.CreatedTime);

            int totalCount = await query.CountAsync();

            var list = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var modelList = _mapper.Map<List<GrowthChartModelView>>(list);

            return new ApiSuccessResult<BasePaginatedList<GrowthChartModelView>>(
                new BasePaginatedList<GrowthChartModelView>(modelList, totalCount, pageNumber, pageSize));
        }

        public async Task<ApiResult<GrowthChartModelView>> GetGrowthChartByIdAsync(int id)
        {
            var entity = await _unitOfWork.GetRepository<GrowthChart>().Entities
                .FirstOrDefaultAsync(g => g.Id == id && !g.DeletedTime.HasValue);

            if (entity == null)
            {
                return new ApiErrorResult<GrowthChartModelView>("Growth chart not found.");
            }

            var model = _mapper.Map<GrowthChartModelView>(entity);
            return new ApiSuccessResult<GrowthChartModelView>(model);
        }

        public async Task<ApiResult<object>> AddGrowthChartAsync(CreateGrowthChartModelView model)
        {
            var entity = _mapper.Map<GrowthChart>(model);
            entity.CreatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<GrowthChart>().InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Growth chart created successfully.");
        }

        public async Task<ApiResult<object>> UpdateGrowthChartAsync(int id, UpdateGrowthChartModelView model)
        {
            var entity = await _unitOfWork.GetRepository<GrowthChart>().Entities
                .FirstOrDefaultAsync(g => g.Id == id && !g.DeletedTime.HasValue);

            if (entity == null)
            {
                return new ApiErrorResult<object>("Growth chart not found.");
            }

            _mapper.Map(model, entity);
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<GrowthChart>().UpdateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Growth chart updated successfully.");
        }

        public async Task<ApiResult<object>> DeleteGrowthChartAsync(int id)
        {
            var entity = await _unitOfWork.GetRepository<GrowthChart>().Entities
                .FirstOrDefaultAsync(g => g.Id == id && !g.DeletedTime.HasValue);

            if (entity == null)
            {
                return new ApiErrorResult<object>("Growth chart not found.");
            }

            entity.DeletedTime = DateTimeOffset.UtcNow;
            await _unitOfWork.GetRepository<GrowthChart>().UpdateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Growth chart deleted successfully.");
        }
    }
}
