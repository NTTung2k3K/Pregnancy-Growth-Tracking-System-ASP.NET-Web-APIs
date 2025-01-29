using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.Core;
using BabyCare.ModelViews.AppointmentModelViews.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabyCare.ModelViews.AppointmentModelViews.Request;

namespace BabyCare.Contract.Services.Interface
{
    public interface IAppointmentService
    {
        Task<ApiResult<object>> CreateAppointment(CreateAppointmentRequest request);
        Task<ApiResult<object>> UpdateAppointment(UpdateAppointmentRequest request);
        Task<ApiResult<object>> UpdateByDoctorAppointment(UpdateAppointmentByDoctorRequest request);

        Task<ApiResult<BasePaginatedList<AppointmentResponseModel>>> GetAppointmentsPagination(BaseSearchRequest request);
        Task<ApiResult<object>> DeleteAppointment(DeleteAppointmentRequest request);
        Task<ApiResult<AppointmentResponseModelV2>> GetAppointmentById(int id);
        Task<ApiResult<List<AppointmentResponseModel>>> GetAppointmentsByUserId(Guid userId);
        Task<ApiResult<List<AppointmentResponseModel>>> GetAll();

        Task<ApiResult<List<AppointmentResponseModel>>> GetAppointmentsByUserIdInRange(Guid userId,DateTime startDay, DateTime endDate);


        ApiResult<List<AppointmentStatusResponseModel>> GetAppointmentStatusHandler();

        Task<ApiResult<object>> UpdateCancelAppointmentStatusByUser(CancelAppointmentByUser request);
        Task<ApiResult<object>> UpdateNoShowAppointmentStatusByDoctor(NoShowAppointmentByDoctor request);
        Task<ApiResult<AvailableSlotResponseModel>> GetSlotAvailable(DateTime date);


    }
}
