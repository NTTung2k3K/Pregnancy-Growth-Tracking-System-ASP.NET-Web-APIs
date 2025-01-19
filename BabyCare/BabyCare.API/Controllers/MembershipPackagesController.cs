using BabyCare.Contract.Services.Implements;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core.Base;
using BabyCare.Core;
using BabyCare.ModelViews.MembershipPackageModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VNPAY.NET.Utilities;
using Azure.Core;

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
        public async Task<IActionResult> IpnActionAsync()
        {
            try
            {
                var result = await _membershipPackageService.HandleIpnActionVNpay(Request.Query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpPost("buy-package")]
        public async Task<IActionResult> BuyPackage([FromBody] BuyPackageRequest request)
        {
            try
            {
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext); 
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
        public  IActionResult GetMembershipPackageStatusHandler()
        {
            try
            {
                var result =  _membershipPackageService.GetMembershipPackageStatusHandler();
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
    }
}
