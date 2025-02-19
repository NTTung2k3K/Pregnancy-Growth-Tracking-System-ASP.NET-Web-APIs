using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.BlogTypeModelView;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BabyCare.Services.Service
{
    public class BlogTypeService : IBlogTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public BlogTypeService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
        }

        public async Task<ApiResult<object>> AddBlogTypeAsync(CreateBlogTypeModelView model)
        {
            var existedBlogType = await _unitOfWork.GetRepository<BlogType>()
                .Entities
                .FirstOrDefaultAsync(r => r.Name.Equals(model.Name) && !r.DeletedTime.HasValue);

            if (existedBlogType != null)
            {
                return new ApiErrorResult<object>("Blog type already exists");
            }

            BlogType newBlogType = _mapper.Map<BlogType>(model);

            if (model.Thumbnail != null)
            {
                newBlogType.Thumbnail = await BabyCare.Core.Firebase.ImageHelper.Upload(model.Thumbnail);
            }


            /*if (userId != null)
			{
				newRole.CreatedBy = userId;
			}
			else
			{
				newRole.CreatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
			}*/

            newBlogType.CreatedBy = model.Name;
            newBlogType.CreatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<BlogType>().InsertAsync(newBlogType);

            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Role added successfully");
        }

        public async Task<ApiResult<object>> DeleteBlogTypeAsync(int id)
        {
            if (id <= 0)
            {
                return new ApiErrorResult<object>("Please provide a valid Blog Type ID.");
            }

            var existingBlogType = await _unitOfWork.GetRepository<BlogType>().Entities
                .FirstOrDefaultAsync(bt => bt.Id == id && !bt.DeletedTime.HasValue);

            if (existingBlogType == null)
            {
                return new ApiErrorResult<object>("The Blog Type cannot be found or has already been deleted!");
            }

            existingBlogType.DeletedTime = DateTimeOffset.UtcNow;

            // Set the user who deleted this Blog Type
            //existingBlogType.DeletedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            existingBlogType.DeletedBy = existingBlogType.Name;

            await _unitOfWork.GetRepository<BlogType>().UpdateAsync(existingBlogType);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Blog Type successfully deleted.");
        }

        public async Task<ApiResult<BasePaginatedList<BlogTypeModelView>>> GetAllBlogTypeAsync(int pageNumber, int pageSize, int? id, string? name)
        {
            IQueryable<BlogType> blogTypeQuery = _unitOfWork.GetRepository<BlogType>().Entities
                .AsNoTracking()
                .Where(p => !p.DeletedTime.HasValue);

            if (id != null)
                blogTypeQuery = blogTypeQuery.Where(p => p.Id == id);

            if (!string.IsNullOrWhiteSpace(name))
                blogTypeQuery = blogTypeQuery.Where(p => p.Name.Contains(name));

            blogTypeQuery = blogTypeQuery.OrderByDescending(r => r.CreatedTime);

            int totalCount = await blogTypeQuery.CountAsync();

            List<BlogType> paginatedBlogTyples = await blogTypeQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<BlogTypeModelView> blogTypleModelViews = _mapper.Map<List<BlogTypeModelView>>(paginatedBlogTyples);
            var result = new BasePaginatedList<BlogTypeModelView>(blogTypleModelViews, totalCount, pageNumber, pageSize);

            return new ApiSuccessResult<BasePaginatedList<BlogTypeModelView>>(result);
        }

        public async Task<ApiResult<BlogTypeModelView>> GetBlogTypeByIdAsync(int id)
        {
            if (id <= 0)
            {
                return new ApiErrorResult<BlogTypeModelView>("Please provide a valid Blog Type ID.");
            }

            var blogTypeEntity = await _unitOfWork.GetRepository<BlogType>().Entities
                .FirstOrDefaultAsync(blogType => blogType.Id == id && !blogType.DeletedTime.HasValue);

            if (blogTypeEntity == null)
            {
                return new ApiErrorResult<BlogTypeModelView>("Blog Type does not exist.");
            }

            BlogTypeModelView blogTypeModelView = _mapper.Map<BlogTypeModelView>(blogTypeEntity);
            return new ApiSuccessResult<BlogTypeModelView>(blogTypeModelView);
        }


        public async Task<ApiResult<object>> UpdateBlogTypeAsync(int id, UpdateBlogTypeModelView model)
        {
            if (id <= 0)
            {
                return new ApiErrorResult<object>("Please provide a valid Blog Type ID.");
            }

            var existingBlogType = await _unitOfWork.GetRepository<BlogType>().Entities
                .FirstOrDefaultAsync(bt => bt.Id == id && !bt.DeletedTime.HasValue);

            if (existingBlogType == null)
            {
                return new ApiErrorResult<object>("The Blog Type cannot be found or has been deleted!");
            }

            bool isUpdated = false;

            // Check and update Name
            if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != existingBlogType.Name)
            {
                var blogTypeWithSameName = await _unitOfWork.GetRepository<BlogType>().Entities
                    .AnyAsync(bt => bt.Name == model.Name && !bt.DeletedTime.HasValue);

                if (blogTypeWithSameName)
                {
                    return new ApiErrorResult<object>("A Blog Type with the same name already exists.");
                }

                existingBlogType.Name = model.Name;
                isUpdated = true;
            }

            // Check and update Description
            if (!string.IsNullOrWhiteSpace(model.Description) && model.Description != existingBlogType.Description)
            {
                existingBlogType.Description = model.Description;
                isUpdated = true;
            }

            // Check and process Thumbnail
            if (model.Thumbnail != null)
            {
                existingBlogType.Thumbnail = await BabyCare.Core.Firebase.ImageHelper.Upload(model.Thumbnail);
                isUpdated = true;
            }

            if (isUpdated)
            {
                //existingBlogType.LastUpdatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
                existingBlogType.LastUpdatedBy = model.Name;
                existingBlogType.LastUpdatedTime = DateTimeOffset.UtcNow;

                await _unitOfWork.GetRepository<BlogType>().UpdateAsync(existingBlogType);
                await _unitOfWork.SaveAsync();

                return new ApiSuccessResult<object>("Blog Type successfully updated.");
            }

            return new ApiErrorResult<object>("Blog Type successfully updated.");
        }
    }
}
