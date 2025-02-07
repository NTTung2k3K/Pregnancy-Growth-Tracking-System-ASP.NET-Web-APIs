using AutoMapper;
using Azure.Core;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Firebase;
using BabyCare.Core.Utils;
using BabyCare.ModelViews.AppointmentModelViews.Request;
using BabyCare.ModelViews.AppointmentModelViews.Response;
using BabyCare.ModelViews.BlogModelViews;
using BabyCare.ModelViews.BlogTypeModelView;
using BabyCare.ModelViews.GrowthChartModelView;
using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;


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
            newBlog.LikesCount = 0;
            newBlog.ViewCount = 0;
            newBlog.Status = model.Status;
            

            // Lưu Blog vào cơ sở dữ liệu
            await _unitOfWork.GetRepository<Blog>().InsertAsync(newBlog);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Blog added successfully.");
        }

        public ApiResult<object> GetBlogStatusHandler()
        {
            var statusList = Enum.GetValues(typeof(BlogStatus))
                     .Cast<BlogStatus>()
                     .Select(status => new MPStatusResponseModel
                     {
                         Id = (int)status,
                         Status = status.ToString()
                     })
                     .ToList();
            return new ApiSuccessResult<object>(statusList);
        }
        public async Task<ApiResult<object>> DeleteBlogAsync(int id)
        {
            if (_contextAccessor.HttpContext?.User?.FindFirst("userId") == null)
            {
                return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
            }
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
            blog.DeletedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value; // Hoặc có thể là userId của người xóa nếu có thông tin

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
                .Where(b => !b.DeletedTime.HasValue && b.Status == (int)BlogStatus.Active); // Loại bỏ các bản ghi đã bị xóa

            // Áp dụng bộ lọc theo id, title, status, và isFeatured nếu có
            if (id != null)
                blogQuery = blogQuery.Where(b => b.Id == id);

            if (!string.IsNullOrWhiteSpace(title))
                blogQuery = blogQuery.Where(b => b.Title.Contains(title));

            //if (!string.IsNullOrWhiteSpace(status))
            //    blogQuery = blogQuery.Where(b => b.Status.Contains(status));

            //if (isFeatured.HasValue)
            //    blogQuery = blogQuery.Where(b => b.IsFeatured == isFeatured.Value);

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
            List<BlogModelView> blogModelViews = paginatedBlogs.Select(x => new BlogModelView()
            {
                Id = x.Id,
                Content = x.Content,
                LikesCount = x.LikesCount,
                Sources = x.Sources,
                Status = Enum.IsDefined(typeof(BlogStatus), x.Status)
                                 ? ((BlogStatus)x.Status).ToString()
                                    : "Unknown",
                Thumbnail = x.Thumbnail,
                Title = x.Title,
                ViewCount = x.ViewCount,
                Week = x.Week,
                AuthorResponseModel = _mapper.Map<EmployeeResponseModel>(x.Author),
                BlogTypeModelView = _mapper.Map<BlogTypeModelView>(x.BlogType),
                CreatedTime = x.CreatedTime
            }).ToList();

            // Tạo đối tượng phân trang
            var result = new BasePaginatedList<BlogModelView>(blogModelViews, totalCount, pageNumber, pageSize);

            // Trả về kết quả
            return new ApiSuccessResult<BasePaginatedList<BlogModelView>>(result);
        }
        public async Task<ApiResult<BasePaginatedList<BlogModelView>>> GetAllBlogAdminAsync(int pageNumber, int pageSize, int? id, string? title, string? status, bool? isFeatured)
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

            //if (!string.IsNullOrWhiteSpace(status))
            //    blogQuery = blogQuery.Where(b => b.Status.Contains(status));

            //if (isFeatured.HasValue)
            //    blogQuery = blogQuery.Where(b => b.IsFeatured == isFeatured.Value);

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
            List<BlogModelView> blogModelViews = paginatedBlogs.Select(x => new BlogModelView()
            {
                Id = x.Id,
                Content = x.Content,
                LikesCount = x.LikesCount,
                Sources = x.Sources,
                Status = Enum.IsDefined(typeof(BlogStatus), x.Status)
                                 ? ((BlogStatus)x.Status).ToString()
                                    : "Unknown",
                Thumbnail = x.Thumbnail,
                Title = x.Title,
                ViewCount = x.ViewCount,
                Week = x.Week,
                AuthorResponseModel = _mapper.Map<EmployeeResponseModel>(x.Author),
                BlogTypeModelView = _mapper.Map<BlogTypeModelView>(x.BlogType),
                CreatedTime = x.CreatedTime

            }).ToList();

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
            blogModelView.AuthorResponseModel = _mapper.Map<EmployeeResponseModel>(blogEntity.Author);
            blogModelView.BlogTypeModelView = _mapper.Map<BlogTypeModelView>(blogEntity.BlogType);


            return new ApiSuccessResult<BlogModelView>(blogModelView);
        }

        public async Task<ApiResult<List<BlogModelView>>> GetBlogByWeekAsync(int week)
        {
            IQueryable<Blog> blogQuery = _unitOfWork.GetRepository<Blog>().Entities
                .AsNoTracking()
                .Where(b => !b.DeletedTime.HasValue && (b.Week!= null && b.Week  == week) && b.Status == (int)BlogStatus.Active); // Loại bỏ các bản ghi đã bị xóa

            // Áp dụng bộ lọc theo id, title, status, và isFeatured nếu có
            

            // Sắp xếp theo thời gian tạo giảm dần
            blogQuery = blogQuery.OrderByDescending(b => b.CreatedTime);

            // Lấy tổng số lượng bản ghi

            // Lấy dữ liệu phân trang
            List<Blog> paginatedBlogs = await blogQuery.ToListAsync();

            // Chuyển đổi từ Blog sang BlogModelView
            List<BlogModelView> blogModelViews = paginatedBlogs.Select(x => new BlogModelView()
            {
                Id = x.Id,
                Content = x.Content,
                LikesCount = x.LikesCount,
                Sources = x.Sources,
                Status = Enum.IsDefined(typeof(BlogStatus), x.Status)
                                 ? ((BlogStatus)x.Status).ToString()
                                    : "Unknown",
                Thumbnail = x.Thumbnail,
                Title = x.Title,
                ViewCount = x.ViewCount,
                Week = x.Week,
                AuthorResponseModel = _mapper.Map<EmployeeResponseModel>(x.Author),
                BlogTypeModelView = _mapper.Map<BlogTypeModelView>(x.BlogType),
                CreatedTime = x.CreatedTime

            }).ToList();

            // Tạo đối tượng phân trang
            return new ApiSuccessResult<List<BlogModelView>>(blogModelViews,"Blog updated successfully.");

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
            var existingImage = existingBlog.Thumbnail;

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

            //if (model.LikesCount.HasValue && model.LikesCount != existingBlog.LikesCount)
            //{
            //    existingBlog.LikesCount = model.LikesCount.Value;
            //    isUpdated = true;
            //}

            //if (model.ViewCount.HasValue && model.ViewCount != existingBlog.ViewCount)
            //{
            //    existingBlog.ViewCount = model.ViewCount.Value;
            //    isUpdated = true;
            //}

            if (model.Status != existingBlog.Status)
            {
                existingBlog.Status = model.Status;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(model.Sources) && model.Sources != existingBlog.Sources)
            {
                existingBlog.Sources = model.Sources;
                isUpdated = true;
            }

            

            if (model.BlogTypeId.HasValue && model.BlogTypeId != existingBlog.BlogTypeId)
            {
                existingBlog.BlogTypeId = model.BlogTypeId.Value;
                isUpdated = true;
            }

            //if (model.IsFeatured.HasValue && model.IsFeatured != existingBlog.IsFeatured)
            //{
            //    existingBlog.IsFeatured = model.IsFeatured.Value;
            //    isUpdated = true;
            //}
            if (model.Week.HasValue && model.Week != existingBlog.Week)
            {
                existingBlog.Week = model.Week.Value;
                isUpdated = true;
            }
            if(model.Thumbnail != null)
            {
                isUpdated = true;
            }


            // Nếu có thay đổi, cập nhật thông tin và lưu vào DB
            if (isUpdated)
            {
                existingBlog.LastUpdatedTime = DateTimeOffset.UtcNow;
                // Bạn có thể sử dụng thông tin userId từ context nếu cần
                // existingBlog.LastUpdatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
                if (model.Thumbnail != null)
                {
                    existingBlog.Thumbnail = await ImageHelper.Upload(model.Thumbnail);
                }
                else
                {
                    existingBlog.Thumbnail = existingImage;
                }

                await _unitOfWork.GetRepository<Blog>().UpdateAsync(existingBlog);
                await _unitOfWork.SaveAsync();

                return new ApiSuccessResult<object>("Blog updated successfully.");
            }

            return new ApiErrorResult<object>("Blog updated successfully.");
        }

        private string NormalizePropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return propertyName;

            return char.ToUpper(propertyName[0]) + propertyName.Substring(1);
        }
        private bool PropertyExists(string propertyName, Type entityType)
        {
            // Kiểm tra nếu thuộc tính tồn tại trong entity
            var property = entityType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return property != null;
        }
        public async Task<ApiResult<BasePaginatedList<BlogModelView>>> GetBlogPagination(SearchOptimizeBlogRequest request)
        {
            var query = _unitOfWork.GetRepository<Blog>().Entities.AsQueryable();
            query = query.Where(x => (x.Status == (int)BlogStatus.Active  && x.DeletedBy == null));
            if(request.BlogTypeId != null)
            {
                query = query.Where(x => (x.BlogTypeId == request.BlogTypeId));
            }

            // 1. Áp dụng bộ lọc (Filtering)
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                query = query.Where(a => a.Title.ToLower().Contains(request.SearchValue.ToLower()) ||
                                        a.Content.ToLower().Contains(request.SearchValue.ToLower()) ||
                                        (a.Author.FullName != null &&  a.Author.FullName.ToLower().Contains(request.SearchValue.ToLower())) ||
                                        (a.Week != null && a.Week.ToString().Contains(request.SearchValue.ToLower()))
                                        );
            }
            if (request.FromDate.HasValue)
            {
                query = query.Where(a => a.CreatedTime.Date >= request.FromDate.Value.Date);
            }
            if (request.ToDate.HasValue)
            {
                query = query.Where(a => a.CreatedTime.Date <= request.ToDate.Value.Date);
            }
            if (request.Status != null)
            {
                query = query.Where(a => a.Status == request.Status);
            }

            if (request.SortBy != null)
            {
                var normalizedSortBy = NormalizePropertyName(request.SortBy);

                if (!PropertyExists(normalizedSortBy, typeof(Blog)))
                {
                    throw new ArgumentException($"Property '{request.SortBy}' does not exist on the GrowthChart entity.");
                }

                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    if (normalizedSortBy == "ViewCount")
                    {
                        // 🔥 Sort đúng kiểu dữ liệu (int)
                        query = request.IsDescending
                            ? query.OrderByDescending(a => a.ViewCount)
                            : query.OrderBy(a => a.ViewCount);
                    }else if (normalizedSortBy == "LikesCount")
                    {
                        // 🔥 Sort đúng kiểu dữ liệu (int)
                        query = request.IsDescending
                            ? query.OrderByDescending(a => a.ViewCount)
                            : query.OrderBy(a => a.ViewCount);
                    }
                    else
                    {
                        // 🔥 Sort các field khác (vẫn giữ logic cũ)
                        query = request.IsDescending
                            ? query.OrderByDescending(a => EF.Property<object>(a, normalizedSortBy))
                            : query.OrderBy(a => EF.Property<object>(a, normalizedSortBy));
                    }
                }
            }
            else
            {
                query = query.OrderByDescending(a => a.CreatedTime);
            }

            // 3. Tổng số bản ghi
            var totalRecords = await query.CountAsync();
            var currentPage = request.PageIndex ?? 1;
            var pageSize = request.PageSize ?? SystemConstant.PAGE_SIZE;
            var total = await query.CountAsync();
            // 4. Áp dụng phân trang (Pagination)
            var data = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();




            var res = new List<BlogModelView>();
            foreach (var existingItem in data)
            {


                var added = _mapper.Map<BlogModelView>(existingItem);

                if (Enum.IsDefined(typeof(BlogStatus), existingItem.Status))
                {
                    added.Status = ((BlogStatus)existingItem.Status).ToString();
                }
                else
                {
                    added.Status = "Unknown";
                }


                added.AuthorResponseModel = _mapper.Map<EmployeeResponseModel>(existingItem.Author);
                added.BlogTypeModelView = _mapper.Map<BlogTypeModelView>(existingItem.BlogType);

                res.Add(added);
            }

            var response = new BasePaginatedList<BlogModelView>(res, total, currentPage, pageSize);
            // return to client
            return new ApiSuccessResult<BasePaginatedList<BlogModelView>>(response);
        }
        public async Task<ApiResult<List<object>>> GetBlogCountByMonthAsync()
        {
            var currentYear = DateTime.UtcNow.Year;
            var blogCounts = new List<object>();

            for (int month = 1; month <= 12; month++)
            {
                var count = await _unitOfWork.GetRepository<Blog>().Entities
                    .Where(b => b.CreatedTime.Year == currentYear && b.CreatedTime.Month == month && !b.DeletedTime.HasValue)
                    .CountAsync();

                blogCounts.Add(new { month, quantity = count });
            }

            return new ApiSuccessResult<List<object>>(blogCounts);
        }

        public async Task<ApiResult<List<BlogModelView>>> GetMostViewedBlogsAsync(int quantity)
        {
            var mostViewedBlogs = await _unitOfWork.GetRepository<Blog>().Entities
                .AsNoTracking()
                .Where(b => !b.DeletedTime.HasValue)
                .OrderByDescending(b => b.ViewCount)
                .Take(quantity)
                .ToListAsync();

            if (!mostViewedBlogs.Any())
            {
                return new ApiErrorResult<List<BlogModelView>>("No blogs found.");
            }

            var blogModelViews = mostViewedBlogs.Select(blog =>
            {
                var blogModelView = _mapper.Map<BlogModelView>(blog);
                blogModelView.AuthorResponseModel = _mapper.Map<EmployeeResponseModel>(blog.Author);
                blogModelView.BlogTypeModelView = _mapper.Map<BlogTypeModelView>(blog.BlogType);
                return blogModelView;
            }).ToList();

            return new ApiSuccessResult<List<BlogModelView>>(blogModelViews);
        }


        public async Task<ApiResult<List<BlogModelView>>> GetMostLikedBlogAsync(int quantity)
        {
            var mostViewedBlogs = await _unitOfWork.GetRepository<Blog>().Entities
                .AsNoTracking()
                .Where(b => !b.DeletedTime.HasValue)
                .OrderByDescending(b => b.LikesCount)
                .Take(quantity)
                .ToListAsync();

            if (!mostViewedBlogs.Any())
            {
                return new ApiErrorResult<List<BlogModelView>>("No blogs found.");
            }

            var blogModelViews = mostViewedBlogs.Select(blog =>
            {
                var blogModelView = _mapper.Map<BlogModelView>(blog);
                blogModelView.AuthorResponseModel = _mapper.Map<EmployeeResponseModel>(blog.Author);
                blogModelView.BlogTypeModelView = _mapper.Map<BlogTypeModelView>(blog.BlogType);
                return blogModelView;
            }).ToList();

            return new ApiSuccessResult<List<BlogModelView>>(blogModelViews);
        }
        
    }

}
