using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.FetalGrowthRecordModelView;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FetalGrowthRecordController : ControllerBase
    {
        private readonly IFetalGrowthRecordService _fetalGrowthRecordService;

        public FetalGrowthRecordController(IFetalGrowthRecordService fetalGrowthRecordService)
        {
            _fetalGrowthRecordService = fetalGrowthRecordService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<BasePaginatedList<FetalGrowthRecordModelView>>> GetAllFetalGrowthRecords([FromQuery] int? childId, [FromQuery] int? weekOfPregnancy, int pageNumber = 1, int pageSize = 5)
        {
            var result = await _fetalGrowthRecordService.GetAllFetalGrowthRecordsAsync(pageNumber, pageSize, childId, weekOfPregnancy);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FetalGrowthRecordModelView>> GetFetalGrowthRecordById(int id)
        {
            var result = await _fetalGrowthRecordService.GetFetalGrowthRecordByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<object>> CreateFetalGrowthRecord([FromBody] CreateFetalGrowthRecordModelView model)
        {
            var result = await _fetalGrowthRecordService.AddFetalGrowthRecordAsync(model);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<object>> UpdateFetalGrowthRecord(int id, [FromBody] UpdateFetalGrowthRecordModelView model)
        {
            var result = await _fetalGrowthRecordService.UpdateFetalGrowthRecordAsync(id, model);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<object>> DeleteFetalGrowthRecord(int id)
        {
            var result = await _fetalGrowthRecordService.DeleteFetalGrowthRecordAsync(id);
            return Ok(result);
        }
    }
}
