using AutoMapper;
using Azure.Core;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.Core.Firebase;
using BabyCare.Core.Utils;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Request;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Response;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.Services.Service
{
    public class AppointmentTemplateService : IAppointmentTemplateService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;

        public AppointmentTemplateService(IHttpContextAccessor httpContextAccessor,IUnitOfWork unitOfWork, IMapper mapper)
        {
            _contextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ApiResult<object>> CreateAppointmentTemplate(CreateATRequest request)
        {
            var repo = _unitOfWork.GetRepository<AppointmentTemplates>();
            // Check name is existed
            if (string.IsNullOrEmpty(request.Name))
            {
                return new ApiErrorResult<object>("Name is required.");
            }
            var existingName = await repo.Entities.FirstOrDefaultAsync(x => x.Name == request.Name);
            if (existingName != null)
            {
                return new ApiErrorResult<object>("Name of appointment template is existed.");
            }
            var appointmentTemplates = _mapper.Map<AppointmentTemplates>(request);

            if (request.DaysFromBirth <= 0)
            {
                return new ApiErrorResult<object>("DaysFromBirth is not valid.");
            }
            if (request.Image != null)
            {
                appointmentTemplates.Image = await ImageHelper.Upload(request.Image);
            }
            appointmentTemplates.Status = (int)AppointmentTemplatesStatus.Active;
            await repo.InsertAsync(appointmentTemplates);
            await repo.SaveAsync();
            return new ApiSuccessResult<object>("Create successfully.");
        }

        public async Task<ApiResult<object>> DeleteAppointmentTemplate(DeleteATRequest request)
        {
            var repo = _unitOfWork.GetRepository<AppointmentTemplates>();
            // Check mp is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null)
            {
                return new ApiErrorResult<object>("Appointment template is not existed.");
            }
            existingItem.DeletedTime = DateTime.Now;
            existingItem.DeletedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            await repo.UpdateAsync(existingItem);
            await repo.SaveAsync();

            return new ApiSuccessResult<object>("Delete successfully.");
        }

        public async Task<ApiResult<List<ATResponseModel>>> GetAll()
        {
            var items = _unitOfWork.GetRepository<AppointmentTemplates>().Entities.Where(x => x.DeletedBy == null);

         
            var data = await items.OrderBy(x => x.DaysFromBirth).ToListAsync();
            var res = data.Select(x => new ATResponseModel
            {
                Id = x.Id,
                DaysFromBirth = x.DaysFromBirth,
                Name = x.Name,
                Image = x.Image,
                Status = Enum.IsDefined(typeof(AppointmentTemplatesStatus), x.Status)
                                 ? ((AppointmentTemplatesStatus)x.Status).ToString()
                                    : "Unknown",

                Description = x.Description,
                
            }).ToList();

            // return to client
            return new ApiSuccessResult<List<ATResponseModel>>(res);
        }

        public async Task<ApiResult<ATResponseModel>> GetAppointmentTemplateById(int id)
        {
            var repo = _unitOfWork.GetRepository<AppointmentTemplates>();
            // Check mp is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<ATResponseModel>("Appointment templates is not existed.");
            }
            var response = _mapper.Map<ATResponseModel>(existingItem);
            if (Enum.IsDefined(typeof(AppointmentTemplatesStatus), existingItem.Status))
            {
                response.Status = ((AppointmentTemplatesStatus)existingItem.Status).ToString();
            }
            else
            {
                response.Status = "Unknown";
            }
           
            return new ApiSuccessResult<ATResponseModel>(response);
        }

        public async Task<ApiResult<BasePaginatedList<ATResponseModel>>> GetAppointmentTemplatesPagination(BaseSearchRequest request)
        {


            var items = _unitOfWork.GetRepository<AppointmentTemplates>().Entities.Where(x => x.DeletedBy == null);

            // filter by search 
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                items = items.Where(x => x.Name.ToLower().Contains(request.SearchValue.ToLower()));
            }
            // paging
            var currentPage = request.PageIndex ?? 1;
            var pageSize = request.PageSize ?? SystemConstant.PAGE_SIZE;
            var total = items.Count();
            var data = await items.Skip((currentPage - 1) * currentPage).Take(pageSize).OrderBy(x => x.DaysFromBirth).ToListAsync();
            // calculate total page

            var res = data.Select(x => new ATResponseModel
            {
                Id = x.Id,
                DaysFromBirth = x.DaysFromBirth,
                Name = x.Name,
                Image = x.Image,
                Status = Enum.IsDefined(typeof(AppointmentTemplatesStatus), x.Status)
                                 ? ((AppointmentTemplatesStatus)x.Status).ToString()
                                    : "Unknown",
                
                Description = x.Description,
                
            }).ToList();

            var response = new BasePaginatedList<ATResponseModel>(res, total, currentPage, pageSize);
            // return to client
            return new ApiSuccessResult<BasePaginatedList<ATResponseModel>>(response);
        }

        public  ApiResult<List<ATStatusResponseModel>> GetAppointmentTemplateStatusHandler()
        {
            var statusList = Enum.GetValues(typeof(AppointmentTemplatesStatus))
                      .Cast<AppointmentTemplatesStatus>()
                      .Select(status => new ATStatusResponseModel
                      {
                          Id = (int)status,
                          Status = status.ToString()
                      })
                      .ToList();


            return new ApiSuccessResult<List<ATStatusResponseModel>>(statusList);
        }

        public async Task<ApiResult<object>> UpdateAppointmentTemplate(UpdateATRequest request)
        {
            var repo = _unitOfWork.GetRepository<AppointmentTemplates>();
            // Check mp is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null)
            {
                return new ApiErrorResult<object>("Appointment template is not existed.");
            }
            // Check status included on enum
            if (!Enum.IsDefined(typeof(SystemConstant.AppointmentTemplatesStatus), request.Status))
            {
                return new ApiErrorResult<object>("Status is not correct.", System.Net.HttpStatusCode.BadRequest);
            }
            var existingImage = existingItem.Image;

            _mapper.Map(request, existingItem);
            existingItem.Status = (int)request.Status;
            existingItem.LastUpdatedTime = DateTime.Now;
            existingItem.LastUpdatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            if (request.Image != null)
            {
                existingItem.Image = await ImageHelper.Upload(request.Image);
            }
            else
            {
                existingItem.Image = existingImage;

            }
            await repo.UpdateAsync(existingItem);
            await repo.SaveAsync();

            return new ApiSuccessResult<object>("Update successfully.");
        }
    }
}
