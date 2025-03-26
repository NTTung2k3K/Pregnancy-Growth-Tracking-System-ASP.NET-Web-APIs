using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core.APIResponse;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IRealTimeService _realTimeService;

    public ChatController(IRealTimeService realTimeService)
    {
        _realTimeService = realTimeService;
    }

    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        // Kiểm tra nếu UserId của người gửi và người nhận giống nhau (không cho phép gửi tin nhắn cho chính mình)
        if (request.UserId == request.RecipientUserId)
        {
            return BadRequest(new { message = "You cannot send messages to yourself." });
        }

        // Kiểm tra role của người gửi (UserId)
        var senderRoleCheckResult = await _realTimeService.CheckUserRole(request.UserId);

        if (senderRoleCheckResult is ApiErrorResult<string>)
        {
            return BadRequest(new { message = "Sender is not an Admin or Doctor, cannot send messages." });
        }

        // Kiểm tra role của người nhận (RecipientUserId)
        var recipientRoleCheckResult = await _realTimeService.CheckUserRole(request.RecipientUserId);

        if (recipientRoleCheckResult is ApiErrorResult<string>)
        {
            return BadRequest(new { message = "Recipient is not an Admin or Doctor, cannot receive messages." });
        }

        // Nếu cả người gửi và người nhận đều hợp lệ, tiếp tục gửi tin nhắn
        if (request.UserId != null && request.RecipientUserId != null)
        {
            await _realTimeService.SendMessage(request.RecipientUserId.ToString(), request.Message);

            return Ok(new { message = "Message sent to " + request.RecipientUserId });
        }

        return BadRequest(new { message = "Invalid request. UserIds are required." });
    }
}

public class SendMessageRequest
{
    public string Message { get; set; }
    public Guid UserId { get; set; } 
    public Guid RecipientUserId { get; set; }
}
