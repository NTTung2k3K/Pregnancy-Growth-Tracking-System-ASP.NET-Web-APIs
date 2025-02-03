using Azure.Core;
using BabyCare.Contract.Services.Implements;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.GrowthChartModelView;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrowthChartController : ControllerBase
    {
        private readonly IGrowthChartService _growthChartService;

        public GrowthChartController(IGrowthChartService growthChartService)
        {
            _growthChartService = growthChartService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<BasePaginatedList<GrowthChartModelView>>> GetAllGrowthCharts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _growthChartService.GetAllGrowthChartsAsync(pageNumber, pageSize);
            return Ok(result);
        }
        [HttpGet("get-status-handler")]
        public IActionResult GetAllGrowthCharts([FromQuery] bool isAdminUpdate)
        {
            try
            {
                var result = _growthChartService.GetGrowthChartsStatusHandler(isAdminUpdate);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<UserResponseModel>>(ex.Message));
            }
        }

        [HttpGet("get-all-growth-chart-by-admin")]
        public async Task<IActionResult> GetAllGrowthChartsAdminAsync()
        {
            try
            {
                var result = await _growthChartService.GetAllGrowthChartsAdminAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<UserResponseModel>>(ex.Message));
            }
        }
        [HttpGet("get-growth-chart-pagination")]
        public async Task<IActionResult> GetGrowthChartPagination([FromQuery]SearchGrowthChartRequest request)
        {
            try
            {
                var result = await _growthChartService.GetGrowthChartPagination(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<UserResponseModel>>(ex.Message));
            }
        }
        [HttpPut("update-growth-chart-by-admin")]
        public async Task<IActionResult> UpdateGrowthChartStatusByAdminAsync([FromBody]UpdateGrowChartByAdmin request)
        {
            try
            {
                var result = await _growthChartService.UpdateGrowthChartStatusByAdminAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<UserResponseModel>>(ex.Message));
            }
        }
        [HttpPut("update-growth-chart-by-user")]
        public async Task<IActionResult> UpdateGrowthChartStatusByUserAsync([FromBody]UpdateGrowChartByUser request)
        {
            try
            {
                var result = await _growthChartService.UpdateGrowthChartStatusByUserAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<UserResponseModel>>(ex.Message));
            }
        }


        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetGrowthChartById([FromQuery]int id)
        {
            try
            {
                var result = await _growthChartService.GetGrowthChartByIdAsync(id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<UserResponseModel>>(ex.Message));
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<object>> CreateGrowthChart([FromBody] CreateGrowthChartModelView model)
        {
            var result = await _growthChartService.AddGrowthChartAsync(model);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<object>> UpdateGrowthChart(int id, [FromBody] UpdateGrowthChartModelView model)
        {
            var result = await _growthChartService.UpdateGrowthChartAsync(id, model);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<object>> DeleteGrowthChart(int id)
        {
            var result = await _growthChartService.DeleteGrowthChartAsync(id);
            return Ok(result);
        }



    }
}
