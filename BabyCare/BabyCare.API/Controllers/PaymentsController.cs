using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.UserModelViews.Response;
using BabyCare.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _paymentService.GetAll();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            try
            {
                var result = await _paymentService.GetById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }

        [HttpGet("get-monthly-payment-statistics")]
        public IActionResult GetMonthlyPaymentStatistics()
        {
            try
            {
                var result = _paymentService.GetMonthlyPaymentStatistics();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }

        [HttpGet("get-recent-transactions")]
        public async Task<IActionResult> GetRecentTransactions([FromQuery] int quantity)
        {
            try
            {
                var result = await _paymentService.GetRecentTransactions(quantity);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }

        [HttpGet("get-total-revenue-for-current-year")]
        public IActionResult GetTotalRevenueForCurrentYear()
        {
            try
            {
                var result = _paymentService.GetTotalRevenueForCurrentYear();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }

        [HttpGet("get-month-with-max-transactions")]
        public IActionResult GetMonthWithMaxTransactions()
        {
            try
            {
                var result = _paymentService.GetMonthWithMaxTransactions();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
    }
}
