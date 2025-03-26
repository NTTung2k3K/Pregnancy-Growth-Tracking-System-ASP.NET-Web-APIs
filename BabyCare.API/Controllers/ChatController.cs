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
        // Kiểm tra nếu UserName và RecipientUserName giống nhau (không cho phép gửi tin nhắn cho chính mình)
        if (request.UserName == request.RecipientUserName)
        {
            return BadRequest(new { message = "You cannot send messages to yourself." });
        }

        // Kiểm tra role của người gửi (UserName)
        var senderRoleCheckResult = await _realTimeService.CheckUserRole(request.UserName);

        if (senderRoleCheckResult is ApiErrorResult<string>)
        {
            return BadRequest(new { message = "Sender is not an Admin or Doctor, cannot send messages." });
        }

        // Kiểm tra role của người nhận (RecipientUserName)
        var recipientRoleCheckResult = await _realTimeService.CheckUserRole(request.RecipientUserName);

        if (recipientRoleCheckResult is ApiErrorResult<string>)
        {
            return BadRequest(new { message = "Recipient is not an Admin or Doctor, cannot receive messages." });
        }

        // Nếu cả người gửi và người nhận đều hợp lệ, tiếp tục gửi tin nhắn
        if (request.UserName != null && request.RecipientUserName != null)
        {
            await _realTimeService.SendMessage(request.RecipientUserName, request.Message);

            return Ok(new { message = "Message sent to " + request.RecipientUserName });
        }

        return BadRequest(new { message = "Invalid request. Usernames are required." });
    }
}

public class SendMessageRequest
{
    public string Message { get; set; }
    public string UserName { get; set; } // Người gửi tin nhắn
    public string RecipientUserName { get; set; } // Người nhận tin nhắn
}
