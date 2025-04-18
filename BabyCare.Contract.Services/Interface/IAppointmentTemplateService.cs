﻿using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.Core;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Response;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Request;

namespace BabyCare.Contract.Services.Interface
{
    public interface IAppointmentTemplateService
    {
        Task<ApiResult<object>> CreateAppointmentTemplate(CreateATRequest request);
        Task<ApiResult<object>> UpdateAppointmentTemplate(UpdateATRequest request);
        Task<ApiResult<BasePaginatedList<ATResponseModel>>> GetAppointmentTemplatesPagination(BaseSearchRequest request);
        Task<ApiResult<object>> DeleteAppointmentTemplate(DeleteATRequest request);
        Task<ApiResult<ATResponseModel>> GetAppointmentTemplateById(int id);
        Task<ApiResult<List<ATResponseModel>>> GetAll(bool isAdmin);

        ApiResult<List<ATStatusResponseModel>> GetAppointmentTemplateStatusHandler();
    }
}
