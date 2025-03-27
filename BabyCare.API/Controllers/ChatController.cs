using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.UserModelViews.Response;
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

        // Tạo tên kênh duy nhất cho cặp người gửi và người nhận, đảm bảo thứ tự không thay đổi
        var userIdStr = request.UserId.ToString("D");  // Chuyển Guid thành chuỗi
        var recipientUserIdStr = request.RecipientUserId.ToString("D");  // Chuyển Guid thành chuỗi

        // Tạo tên kênh dựa trên thứ tự của các Guid để luôn ổn định
        var channelName = $"chat-{(string.Compare(userIdStr, recipientUserIdStr) < 0 ? userIdStr : recipientUserIdStr)}-{(string.Compare(userIdStr, recipientUserIdStr) < 0 ? recipientUserIdStr : userIdStr)}";

        // Gửi tin nhắn qua Pusher tới kênh duy nhất
        await _realTimeService.SendMessage(channelName, request.Message, request.UserId, request.RecipientUserId);
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        // Trả về channelName, UserId, RecipientUserId và nội dung tin nhắn cho frontend
        return Ok(new
        {
            message = "Message sent to " + request.RecipientUserId,
            channelName,
            userId = request.UserId,
            recipientUserId = request.RecipientUserId,
            messageContent = request.Message,
            sendAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
        });
    }
    [HttpGet("get-message")]
    public async Task<IActionResult> GetMessageHistory([FromQuery] Guid senderId,[FromQuery] Guid receiverId)
    {
        try
        {
            var result = await _realTimeService.GetMessageHistory(senderId, receiverId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new BabyCare.Core.APIResponse.ApiErrorResult<BasePaginatedList<EmployeeResponseModel>>(ex.Message));
        }
    }

}

public class SendMessageRequest
{
    public string Message { get; set; }
    public Guid UserId { get; set; }
    public Guid RecipientUserId { get; set; }
    
}
