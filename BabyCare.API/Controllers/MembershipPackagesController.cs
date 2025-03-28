using BabyCare.Contract.Services.Interface;
using BabyCare.Core.Base;
using BabyCare.Core;
using BabyCare.ModelViews.MembershipPackageModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipPackagesController : ControllerBase
    {
        private readonly IMembershipPackageService _membershipPackageService;
        private readonly IConnectionMultiplexer _redis;

        public MembershipPackagesController(IMembershipPackageService membershipPackageService, IConnectionMultiplexer redis)
        {
            _membershipPackageService = membershipPackageService;
            _redis = redis;
        }

        private async Task ClearCache()
        {
            var cache = _redis.GetDatabase();
            await cache.KeyDeleteAsync("membership_packages");
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMembershipPackage([FromForm] CreateMPRequest request)
        {
            try
            {
                var result = await _membershipPackageService.CreateMembershipPackage(request);
                await ClearCache(); // Xóa cache khi tạo mới
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
                await ClearCache(); // Xóa cache khi cập nhật
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
                await ClearCache(); // Xóa cache khi xóa
                return Ok(result);
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
                var cache = _redis.GetDatabase();
                string cacheKey = "membership_packages";

                var cachedData = await cache.StringGetAsync(cacheKey);
                if (!cachedData.IsNullOrEmpty)
                {
                    return Ok(System.Text.Json.JsonSerializer.Deserialize<object>(cachedData!));
                }

                var result = await _membershipPackageService.GetAll();

                await cache.StringSetAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(result), TimeSpan.FromMinutes(BabyCare.Core.Utils.TimeHelper.CACHE_TIME_MINUTES));

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
                var cache = _redis.GetDatabase();
                string cacheKey = $"membership_packages_{request.PageIndex}_{request.PageSize}";

                var cachedData = await cache.StringGetAsync(cacheKey);
                if (!cachedData.IsNullOrEmpty)
                {
                    return Ok(System.Text.Json.JsonSerializer.Deserialize<object>(cachedData!));
                }

                var result = await _membershipPackageService.GetMembershipPackagePagination(request);

                await cache.StringSetAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(result), TimeSpan.FromMinutes(BabyCare.Core.Utils.TimeHelper.CACHE_TIME_MINUTES));

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
                var cache = _redis.GetDatabase();
                string cacheKey = $"membership_package_{id}";

                var cachedData = await cache.StringGetAsync(cacheKey);
                if (!cachedData.IsNullOrEmpty)
                {
                    return Ok(System.Text.Json.JsonSerializer.Deserialize<object>(cachedData!));
                }

                var result = await _membershipPackageService.GetMembershipPackageById(id);

                await cache.StringSetAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(result), TimeSpan.FromMinutes(BabyCare.Core.Utils.TimeHelper.CACHE_TIME_MINUTES)    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
    }
}
