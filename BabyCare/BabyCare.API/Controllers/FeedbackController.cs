using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.ModelViews.BlogTypeModelView;
using BabyCare.ModelViews.FeedbackModelView;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

        [HttpGet("all")]
        public async Task<ActionResult<BasePaginatedList<FeedbackModelView>>> GetAllFeedbacks([FromQuery] int? growthChartsID, [FromQuery] string? status=null, int pageNumber = 1, int pageSize = 5)
        {
            var result = await _feedbackService.GetAllFeedbackAsync(pageNumber, pageSize, growthChartsID, status);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FeedbackModelView>> GetFeedbackById(int id)
        {
            var result = await _feedbackService.GetFeedbackByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult<object>> CreateFeedback([FromBody] CreateFeedbackModelView model)
        {
            var result = await _feedbackService.AddFeedbackAsync(model);
            return Ok(result);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<object>> UpdateFeedback(int id, [FromBody] UpdateFeedbackModelView model)
        {
            var result = await _feedbackService.UpdateFeedbackAsync(id, model);
            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<object>> DeleteFeedback(int id)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(id);
            return Ok(result);
        }
    }
}
