using BabyCare.Contract.Services.Interface;
using BabyCare.Core.Base;
using BabyCare.Core;
using BabyCare.ModelViews.MembershipPackageModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipPackagesController : ControllerBase
    {
        private readonly IMembershipPackageService _membershipPackageService;


        public MembershipPackagesController(IMembershipPackageService membershipPackageService)
        {
            _membershipPackageService = membershipPackageService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMembershipPackage([FromForm] CreateMPRequest request)
        {
            try
            {
                var result = await _membershipPackageService.CreateMembershipPackage(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpGet("callbackvnpay")]
        public async Task<IActionResult> HandleIpnActionVNpayBackEnd()
        {
            try
            {
                //var resultDescription = $"{paymentResult.PaymentResponse.Description}. {paymentResult.TransactionStatus.Description}.";


                var result = await _membershipPackageService.HandleIpnActionVNpayBackEnd(Request.Query);
                //return Ok(result);
                return Redirect(result.ResultObj);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                //var resultDescription = $"{paymentResult.PaymentResponse.Description}. {paymentResult.TransactionStatus.Description}.";


                var result = await _membershipPackageService.GetAll();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        public static string GetIpAddress(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress?.ToString();
            }
            return ip ?? "0.0.0.0"; // Tránh null
        }

        [HttpPost("buy-package")]
        public async Task<IActionResult> BuyPackage([FromBody] BuyPackageRequest request)
        {
            try
            {
                var ipAddress = MembershipPackagesController.GetIpAddress(HttpContext);
                var result = await _membershipPackageService.BuyPackage(request, ipAddress);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateMembershipPackage([FromForm] UpdateMPRequest request)
        {
            try
            {
                var result = await _membershipPackageService.UpdateMembershipPackage(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteMembershipPackage([FromQuery] DeleteMPRequest request)
        {
            try
            {
                var result = await _membershipPackageService.DeleteMembershipPackage(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpGet("get-pagination")]
        public async Task<IActionResult> GetMembershipPackagePagination([FromQuery] BaseSearchRequest request)
        {
            try
            {
                var result = await _membershipPackageService.GetMembershipPackagePagination(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-membership-package-status-handler")]
        public IActionResult GetMembershipPackageStatusHandler()
        {
            try
            {
                var result = _membershipPackageService.GetMembershipPackageStatusHandler();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetMembershipPackageById([FromQuery] int id)
        {
            try
            {
                var result = await _membershipPackageService.GetMembershipPackageById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
            }
        }



        [HttpGet("can-added-record")]
        public async Task<IActionResult> CanAddedRecord([FromQuery] Guid id)
        {
            try
            {
                var result = await _membershipPackageService.CanAddedRecord(id);
                return Ok(new BabyCare.Core.APIResponse.ApiSuccessResult<bool>(result));
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }



        [HttpGet("can-generate-appointment")]
        public async Task<IActionResult> CanGenerateAppointments([FromQuery] Guid id)
        {
            try
            {
                var result = await _membershipPackageService.CanGenerateAppointments(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }


        [HttpGet("can-share-growth-chart")]
        public async Task<IActionResult> CanShareGrowthChart([FromQuery] Guid id)
        {
            try
            {
                var result = await _membershipPackageService.CanShareGrowthChart(id);
                return Ok(new BabyCare.Core.APIResponse.ApiSuccessResult<bool>(result));
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }


        [HttpGet("can-view-growth-chart")]
        public async Task<IActionResult> CanViewGrowthChart([FromQuery] Guid id)
        {
            try
            {
                var result = await _membershipPackageService.CanViewGrowthChart(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
    }
}

