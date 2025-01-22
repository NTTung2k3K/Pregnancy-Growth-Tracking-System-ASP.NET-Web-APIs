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
        Task<ApiResult<BasePaginatedList<AppointmentResponseModel>>> GetAppointmentsPagination(BaseSearchRequest request);
        Task<ApiResult<object>> DeleteAppointment(DeleteAppointmentRequest request);
        Task<ApiResult<AppointmentResponseModel>> GetAppointmentById(int id);
        ApiResult<List<AppointmentStatusResponseModel>> GetAppointmentStatusHandler();

        Task<ApiResult<object>> UpdateCancelAppointmentStatusByUser(CancelAppointmentByUser request);
        Task<ApiResult<object>> UpdateNoShowAppointmentStatusByDoctor(NoShowAppointmentByDoctor request);
        Task<ApiResult<AvailableSlotResponseModel>> GetSlotAvailable(AvailableSlotRequest request);


    }
}
