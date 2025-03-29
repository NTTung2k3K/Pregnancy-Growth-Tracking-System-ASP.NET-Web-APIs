using BabyCare.Contract.Services.Interface;
using BabyCare.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabyCare.API.Controllers
{
    [Route("api/ai-child")]
    [ApiController]
    public class AIChildController : ControllerBase
    {
        private readonly IAIChildService _aiChildService;

        public AIChildController(IAIChildService aiChildService)
        {
            _aiChildService = aiChildService;
        }

        /// <summary>
        /// Nhận câu hỏi từ người dùng và trả lời bằng AI dựa trên dữ liệu của một Child cụ thể
        /// </summary>
        [HttpPost("get-answer-child")]
        public async Task<IActionResult> GetAIResponseAsync([FromBody] AIChildQuestion aIChildQuestion)
        {
            try
            {
                var result = await _aiChildService.GetAIResponseAsync(aIChildQuestion.question, aIChildQuestion.userId, aIChildQuestion.childId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<object>(ex.Message));
            }
        }
    }
    public class AIChildQuestion
    {
         public string question { get; set; }
        public Guid userId { get; set; }
        public int childId { get; set; }
    }
}
