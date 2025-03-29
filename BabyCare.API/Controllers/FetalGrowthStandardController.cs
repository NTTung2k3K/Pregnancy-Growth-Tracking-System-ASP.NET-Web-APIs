using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.FetalGrowthStandardModelView;
using Microsoft.AspNetCore.Mvc;

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
            try
            {
                var result = await _fetalGrowthStandardService.GetAllFetalGrowthStandardsAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FetalGrowthStandardModelView>> GetFetalGrowthStandardById(int id)
        {
            try
            {
                var result = await _fetalGrowthStandardService.GetFetalGrowthStandardByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpGet("get-by-week")]
        public async Task<ActionResult<FetalGrowthStandardModelView>> GetFetalGrowthStandardByWeekAsync([FromQuery]int week,[FromQuery] int gender)
        {
            try
            {
                var result = await _fetalGrowthStandardService.GetFetalGrowthStandardByWeekAsync(week,gender);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpPost("create")]
        public async Task<ActionResult<object>> CreateFetalGrowthStandard([FromBody] CreateFetalGrowthStandardModelView model)
        {
            try
            {
                var result = await _fetalGrowthStandardService.AddFetalGrowthStandardAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<object>> UpdateFetalGrowthStandard(int id, [FromBody] UpdateFetalGrowthStandardModelView model)
        {
            try
            {
                var result = await _fetalGrowthStandardService.UpdateFetalGrowthStandardAsync(id, model);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<object>> DeleteFetalGrowthStandard(int id)
        {
            try
            {
                var result = await _fetalGrowthStandardService.DeleteFetalGrowthStandardAsync(id);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
    }

}
