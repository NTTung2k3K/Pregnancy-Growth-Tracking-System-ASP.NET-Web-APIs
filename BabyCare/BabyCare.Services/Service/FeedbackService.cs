using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core.APIResponse;
using BabyCare.Core;
using BabyCare.ModelViews.FeedbackModelView;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BabyCare.Services.Service
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
        }

        public async Task<ApiResult<object>> AddFeedbackAsync(CreateFeedbackModelView model)
        {
            Feedback newFeedback = _mapper.Map<Feedback>(model);

            newFeedback.CreatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<Feedback>().InsertAsync(newFeedback);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Feedback added successfully");
        }

        public async Task<ApiResult<object>> DeleteFeedbackAsync(int id)
        {
            var existingFeedback = await _unitOfWork.GetRepository<Feedback>().Entities
                .FirstOrDefaultAsync(f => f.Id == id && !f.DeletedTime.HasValue);

            if (existingFeedback == null)
            {
                return new ApiErrorResult<object>("Feedback not found or already deleted");
            }

            existingFeedback.DeletedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<Feedback>().UpdateAsync(existingFeedback);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Feedback deleted successfully");
        }

        public async Task<ApiResult<BasePaginatedList<FeedbackModelView>>> GetAllFeedbackAsync(int pageNumber, int pageSize, int? growthChartId, string? status)
        {
            IQueryable<Feedback> feedbackQuery = _unitOfWork.GetRepository<Feedback>().Entities.AsNoTracking();

           

            if (growthChartId.HasValue)
            {
                feedbackQuery = feedbackQuery.Where(f => f.GrowthChartsID == growthChartId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                feedbackQuery = feedbackQuery.Where(f => f.Status == status);
            }

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
