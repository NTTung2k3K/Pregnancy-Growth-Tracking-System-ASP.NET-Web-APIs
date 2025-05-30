﻿using BabyCare.Core.APIResponse;
using BabyCare.Core;
using BabyCare.ModelViews.BlogModelViews;
using BabyCare.ModelViews.AppointmentModelViews.Request;

namespace BabyCare.Contract.Services.Interface
{
    public interface IBlogService
    {
        ApiResult<object> GetBlogStatusHandler();

        Task<ApiResult<BasePaginatedList<BlogModelView>>> GetBlogPagination(SearchOptimizeBlogRequest request);

        Task<ApiResult<BasePaginatedList<BlogModelView>>> GetAllBlogAsync(
            int pageNumber,
            int pageSize,
            int? id ,
            string? title,
            string? status,
            bool? isFeatured);

        Task<ApiResult<BasePaginatedList<BlogModelView>>> GetAllBlogAdminAsync(
            int pageNumber,
            int pageSize,
            int? id,
            string? title,
            string? status,
            bool? isFeatured);
        Task<ApiResult<object>> AddBlogAsync(CreateBlogModelView model);
        Task<ApiResult<object>> UpdateBlogAsync(int id, UpdateBlogModelView model);
        Task<ApiResult<object>> DeleteBlogAsync(int id);
        Task<ApiResult<object>> UpdateQuantity(UpdateQuantityRequest request);


        Task<ApiResult<BlogModelView>> GetBlogByIdAsync(int id);
        Task<ApiResult<BasePaginatedList<BlogModelView>>> GetBlogByWeekAsync(SeachOptimizeBlogByWeek request);


        Task<ApiResult<List<object>>> GetBlogCountByMonthAsync();
        Task<ApiResult<List<BlogModelView>>> GetMostViewedBlogsAsync(int quantity);
        Task<ApiResult<List<BlogModelView>>> GetMostLikedBlogAsync(int quantity);


    }
}
