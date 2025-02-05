using BabyCare.Contract.Repositories.Entity;
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
using Microsoft.AspNetCore.Identity;
using Azure.Core;
using BabyCare.ModelViews.AuthModelViews.Response;
using static BabyCare.Core.Utils.SystemConstant;
using Microsoft.AspNetCore.Http;
using Azure;
using BabyCare.ModelViews.AppointmentModelViews.Response;
using BabyCare.Core.Utils;
using BabyCare.ModelViews.AppointmentModelViews.Request;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Response;
using BabyCare.ModelViews.ChildModelView;
using BabyCare.ModelViews.UserModelViews.Response;
using System.Reflection;
using BabyCare.ModelViews.FeedbackModelView;
using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.Services.Service
{
    public class GrowthChartService : IGrowthChartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public readonly UserManager<ApplicationUsers> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;


        public GrowthChartService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUsers> userManager)
        {
            _contextAccessor = httpContextAccessor;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
        public async Task<ApiResult<BasePaginatedList<GrowthChartModelView>>> GetMyGrowthChartPagination(ModelViews.AppointmentModelViews.Request.SearchAppointmentByUserId request)
        {
            var query = _unitOfWork.GetRepository<GrowthChart>().Entities.AsQueryable();
            query = query.Where(x => (x.Status == (int)GrowthChartStatus.Shared || x.Status == (int)GrowthChartStatus.Answered) && x.DeletedBy == null && x.Child.UserId == request.userId);
            // 1. Áp dụng bộ lọc (Filtering)
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                query = query.Where(a => a.Topic.ToLower().Contains(request.SearchValue.ToLower()) ||
                                        a.Question.ToLower().Contains(request.SearchValue.ToLower())
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

                if (!PropertyExists(normalizedSortBy, typeof(GrowthChart)))
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




            var res = new List<GrowthChartModelView>();
            foreach (var existingItem in data)
            {


                var added = _mapper.Map<GrowthChartModelView>(existingItem);

                if (Enum.IsDefined(typeof(GrowthChartStatus), existingItem.Status))
                {
                    added.Status = ((GrowthChartStatus)existingItem.Status).ToString();
                }
                else
                {
                    added.Status = "Unknown";
                }
                // Map Child entity sang ChildModelViewAddeRecords
                added.childModelView = _mapper.Map<ChildModelViewAddeRecords>(existingItem.Child);

                // Lấy các FGR liên quan và map cùng tiêu chuẩn
                var fgrs = await _unitOfWork.GetRepository<FetalGrowthRecord>().Entities
                    .Include(x => x.FetalGrowthStandard) // Đảm bảo include dữ liệu liên quan
                    .Where(x => x.ChildId == existingItem.ChildId)
                    .ToListAsync();

                // Map danh sách FGR sang ModelView
                added.childModelView.FetalGrowthRecordModelViews = _mapper.Map<List<FetalGrowthRecordModelViewAddedStandards>>(fgrs);

                res.Add(added);
            }

            var response = new BasePaginatedList<GrowthChartModelView>(res, total, currentPage, pageSize);
            // return to client
            return new ApiSuccessResult<BasePaginatedList<GrowthChartModelView>>(response);
        }

        public async Task<ApiResult<BasePaginatedList<GrowthChartModelView>>> GetGrowthChartPagination(ModelViews.AppointmentModelViews.Request.SearchOptimizeRequest request)
        {
            var query = _unitOfWork.GetRepository<GrowthChart>().Entities.AsQueryable();
            query = query.Where(x => (x.Status == (int)GrowthChartStatus.Shared || x.Status == (int)GrowthChartStatus.Answered) && x.DeletedBy == null);
            // 1. Áp dụng bộ lọc (Filtering)
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                query = query.Where(a => a.Topic.ToLower().Contains(request.SearchValue.ToLower()) ||
                                        a.Question.ToLower().Contains(request.SearchValue.ToLower())
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

                if (!PropertyExists(normalizedSortBy, typeof(GrowthChart)))
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




            var res = new List<GrowthChartModelView>();
            foreach (var existingItem in data)
            {


                var added = _mapper.Map<GrowthChartModelView>(existingItem);

                if (Enum.IsDefined(typeof(GrowthChartStatus), existingItem.Status))
                {
                    added.Status = ((GrowthChartStatus)existingItem.Status).ToString();
                }
                else
                {
                    added.Status = "Unknown";
                }
                // Map Child entity sang ChildModelViewAddeRecords
                added.childModelView = _mapper.Map<ChildModelViewAddeRecords>(existingItem.Child);

                // Lấy các FGR liên quan và map cùng tiêu chuẩn
                var fgrs = await _unitOfWork.GetRepository<FetalGrowthRecord>().Entities
                    .Include(x => x.FetalGrowthStandard) // Đảm bảo include dữ liệu liên quan
                    .Where(x => x.ChildId == existingItem.ChildId)
                    .ToListAsync();

                // Map danh sách FGR sang ModelView
                added.childModelView.FetalGrowthRecordModelViews = _mapper.Map<List<FetalGrowthRecordModelViewAddedStandards>>(fgrs);

                res.Add(added);
            }

            var response = new BasePaginatedList<GrowthChartModelView>(res, total, currentPage, pageSize);
            // return to client
            return new ApiSuccessResult<BasePaginatedList<GrowthChartModelView>>(response);
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


        public async Task<ApiResult<object>> UpdateView(UpdateViewRequest request)
        {
            var feedback = await _unitOfWork.GetRepository<GrowthChart>().GetByIdAsync(request.Id);
            if (feedback == null)
            {
                return new ApiErrorResult<object>("Growth chart not found");
            }

            feedback.ViewCount += 1;
            _unitOfWork.GetRepository<GrowthChart>().Update(feedback);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Success");
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


            if (Enum.IsDefined(typeof(GrowthChartStatus), entity.Status))
            {
                model.Status = ((GrowthChartStatus)entity.Status).ToString();
            }
            else
            {
                model.Status = "Unknown";
            }
            // Map Child entity sang ChildModelViewAddeRecords
            model.childModelView = _mapper.Map<ChildModelViewAddeRecords>(entity.Child);

            // Lấy các FGR liên quan và map cùng tiêu chuẩn
            var fgrs = await _unitOfWork.GetRepository<FetalGrowthRecord>().Entities
                .Include(x => x.FetalGrowthStandard) // Đảm bảo include dữ liệu liên quan
                .Where(x => x.ChildId == entity.ChildId)
                .ToListAsync();

            // Map danh sách FGR sang ModelView
            model.childModelView.FetalGrowthRecordModelViews = _mapper.Map<List<FetalGrowthRecordModelViewAddedStandards>>(fgrs);
            model.userViewModel = _mapper.Map<UserResponseModel>(entity.Child.User);

            // Retrieve related Feedbacks DETAIL: for independent
            //    var feedbacks = await _unitOfWork.GetRepository<Feedback>().Entities
            //.Where(f => f.GrowthChartsID == entity.Id)
            //.ToListAsync();

            //    var feedbackModelViews = _mapper.Map<List<FeedbackModelView>>(feedbacks);

            //    foreach (var feedback in feedbackModelViews)
            //    {
            //        if (Enum.IsDefined(typeof(FeedbackStatus), feedbacks.First(f => f.Id == feedback.Id).Status))
            //        {
            //            feedback.Status = ((FeedbackStatus)feedbacks.First(f => f.Id == feedback.Id).Status).ToString();
            //        }
            //        else
            //        {
            //            feedback.Status = "Unknown";
            //        }
            //        feedback.User = _mapper.Map<UserResponseModel>(feedbacks.FirstOrDefault(f => f.Id == feedback.Id)?.User);
            //        feedback.ResponseFeedbacks = _mapper.Map<List<FeedbackModelView>>(feedbacks.Where(f => f.ResponseFeedback?.Id == feedback.Id).ToList());
            //    }

            //    model.feedbackModelViews = feedbackModelViews;


            var feedbacks = await _unitOfWork.GetRepository<Feedback>().Entities
    .Where(f => f.GrowthChartsID == entity.Id)
    .ToListAsync();

            var feedbackModelViews = _mapper.Map<List<FeedbackModelView>>(feedbacks);

            // Tạo danh sách feedback cha (không có ParentFeedbackID)
            var parentFeedbacks = feedbackModelViews.Where(f => !feedbacks.Any(fb => fb.Id == f.Id && fb.ResponseFeedbackId.HasValue))
                .OrderByDescending(x => x.CreatedTime)
                 .Take(SystemConstant.MAX_PER_COMMENT)
                .ToList();


            foreach (var feedback in parentFeedbacks)
            {
                // Chuyển đổi Status từ Enum
                var feedbackEntity = feedbacks.FirstOrDefault(f => f.Id == feedback.Id);
                if (feedbackEntity != null && Enum.IsDefined(typeof(FeedbackStatus), feedbackEntity.Status))
                {
                    feedback.Status = ((FeedbackStatus)feedbackEntity.Status).ToString();
                }
                else
                {
                    feedback.Status = "Unknown";
                }

                feedback.User = _mapper.Map<UserResponseModel>(feedbacks.FirstOrDefault(f => f.Id == feedback.Id)?.User);

                // Tìm các feedback con có ParentFeedbackID = feedback.Id
                feedback.ResponseFeedbacks = _mapper.Map<List<FeedbackModelView>>(
                    feedbacks.Where(f => f.ResponseFeedbackId == feedback.Id).ToList()
                );

                // Chuyển đổi Status của các feedback con
                foreach (var childFeedback in feedback.ResponseFeedbacks)
                {
                    var childEntity = feedbacks.FirstOrDefault(f => f.Id == childFeedback.Id);
                    if (childEntity != null && Enum.IsDefined(typeof(FeedbackStatus), childEntity.Status))
                    {
                        childFeedback.Status = ((FeedbackStatus)childEntity.Status).ToString();
                    }
                    else
                    {
                        childFeedback.Status = "Unknown";
                    }

                    childFeedback.User = _mapper.Map<UserResponseModel>(feedbacks.FirstOrDefault(f => f.Id == childFeedback.Id)?.User);
                }
            }

            model.feedbackModelViews = parentFeedbacks;



            return new ApiSuccessResult<GrowthChartModelView>(model);
        }

        public async Task<ApiResult<GrowthChartCreateResponse>> AddGrowthChartAsync(CreateGrowthChartModelView model)
        {
            // Check 
            var existingChild = await _unitOfWork.GetRepository<Child>().GetByIdAsync(model.ChildId);
            if (existingChild == null)
            {
                return new ApiErrorResult<GrowthChartCreateResponse>("Child is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            var entity = _mapper.Map<GrowthChart>(model);
            entity.ChildId = existingChild.Id;
            entity.CreatedTime = DateTime.Now;
            entity.Status = (int)GrowthChartStatus.Shared;

            await _unitOfWork.GetRepository<GrowthChart>().InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<GrowthChartCreateResponse>(new GrowthChartCreateResponse() { Id = entity.Id},"Growth chart created successfully.");
        }
        public async Task<ApiResult<object>> UpdateGrowthChartStatusByUserAsync(UpdateGrowChartByUser model)
        {

            var entity = await _unitOfWork.GetRepository<GrowthChart>().Entities
                .FirstOrDefaultAsync(g => g.Id == model.GrowthChartId && !g.DeletedTime.HasValue);

            if (entity == null)
            {
                return new ApiErrorResult<object>("Growth chart not found.");
            }
            if (entity.Status == (int)GrowthChartStatus.Blocked)
            {
                return new ApiErrorResult<object>("Growth chart post has been blocked");
            }
            if (entity.Status == (int)GrowthChartStatus.Answered)
            {
                return new ApiErrorResult<object>("Growth chart post has been answered");
            }

            _mapper.Map(model, entity);
            entity.LastUpdatedTime = DateTime.Now;
            entity.LastUpdatedBy = model.UserId.ToString();
            await _unitOfWork.GetRepository<GrowthChart>().UpdateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Growth chart updated successfully.");
        }

        public async Task<ApiResult<object>> UpdateGrowthChartStatusByAdminAsync(UpdateGrowChartByAdmin model)
        {
            if (_contextAccessor.HttpContext?.User?.FindFirst("userId") == null)
            {
                return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
            }
            var existingUser = await _userManager.FindByIdAsync(_contextAccessor.HttpContext?.User?.FindFirst("userId").Value);
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("Account is not found.", System.Net.HttpStatusCode.NotFound);
            }
            var entity = await _unitOfWork.GetRepository<GrowthChart>().Entities
                .FirstOrDefaultAsync(g => g.Id == model.GrowthChartId && !g.DeletedTime.HasValue);

            if (entity == null)
            {
                return new ApiErrorResult<object>("Growth chart not found.");
            }
            _mapper.Map(model, entity);
            entity.LastUpdatedTime = DateTime.Now;
            entity.LastUpdatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            entity.Status = model.Status;

            await _unitOfWork.GetRepository<GrowthChart>().UpdateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Growth chart updated successfully.");
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
            if (_contextAccessor.HttpContext?.User?.FindFirst("userId") == null)
            {
                return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
            }
            var entity = await _unitOfWork.GetRepository<GrowthChart>().Entities
                .FirstOrDefaultAsync(g => g.Id == id && !g.DeletedTime.HasValue);

            if (entity == null)
            {
                return new ApiErrorResult<object>("Growth chart not found.");
            }

            entity.DeletedTime = DateTime.Now;
            entity.DeletedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId").Value;
            await _unitOfWork.GetRepository<GrowthChart>().UpdateAsync(entity);
            await _unitOfWork.SaveAsync();

            return new ApiSuccessResult<object>("Growth chart deleted successfully.");
        }



        public async Task<ApiResult<List<GrowthChartModelView>>> GetAllGrowthChartsAdminAsync()
        {
            IQueryable<GrowthChart> query = _unitOfWork.GetRepository<GrowthChart>().Entities
               .AsNoTracking()
               .Where(g => !g.DeletedTime.HasValue && g.Status != (int)GrowthChartStatus.Unshared)
               .OrderByDescending(g => g.CreatedTime);

            int totalCount = await query.CountAsync();

            var list = await query.ToListAsync();
            var modelList = list.Select(x => new GrowthChartModelView()
            {
                Id = x.Id,
                Question = x.Question,
                Topic = x.Topic,
                Status = (Enum.IsDefined(typeof(GrowthChartStatus), x.Status)) ? ((GrowthChartStatus)x.Status).ToString() : "Unknown",
                ViewCount = x.ViewCount,
                childModelView = _mapper.Map<ChildModelViewAddeRecords>(x.Child)
            }).ToList();

           

            return new ApiSuccessResult<List<GrowthChartModelView>>(modelList);
        }

        public ApiResult<List<GrowthChartStatusResponse>> GetGrowthChartsStatusHandler(bool isAdminUpdate)
        {
            var statusList = new List<GrowthChartStatusResponse>();

            if (isAdminUpdate)
            {
                statusList = Enum.GetValues(typeof(GrowthChartStatus))
                    .Cast<GrowthChartStatus>()
                    .Where(x => x != GrowthChartStatus.Unshared)
                    .Select(status => new GrowthChartStatusResponse
                    {
                        Id = (int)status,
                        Status = status.ToString()
                    }).ToList();
            }
            else
            {
                statusList = Enum.GetValues(typeof(GrowthChartStatus))
                   .Cast<GrowthChartStatus>()
                   .Where(x => x == GrowthChartStatus.Shared || x == GrowthChartStatus.Answered)
                   .Select(status => new GrowthChartStatusResponse
                   {
                       Id = (int)status,
                       Status = status.ToString()
                   }).ToList();
            }

            return new ApiSuccessResult<List<GrowthChartStatusResponse>>(statusList);

        }
    }
}
