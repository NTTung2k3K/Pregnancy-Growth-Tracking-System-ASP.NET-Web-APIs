﻿using BabyCare.Contract.Services.Interface;
using BabyCare.Core.Base;
using BabyCare.Core;
using BabyCare.ModelViews.AppointmentModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            try
            {
                var result = await _appointmentService.CreateAppointment(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmAppointment([FromBody] ConfirmAppointment request)
        {
            try
            {
                var result = await _appointmentService.ConfirmAppointment(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAppointment([FromBody] UpdateAppointmentRequest request)
        {
            try
            {
                var result = await _appointmentService.UpdateAppointment(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpPut("update-by-doctor")]
        public async Task<IActionResult> UpdateByDoctorAppointment([FromBody] UpdateAppointmentByDoctorRequest request)
        {
            try
            {
                var result = await _appointmentService.UpdateByDoctorAppointment(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpPatch("cancel-appointment-by-user")]
        public async Task<IActionResult> UpdateAppointment([FromBody] CancelAppointmentByUser request)
        {
            try
            {
                var result = await _appointmentService.UpdateCancelAppointmentStatusByUser(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpPatch("no-show-appointment-by-doctor")]
        public async Task<IActionResult> UpdateAppointment([FromBody] NoShowAppointmentByDoctor request)
        {
            try
            {
                var result = await _appointmentService.UpdateNoShowAppointmentStatusByDoctor(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAppointment([FromQuery] DeleteAppointmentRequest request)
        {
            try
            {
                var result = await _appointmentService.DeleteAppointment(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpGet("get-pagination")]
        public async Task<IActionResult> GetAppointmentPagination([FromQuery] BaseSearchRequest request)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentsPagination(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-pagination-by-user-id")]
        public async Task<IActionResult> GetAppointmentsByUserIdPagination([FromQuery] SearchAppointmentByUserId request)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentsByUserIdPagination(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-available-slot")]
        public async Task<IActionResult> GetSlotAvailable([FromQuery] DateTime date)
        {
            try
            {
                var result = await _appointmentService.GetSlotAvailable(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-available-slot-user")]
        public async Task<IActionResult> GetAvailableSlotsUserAsync([FromQuery] DateTime date)
        {
            try
            {
                var result = await _appointmentService.GetAvailableSlotsUserAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-appointment-status-handler")]
        public  IActionResult GetAppointmentStatusHandler()
        {
            try
            {
                var result = _appointmentService.GetAppointmentStatusHandler();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetAppointmentById([FromQuery] int id)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-by-id-side-admin")]
        public async Task<IActionResult> GetAppointmentByIdSideAdmin([FromQuery] int id)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentByIdSideAdmin(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-all-by-user-id")]
        public async Task<IActionResult> GetAppointmentsByUserId([FromQuery] Guid userId)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentsByUserId(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-in-range-by-user-id")]
        public async Task<IActionResult> GetAppointmentsByUserId([FromQuery] Guid userId, [FromQuery] DateTime startDay, [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentsByUserIdInRange(userId,startDay,endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-doctor-in-range-by-user-id")]
        public async Task<IActionResult> GetAppointmentsDoctorByUserIdInRange([FromQuery] Guid userId, [FromQuery] DateTime startDay, [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _appointmentService.GetAppointmentsDoctorByUserIdInRange(userId, startDay, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] Guid doctorId)
        {
            try
            {
                var result = await _appointmentService.GetAll(doctorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-all-by-admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _appointmentService.GetAllByAdmin();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }

        [HttpPost("change-doctor-appointment")]
        public async Task<IActionResult> ChangeDoctorAppointment(ChangeDoctorAppointmentRequest request)
        {
            try
            {
                var result = await _appointmentService.ChangeDoctorAppointment(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        [HttpGet("get-doctor-free")]
        public async Task<IActionResult> GetAllDoctorFree([FromQuery] int appointmentId)
        {
            try
            {
                var result = await _appointmentService.GetAllDoctorFree(appointmentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }

    }
}
