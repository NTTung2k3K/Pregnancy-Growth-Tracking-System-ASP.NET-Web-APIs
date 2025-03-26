using PusherServer;
using Microsoft.AspNetCore.Identity;
using BabyCare.Contract.Services.Interface;
using System.Threading.Tasks;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Utils;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;

public class RealTimeService : IRealTimeService
{
    private readonly Pusher _pusher;
    private readonly UserManager<ApplicationUsers> _userManager;
    private readonly RoleManager<ApplicationRoles> _roleManager;

    public RealTimeService(UserManager<ApplicationUsers> userManager, RoleManager<ApplicationRoles> roleManager)
    {
        _pusher = new Pusher("1964573", "01567a69c62f53eeceb1", "2a5e8270339a5c65862a", new PusherOptions
        {
            Cluster = "ap1",
            Encrypted = true
        });
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SendMessage(string channel, string message)
    {
        var result = await _pusher.TriggerAsync(channel, "new-message", new { text = message });
    }


    /// Kiểm tra userId và role của người dùng
    public async Task<ApiResult<string>> CheckUserRole(string userName)
    {
        // Truy vấn người dùng từ userName và đảm bảo rằng người dùng không bị xóa
        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.UserName == userName && x.DeletedBy == null);

        // Kiểm tra xem người dùng có tồn tại hay không
        if (user == null)
        {
            return new ApiErrorResult<string>("User not found.");
        }

        // Lấy tất cả các role của người dùng
        var roles = await _userManager.GetRolesAsync(user);

        // Kiểm tra nếu người dùng có role Admin hoặc Doctor
        if (roles.Contains(SystemConstant.Role.ADMIN) || roles.Contains(SystemConstant.Role.DOCTOR))
        {
            return new ApiSuccessResult<string>("Valid user with correct role");
        }

        // Nếu không có role hợp lệ
        return new ApiErrorResult<string>("User does not have Admin or Doctor role.");
    }
}
