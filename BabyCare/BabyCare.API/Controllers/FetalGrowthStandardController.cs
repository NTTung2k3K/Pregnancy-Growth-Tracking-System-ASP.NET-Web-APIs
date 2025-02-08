using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.FetalGrowthStandardModelView;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FetalGrowthStandardController : ControllerBase
    {
        private readonly IFetalGrowthStandardService _fetalGrowthStandardService;

        public FetalGrowthStandardController(IFetalGrowthStandardService fetalGrowthStandardService)
        {
            _fetalGrowthStandardService = fetalGrowthStandardService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<BasePaginatedList<FetalGrowthStandardModelView>>> GetAllFetalGrowthStandards([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _fetalGrowthStandardService.GetAllFetalGrowthStandardsAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FetalGrowthStandardModelView>> GetFetalGrowthStandardById(int id)
        {
            var result = await _fetalGrowthStandardService.GetFetalGrowthStandardByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<object>> CreateFetalGrowthStandard([FromBody] CreateFetalGrowthStandardModelView model)
        {
            var result = await _fetalGrowthStandardService.AddFetalGrowthStandardAsync(model);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<object>> UpdateFetalGrowthStandard(int id, [FromBody] UpdateFetalGrowthStandardModelView model)
        {
            var result = await _fetalGrowthStandardService.UpdateFetalGrowthStandardAsync(id, model);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<object>> DeleteFetalGrowthStandard(int id)
        {
            var result = await _fetalGrowthStandardService.DeleteFetalGrowthStandardAsync(id);
            return Ok(result);
        }
    }
}
