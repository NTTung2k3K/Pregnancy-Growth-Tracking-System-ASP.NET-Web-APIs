using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.BlogModelViews;
using BabyCare.ModelViews.BlogTypeModelView;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Services.Service
{
    public class BlogService : IBlogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public BlogService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
        }

        public async Task<ApiResult<object>> AddBlogAsync(CreateBlogModelView model)
        {
            // Kiểm tra nếu một blog với title giống như thế đã tồn tại
            var existingBlog = await _unitOfWork.GetRepository<Blog>()
                .Entities
                .FirstOrDefaultAsync(b => b.Title.Equals(model.Title) && !b.DeletedTime.HasValue);

            if (existingBlog != null)
            {
                return new ApiErrorResult<object>("A blog with the same title already exists.");
            }

            // Ánh xạ từ CreateBlogModelView sang Blog entity
            Blog newBlog = _mapper.Map<Blog>(model);

            // Upload thumbnail nếu có
            if (model.Thumbnail != null)
            {
                newBlog.Thumbnail = await BabyCare.Core.Firebase.ImageHelper.Upload(model.Thumbnail);
            }

            // Thiết lập các trường còn lại
            newBlog.CreatedTime = DateTimeOffset.UtcNow;
            newBlog.CreatedBy = model.AuthorId.ToString();  // Hoặc lấy từ một hệ thống User ID nếu có

            // Chỉnh sửa các thông tin mặc định
            newBlog.LikesCount = model.LikesCount ?? 0;
            newBlog.ViewCount = model.ViewCount ?? 0;

            // Lưu Blog vào cơ sở dữ liệu
            await _unitOfWork.GetRepository<Blog>().InsertAsync(newBlog);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Blog added successfully.");
        }


        public async Task<ApiResult<object>> DeleteBlogAsync(int id)
        {
            // Kiểm tra sự tồn tại của blog
            var blog = await _unitOfWork.GetRepository<Blog>()
                .Entities
                .FirstOrDefaultAsync(b => b.Id == id && !b.DeletedTime.HasValue);

            // Nếu không tìm thấy blog, trả về lỗi
            if (blog == null)
            {
                return new ApiErrorResult<object>("Blog not found or already deleted.");
            }

            // Đánh dấu thời gian xóa (soft delete)
            blog.DeletedTime = DateTimeOffset.UtcNow;

            // Cập nhật lại thông tin xóa của blog
            blog.DeletedBy = "System"; // Hoặc có thể là userId của người xóa nếu có thông tin

            // Cập nhật blog trong cơ sở dữ liệu
            await _unitOfWork.GetRepository<Blog>().UpdateAsync(blog);
            await _unitOfWork.SaveAsync();

            // Trả về thông báo thành công
            return new ApiSuccessResult<object>("Blog successfully deleted.");
        }


        public async Task<ApiResult<BasePaginatedList<BlogModelView>>> GetAllBlogAsync(int pageNumber, int pageSize, int? id, string? title, string? status, bool? isFeatured)
        {
            // Khởi tạo query cơ bản cho bảng Blog
            IQueryable<Blog> blogQuery = _unitOfWork.GetRepository<Blog>().Entities
                .AsNoTracking()
                .Where(b => !b.DeletedTime.HasValue); // Loại bỏ các bản ghi đã bị xóa

            // Áp dụng bộ lọc theo id, title, status, và isFeatured nếu có
            if (id != null)
                blogQuery = blogQuery.Where(b => b.Id == id);

            if (!string.IsNullOrWhiteSpace(title))
                blogQuery = blogQuery.Where(b => b.Title.Contains(title));

            if (!string.IsNullOrWhiteSpace(status))
                blogQuery = blogQuery.Where(b => b.Status.Contains(status));

            if (isFeatured.HasValue)
                blogQuery = blogQuery.Where(b => b.IsFeatured == isFeatured.Value);

            // Sắp xếp theo thời gian tạo giảm dần
            blogQuery = blogQuery.OrderByDescending(b => b.CreatedTime);

            // Lấy tổng số lượng bản ghi
            int totalCount = await blogQuery.CountAsync();

            // Lấy dữ liệu phân trang
            List<Blog> paginatedBlogs = await blogQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Chuyển đổi từ Blog sang BlogModelView
            List<BlogModelView> blogModelViews = _mapper.Map<List<BlogModelView>>(paginatedBlogs);

            // Tạo đối tượng phân trang
            var result = new BasePaginatedList<BlogModelView>(blogModelViews, totalCount, pageNumber, pageSize);

            // Trả về kết quả
            return new ApiSuccessResult<BasePaginatedList<BlogModelView>>(result);
        }


        public async Task<ApiResult<BlogModelView>> GetBlogByIdAsync(int id)
        {
            if (id <= 0)
            {
                return new ApiErrorResult<BlogModelView>("Please provide a valid Blog ID.");
            }

            // Lấy Blog từ cơ sở dữ liệu
            var blogEntity = await _unitOfWork.GetRepository<Blog>().Entities
                .AsNoTracking()
                .FirstOrDefaultAsync(blog => blog.Id == id && !blog.DeletedTime.HasValue);

            if (blogEntity == null)
            {
                return new ApiErrorResult<BlogModelView>("Blog not found or has been deleted.");
            }

            // Chuyển đổi từ Blog sang BlogModelView
            BlogModelView blogModelView = _mapper.Map<BlogModelView>(blogEntity);

            return new ApiSuccessResult<BlogModelView>(blogModelView);
        }


        public async Task<ApiResult<object>> UpdateBlogAsync(int id, UpdateBlogModelView model)
        {
            // Kiểm tra ID hợp lệ
            if (id <= 0)
            {
                return new ApiErrorResult<object>("Please provide a valid Blog ID.");
            }

            // Tìm blog theo ID và kiểm tra xem nó có tồn tại không
            var existingBlog = await _unitOfWork.GetRepository<Blog>()
                .Entities
                .FirstOrDefaultAsync(blog => blog.Id == id && !blog.DeletedTime.HasValue);

            if (existingBlog == null)
            {
                return new ApiErrorResult<object>("Blog not found or has been deleted.");
            }

            // Cập nhật các trường nếu có sự thay đổi
            bool isUpdated = false;

            if (!string.IsNullOrWhiteSpace(model.Title) && model.Title != existingBlog.Title)
            {
                existingBlog.Title = model.Title;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.Content) && model.Content != existingBlog.Content)
            {
                existingBlog.Content = model.Content;
                isUpdated = true;
            }

            if (model.AuthorId.HasValue && model.AuthorId != existingBlog.AuthorId)
            {
                existingBlog.AuthorId = model.AuthorId.Value;
                isUpdated = true;
            }

            if (model.LikesCount.HasValue && model.LikesCount != existingBlog.LikesCount)
            {
                existingBlog.LikesCount = model.LikesCount.Value;
                isUpdated = true;
            }

            if (model.ViewCount.HasValue && model.ViewCount != existingBlog.ViewCount)
            {
                existingBlog.ViewCount = model.ViewCount.Value;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.Status) && model.Status != existingBlog.Status)
            {
                existingBlog.Status = model.Status;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.Sources) && model.Sources != existingBlog.Sources)
            {
                existingBlog.Sources = model.Sources;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.Thumbnail) && model.Thumbnail != existingBlog.Thumbnail)
            {
                existingBlog.Thumbnail = model.Thumbnail;
                isUpdated = true;
            }

            if (model.BlogTypeId.HasValue && model.BlogTypeId != existingBlog.BlogTypeId)
            {
                existingBlog.BlogTypeId = model.BlogTypeId.Value;
                isUpdated = true;
            }

            if (model.IsFeatured.HasValue && model.IsFeatured != existingBlog.IsFeatured)
            {
                existingBlog.IsFeatured = model.IsFeatured.Value;
                isUpdated = true;
            }

            // Nếu có thay đổi, cập nhật thông tin và lưu vào DB
            if (isUpdated)
            {
                existingBlog.LastUpdatedTime = DateTimeOffset.UtcNow;
                // Bạn có thể sử dụng thông tin userId từ context nếu cần
                // existingBlog.LastUpdatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

                await _unitOfWork.GetRepository<Blog>().UpdateAsync(existingBlog);
                await _unitOfWork.SaveAsync();

                return new ApiSuccessResult<object>("Blog updated successfully.");
            }

            return new ApiErrorResult<object>("Blog updated successfully.");
        }
    }
}
