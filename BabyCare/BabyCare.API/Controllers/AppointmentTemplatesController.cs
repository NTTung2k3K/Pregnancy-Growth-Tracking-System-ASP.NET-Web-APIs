using BabyCare.Contract.Services.Implements;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core.Base;
using BabyCare.Core;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VNPAY.NET.Utilities;
using Azure.Core;
using BabyCare.ModelViews.AppointmentTemplateModelViews.Response;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentTemplatesController : ControllerBase
    {
        private readonly IAppointmentTemplateService _appointmentTemplateService;
        public AppointmentTemplatesController(IAppointmentTemplateService appointmentTemplateService)
        {
            _appointmentTemplateService = appointmentTemplateService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointmentTemplate([FromForm] CreateATRequest request)
        {
            try
            {
                var result = await _appointmentTemplateService.CreateAppointmentTemplate(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
    
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAppointmentTemplate([FromForm] UpdateATRequest request)
        {
            try
            {
                var result = await _appointmentTemplateService.UpdateAppointmentTemplate(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAppointmentTemplate([FromQuery] DeleteATRequest request)
        {
            try
            {
                var result = await _appointmentTemplateService.DeleteAppointmentTemplate(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpGet("get-pagination")]
        public async Task<IActionResult> GetAppointmentTemplatePagination([FromQuery] BaseSearchRequest request)
        {
            try
            {
                var result = await _appointmentTemplateService.GetAppointmentTemplatesPagination(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] bool isAdmin)
        {
            try
            {
                var result = await _appointmentTemplateService.GetAll(isAdmin);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-membership-package-status-handler")]
        public  IActionResult GetAppointmentTemplateStatusHandler()
        {
            try
            {
                var result = _appointmentTemplateService.GetAppointmentTemplateStatusHandler();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetAppointmentTemplateById([FromQuery] int id)
        {
            try
            {
                var result = await _appointmentTemplateService.GetAppointmentTemplateById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
    }
}
