﻿using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core.APIResponse;
using BabyCare.Core;
using BabyCare.ModelViews.FeedbackModelView;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BabyCare.ModelViews.UserModelViews.Response;
using BabyCare.ModelViews.GrowthChartModelView;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.Services.Service
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUsers> _userManager;
        public FeedbackService(UserManager<ApplicationUsers> userManager,IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
        }

        public async Task<ApiResult<object>> AddFeedbackAsync(CreateFeedbackModelView model)
        {

            // Check if user exists
            var userExists = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (userExists == null)
            {
                return new ApiErrorResult<object>("User does not exist.");
            }

            // Check if GrowthChart exists
            var growthChartExists = await _unitOfWork.GetRepository<GrowthChart>().GetByIdAsync(model.GrowthChartsID);
            if (growthChartExists == null)
            {
                return new ApiErrorResult<object>("Growth chart does not exist.");
            }

            // Check validation for Description
            if (string.IsNullOrWhiteSpace(model.Description))
            {
                return new ApiErrorResult<object>("Description is required.");
            }
            if (model.Description.Length > 250)
            {
                return new ApiErrorResult<object>("Description must be at most 250 characters.");
            }
            

            var feedback = _mapper.Map<Feedback>(model);
            feedback.CreatedTime = DateTime.UtcNow;

            if (model.ParentFeedbackID.HasValue)
            {
                // Check if parent feedback exists
                var parentFeedback = await _unitOfWork.GetRepository<Feedback>().GetByIdAsync(model.ParentFeedbackID);
                if (parentFeedback == null)
                {
                    return new ApiErrorResult<object>("Parent feedback not found.");
                }
                feedback.ResponseFeedback = parentFeedback;
            }
            feedback.Status = (int)FeedbackStatus.Active;
            await _unitOfWork.GetRepository<Feedback>().InsertAsync(feedback);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Feedback added successfully");
        }

        public async Task<ApiResult<object>> DeleteFeedbackAsync(int id)
        {
            if (_contextAccessor.HttpContext?.User?.FindFirst("userId") == null)
            {
                return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
            }
            var existingFeedback = await _unitOfWork.GetRepository<Feedback>().Entities
                .FirstOrDefaultAsync(f => f.Id == id && !f.DeletedTime.HasValue);

            if (existingFeedback == null)
            {
                return new ApiErrorResult<object>("Feedback not found or already deleted");
            }

            existingFeedback.DeletedTime = DateTime.Now;
            existingFeedback.DeletedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId").Value;

            await _unitOfWork.GetRepository<Feedback>().UpdateAsync(existingFeedback);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Feedback deleted successfully");
        }
        public async Task<ApiResult<object>> BlockFeedbackAsync(BanFeedbackRequest request)
        {
            if (_contextAccessor.HttpContext?.User?.FindFirst("userId") == null)
            {
                return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
            }
            var existingFeedback = await _unitOfWork.GetRepository<Feedback>().Entities
                .FirstOrDefaultAsync(f => f.Id == request.Id && !f.DeletedTime.HasValue);

            if (existingFeedback == null)
            {
                return new ApiErrorResult<object>("Feedback not found or already deleted");
            }
            if (existingFeedback.Status == (int)FeedbackStatus.BANNED) { 
                return new ApiErrorResult<object>("Feedback has been banned");
            }

            existingFeedback.LastUpdatedTime = DateTime.Now;
            existingFeedback.LastUpdatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId").Value;
            existingFeedback.Status = (int)FeedbackStatus.BANNED;
            await _unitOfWork.GetRepository<Feedback>().UpdateAsync(existingFeedback);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Feedback deleted successfully");
        }

        public async Task<ApiResult<List<FeedbackModelViewForAdmin>>> GetAllFeedbackAdminAsync()
        {
            IQueryable<Feedback> feedbackQuery = _unitOfWork.GetRepository<Feedback>().Entities.AsNoTracking();


            var data = await feedbackQuery.OrderByDescending(f => f.CreatedTime).ToListAsync();

            
           var res = data.Select(x => new FeedbackModelViewForAdmin()
            {
                CreatedTime = x.CreatedTime,
                Description = x.Description,
                Id = x.Id,
                Rating = x.Rating,
                Status = Enum.IsDefined(typeof (FeedbackStatus),x.Status) ? ((FeedbackStatus)x.Status).ToString() : "Unknown",
                UserResponseModel = _mapper.Map<UserResponseModel>(x.User),
                GrowthChartModelView = _mapper.Map<GrowthChartModelView>(x.GrowthChart),
            }).ToList();
            return new ApiSuccessResult<List<FeedbackModelViewForAdmin>>(
               res
            );
        }

        public async Task<ApiResult<BasePaginatedList<FeedbackModelView>>> GetAllFeedbackAsync(int pageNumber, int pageSize, int? growthChartId, string? status)
        {
            IQueryable<Feedback> feedbackQuery = _unitOfWork.GetRepository<Feedback>().Entities.AsNoTracking();

           

            if (growthChartId.HasValue)
            {
                feedbackQuery = feedbackQuery.Where(f => f.GrowthChartsID == growthChartId.Value);
            }

            //if (!string.IsNullOrWhiteSpace(status))
            //{
            //    feedbackQuery = feedbackQuery.Where(f => f.Status == status);
            //}

            feedbackQuery = feedbackQuery.OrderByDescending(f => f.CreatedTime);

            int totalCount = await feedbackQuery.CountAsync();
            var paginatedFeedback = await feedbackQuery.Skip((pageNumber - 1) * pageSize)
                                                       .Take(pageSize)
                                                       .ToListAsync();

            var feedbackViews = _mapper.Map<List<FeedbackModelView>>(paginatedFeedback);

            return new ApiSuccessResult<BasePaginatedList<FeedbackModelView>>(
                new BasePaginatedList<FeedbackModelView>(feedbackViews, totalCount, pageNumber, pageSize)
            );
        }

        public async Task<ApiResult<FeedbackModelView>> GetFeedbackByIdAsync(int id)
        {
            var feedback = await _unitOfWork.GetRepository<Feedback>().Entities
                .FirstOrDefaultAsync(f => f.Id == id && !f.DeletedTime.HasValue);

            if (feedback == null)
            {
                return new ApiErrorResult<FeedbackModelView>("Feedback not found");
            }

            var feedbackView = _mapper.Map<FeedbackModelView>(feedback);
            return new ApiSuccessResult<FeedbackModelView>(feedbackView);
        }

        public async Task<ApiResult<object>> UpdateFeedbackAsync(int id, UpdateFeedbackModelView model)
        {
            var feedback = await _unitOfWork.GetRepository<Feedback>().Entities
                .FirstOrDefaultAsync(f => f.Id == id && !f.DeletedTime.HasValue);

            if (feedback == null)
            {
                return new ApiErrorResult<object>("Feedback not found or already deleted");
            }

            bool isUpdated = false;

            if (!string.IsNullOrWhiteSpace(model.Description) && feedback.Description != model.Description)
            {
                feedback.Description = model.Description;
                isUpdated = true;
            }

            if (model.Rating.HasValue && feedback.Rating != model.Rating)
            {
                feedback.Rating = model.Rating.Value;
                isUpdated = true;
            }

            if (isUpdated)
            {
                feedback.LastUpdatedTime = DateTimeOffset.UtcNow;

                await _unitOfWork.GetRepository<Feedback>().UpdateAsync(feedback);
                await _unitOfWork.SaveAsync();

                return new ApiSuccessResult<object>("Feedback updated successfully");
            }

            return new ApiErrorResult<object>("No changes were made");
        }
    }
}
