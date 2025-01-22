using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.Core.Firebase;
using BabyCare.Core.Utils;
using BabyCare.ModelViews.AppointmentModelViews.Request;
using BabyCare.ModelViews.AppointmentModelViews.Response;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Response;
using BabyCare.ModelViews.AuthModelViews.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.Services.Service
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUsers> _userManager;

        public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor, UserManager<ApplicationUsers> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }
        public async Task<Guid?> GetRandomDoctorIdAsync()
        {
            var doctors = (await _userManager.GetUsersInRoleAsync(SystemConstant.Role.DOCTOR)).ToList();

            // Kiểm tra nếu danh sách trống
            if (doctors == null || !doctors.Any())
            {
                return null; 
            }

            // Lấy ngẫu nhiên một bác sĩ
            var random = new Random();
            var randomDoctor = doctors[random.Next(doctors.Count)];

            // Trả về ID của bác sĩ ngẫu nhiên
            return randomDoctor.Id;
        }
        public async Task<ApiResult<object>> CreateAppointment(CreateAppointmentRequest request)
        {
            var repoAppointment = _unitOfWork.GetRepository<Appointment>();
            var repoAppointmentUser = _unitOfWork.GetRepository<AppointmentUser>();
            var repoChild = _unitOfWork.GetRepository<Child>();
            var repoAT = _unitOfWork.GetRepository<AppointmentTemplates>();



            // Check user is existed
            var existingUser = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check child is existed
            var existingChild = await repoChild.GetByIdAsync(request.ChildId);
            if (existingChild == null)
            {
                return new ApiErrorResult<object>("Child is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check appointment template is existed
            var existingAT = await repoAT.GetByIdAsync(request.AppointmentTemplateId);
            if (existingAT == null)
            {
                return new ApiErrorResult<object>("Appointment Type is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check available slot
            var slotOnDay = repoAppointment.Entities.Where(x => x.AppointmentDate == request.AppointmentDate);
            if(slotOnDay.Count() == SystemConstant.MAX_SLOT_AVAILABLE_APPOINTMENT)
            {
                return new ApiErrorResult<object>("Day "+request.AppointmentDate.ToShortDateString() + " is full of slot. Please choose another day!", System.Net.HttpStatusCode.NotFound);
            }
            foreach (var item in slotOnDay)
            {
                if(item.AppointmentSlot == request.AppointmentSlot)
                {
                    return new ApiErrorResult<object>("Slot has been selected by other. Please choose another slot", System.Net.HttpStatusCode.NotFound);
                }
            }
            _unitOfWork.BeginTransaction();
            var appointment = new Appointment()
            {
                AppointmentTemplateId = request.AppointmentTemplateId,
                AppointmentSlot = request.AppointmentSlot,
                AppointmentDate = request.AppointmentDate,
                Status = (int)AppointmentStatus.Confirmed,
                Fee = existingAT.Fee,
                Notes = request.Notes,
            };
            await repoAppointment.InsertAsync(appointment);
            await repoAppointment.SaveAsync();
            var doctorId = await GetRandomDoctorIdAsync();
            var appointmentUser = new AppointmentUser()
            {
                Appointment = appointment,
                CreatedTime = DateTime.Now,
                Description = $"{existingAT.Name}_{appointment.AppointmentDate.ToShortDateString}_Slot:{appointment.AppointmentSlot}_Estimate Cost:{appointment.Fee}",
                UserId = request.UserId,
                DoctorId = doctorId != null ? doctorId.Value : null,
            };
            await repoAppointmentUser.InsertAsync(appointmentUser);
            await repoAppointment.SaveAsync();
            _unitOfWork.CommitTransaction();
            return new ApiSuccessResult<object>("Create successfully.");
        }



        public async Task<ApiResult<object>> DeleteAppointment(DeleteAppointmentRequest request)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            // Check mp is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null)
            {
                return new ApiErrorResult<object>("Appointment is not existed.");
            }
            existingItem.DeletedTime = DateTime.Now;
            existingItem.DeletedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            await repo.UpdateAsync(existingItem);
            await repo.SaveAsync();

            return new ApiSuccessResult<object>("Delete successfully.");
        }

        public async Task<ApiResult<AppointmentResponseModel>> GetAppointmentById(int id)
        {
            var repo = _unitOfWork.GetRepository<Appointment>();
            // Check mp is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<AppointmentResponseModel>("Appointment is not existed.");
            }
            var response = _mapper.Map<AppointmentResponseModel>(existingItem);


            if (Enum.IsDefined(typeof(AppointmentTemplatesStatus), existingItem.Status))
            {
                response.Status = ((AppointmentTemplatesStatus)existingItem.Status).ToString();
            }
            else
            {
                response.Status = "Unknown";
            }

            return new ApiSuccessResult<AppointmentResponseModel>(response);
        }

        public Task<ApiResult<BasePaginatedList<AppointmentResponseModel>>> GetAppointmentsPagination(BaseSearchRequest request)
        {
            throw new NotImplementedException();
        }

        public ApiResult<List<AppointmentStatusResponseModel>> GetAppointmentStatusHandler()
        {
            throw new NotImplementedException();
        }

        public Task<ApiResult<object>> UpdateAppointment(UpdateAppointmentRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
