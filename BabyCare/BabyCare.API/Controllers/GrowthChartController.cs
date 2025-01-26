using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.GrowthChartModelView;
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

        [HttpGet("{id}")]
        public async Task<ActionResult<GrowthChartModelView>> GetGrowthChartById(int id)
        {
            var result = await _growthChartService.GetGrowthChartByIdAsync(id);
            return Ok(result);
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
