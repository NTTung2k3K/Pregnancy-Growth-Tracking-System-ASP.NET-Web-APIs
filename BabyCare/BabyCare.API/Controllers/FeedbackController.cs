using Azure.Core;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.FeedbackModelView;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpGet("get-all-admin")]
        public async Task<IActionResult> GetAllFeedbackAdminAsync()
        {
            try
            {
                var result = await _feedbackService.GetAllFeedbackAdminAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<BasePaginatedList<FeedbackModelView>>> GetAllFeedbacks([FromQuery] int? growthChartsID, [FromQuery] string? status = null, int pageNumber = 1, int pageSize = 5)
        {
            var result = await _feedbackService.GetAllFeedbackAsync(pageNumber, pageSize, growthChartsID, status);
            return Ok(result);
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetFeedbackById([FromQuery] int id)
        {
            try
            {
                var result = await _feedbackService.GetFeedbackByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpGet("get-feedback-pagination")]
        public async Task<IActionResult> GetFeedbacksWithPagination([FromQuery] int growthChartId, [FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {
            try
            {
                var result = await _feedbackService.GetFeedbacksWithPagination(growthChartId,pageIndex,pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
        [HttpGet("get-feedback-pagination-admin")]
        public async Task<IActionResult> GetFeedbacksWithPaginationAdmin([FromQuery] int growthChartId, [FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {
            try
            {
                var result = await _feedbackService.GetFeedbacksWithPaginationAdmin(growthChartId, pageIndex, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<object>> CreateFeedback([FromBody] CreateFeedbackModelView model)
        {
            try
            {
                var result = await _feedbackService.AddFeedbackAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<object>> UpdateFeedback(int id, [FromBody] UpdateFeedbackModelView model)
        {
            var result = await _feedbackService.UpdateFeedbackAsync(id, model);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<ActionResult<object>> DeleteFeedback([FromQuery]int id)
        {
            try
            {
            var result = await _feedbackService.DeleteFeedbackAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }

        }

        [HttpPut("ban-feedback")]
        public async Task<IActionResult> BlockFeedbackAsync([FromBody] BanFeedbackRequest request)
        {
            try
            {
                var result = await _feedbackService.BlockFeedbackAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }

        }
        [HttpPut("unban-feedback")]
        public async Task<IActionResult> UnBlockFeedbackAsync([FromBody] BanFeedbackRequest request)
        {
            try
            {
                var result = await _feedbackService.UnBlockFeedbackAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }

        }
    }
}
