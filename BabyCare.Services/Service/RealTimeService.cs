using PusherServer;
using Microsoft.AspNetCore.Identity;
using BabyCare.Contract.Services.Interface;
using System.Threading.Tasks;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Utils;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
using BabyCare.Contract.Repositories.Interface;

public class RealTimeService : IRealTimeService
{
    private readonly Pusher _pusher;
    private readonly UserManager<ApplicationUsers> _userManager;
    private readonly RoleManager<ApplicationRoles> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public RealTimeService(UserManager<ApplicationUsers> userManager, RoleManager<ApplicationRoles> roleManager, IUnitOfWork unitOfWork)
    {
        _pusher = new Pusher("1964573", "01567a69c62f53eeceb1", "2a5e8270339a5c65862a", new PusherOptions
        {
            Cluster = "ap1",
            Encrypted = true
        });
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    public async Task SendMessage(string channel, string message, Guid senderId, Guid receiverId)
    {
        try
        {
            // Gửi tin nhắn qua Pusher
            var result = await _pusher.TriggerAsync(channel, "new-message", new
            {
                messageContent = message, // ✅ Đổi từ text → messageContent
                senderId = senderId.ToString(),
                receiverId = receiverId.ToString()
            });


            // Không cần kiểm tra kết quả vì Pusher TriggerAsync sẽ ném ra ngoại lệ nếu có lỗi
        }
        catch (Exception ex)
        {
            // Nếu có lỗi xảy ra trong quá trình gửi tin nhắn, ném lỗi hoặc xử lý theo cách bạn muốn
            throw new Exception("Failed to send message via Pusher.", ex);
        }
    }




    /// Kiểm tra userId và role của người dùng
    public async Task<ApiResult<string>> CheckUserRole(Guid userId)
    {
        // Lấy danh sách Role có tên DOCTOR hoặc ADMIN
        var doctorAdminRoles = await _roleManager.Roles
            .Where(r => r.Name == SystemConstant.Role.DOCTOR || r.Name == SystemConstant.Role.ADMIN)
            .ToListAsync();

        if (doctorAdminRoles == null || !doctorAdminRoles.Any())
        {
            return new ApiErrorResult<string>("No roles found.");
        }

        // Lấy danh sách RoleId tương ứng
        var roleIds = doctorAdminRoles.Select(r => r.Id).ToList();

        // Lấy danh sách UserId của những người có RoleId nằm trong danh sách roleIds
        var doctorAdminUserIds = await _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
            .Where(ur => roleIds.Contains(ur.RoleId))
            .OrderByDescending(x => x.LastUpdatedTime)
            .Select(ur => ur.UserId)
            .ToListAsync();

        // Kiểm tra xem userId có tồn tại trong danh sách doctorAdminUserIds không
        if (doctorAdminUserIds.Contains(userId))
        {
            // Lọc user theo userId từ bảng Users
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedBy == null);

            if (user == null)
            {
                return new ApiErrorResult<string>("User not found.");
            }

            return new ApiSuccessResult<string>("Valid user with correct role");
        }

        // Nếu không tìm thấy userId hợp lệ
        return new ApiErrorResult<string>("User does not have Admin or Doctor role.");
    }


}
