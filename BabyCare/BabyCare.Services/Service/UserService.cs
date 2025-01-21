using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.Core.Firebase;
using BabyCare.Core.Utils;
using BabyCare.ModelViews.AuthModelViews.Request;
using BabyCare.ModelViews.AuthModelViews.Response;
using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using BabyCare.ModelViews.UserModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;
using Firebase.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.Contract.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUsers> _userManager;    
        private readonly RoleManager<ApplicationRoles> _roleManager;

        private readonly IHttpContextAccessor _contextAccessor;

        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public UserService(IConfiguration configuration, IHttpContextAccessor contextAccessor, IUnitOfWork unitOfWork, UserManager<ApplicationUsers> userManager, IMapper mapper, RoleManager<ApplicationRoles> roleManager)
        {
            _configuration = configuration;
            _contextAccessor = contextAccessor;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        #region Authen User
        public async Task<ApiResult<UserLoginResponseModel>> UserLogin(UserLoginRequestModel request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser == null)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.", System.Net.HttpStatusCode.NotFound);
            }
            if (existingUser.DeletedBy != null)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.", System.Net.HttpStatusCode.NotFound);
            }
            var validPassword = await _userManager.CheckPasswordAsync(existingUser, request.Password);
            if (!validPassword)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.", System.Net.HttpStatusCode.NotFound);
            }
            var roles = await _userManager.GetRolesAsync(existingUser);
            foreach (var role in roles)
            {
                if (role != SystemConstant.Role.USER)
                {
                    return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.", System.Net.HttpStatusCode.NotFound);
                }
            }
            var isConfirmed = await _userManager.IsEmailConfirmedAsync(existingUser);
            if (!isConfirmed)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.", System.Net.HttpStatusCode.NotFound);

            }

            if (existingUser.Status == ((int)SystemConstant.UserStatus.InActive))
            {
                return new ApiErrorResult<UserLoginResponseModel>("You cannot access system.", System.Net.HttpStatusCode.NotFound);

            }
            var refreshTokenData = GenerateRefreshToken();
            var accessTokenData = await GenerateAccessTokenAsync(existingUser);
            existingUser.RefreshToken = refreshTokenData.Item1;
            existingUser.RefreshTokenExpiryTime = refreshTokenData.Item2;

            await _userManager.UpdateAsync(existingUser);
            var response = _mapper.Map<UserLoginResponseModel>(existingUser);
            response.AccessToken = accessTokenData.Item1;
            response.AccessTokenExpiredTime = accessTokenData.Item2;
            response.RefreshToken = refreshTokenData.Item1;
            response.RefreshTokenExpiryTime = refreshTokenData.Item2;
            return new ApiSuccessResult<UserLoginResponseModel>(response, "Đăng nhập thành công.");

        }
        private (string, DateTime) GenerateRefreshToken()
        {
            var expiredTime = DateTime.Now.AddMinutes(BabyCare.Core.Utils.TimeHelper.DURATION_REFRESH_TOKEN_TIME);
            var refreshToken = "";
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);

                refreshToken = Convert.ToBase64String(random);
            }
            return (refreshToken, expiredTime);
        }
        private async Task<(string, DateTime)> GenerateAccessTokenAsync(ApplicationUsers user)
        {
            var expiredTime = DateTime.Now.AddMinutes(BabyCare.Core.Utils.TimeHelper.DURATION_ACCESS_TOKEN_TIME);
            var authClaims = new List<Claim>
            {
                new Claim("userId", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: expiredTime,
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512)
                );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (accessToken, expiredTime);
        }

        private async Task<string> _generateUsernameOfGuestAsync()
        {
            Random random = new Random();
            while (true)
            {
                var username = "USER_" + random.Next(0, 999999).ToString("D6");

                var userCheckExisted = await _userManager.FindByNameAsync(username);
                if (userCheckExisted == null)
                {
                    return username;
                }
            }
        }


        public async Task<ApiResult<object>> UserRegister(UserRegisterRequestModel request)
        {
            // Check existed email
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (existingUser != null)
            {
                return new ApiErrorResult<object>("Email is existed.", System.Net.HttpStatusCode.BadRequest);
            }
            var user = new ApplicationUsers
            {
                Email = request.Email,
                UserName = await _generateUsernameOfGuestAsync(),
            };
            // Save user
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<object>("Register unsuccessfully.", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            // Add role
            var rs = await _userManager.AddToRoleAsync(user, SystemConstant.Role.USER);
            if (!rs.Succeeded)
            {
                return new ApiErrorResult<object>("Register unsuccessfully.", rs.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Create OTP
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "Welcome.html");
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                return new ApiErrorResult<object>("Không tìm thấy file gửi mail");
            }
            var content = File.ReadAllText(path);
            content = content.Replace("{{OTP}}", Uri.EscapeDataString(token));
            content = content.Replace("{{Name}}", user.Email);
            var resultSendMail = DoingMail.SendMail("BabyCare", "Confirm Email", content, user.Email);
            if (!resultSendMail)
            {
                return new ApiErrorResult<object>("Cannot send email to " + request.Email);
            }

            return new ApiSuccessResult<object>("Please check your gmail to confirm");
        }

        public async Task<ApiResult<UserLoginResponseModel>> RefreshToken(NewRefreshTokenRequestModel request)
        {

            // Check existed user
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingUser == null)
            {
                return new ApiErrorResult<UserLoginResponseModel>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check refresh token
            if (existingUser.RefreshToken != request.RefreshToken)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Refresh token is not correct.", System.Net.HttpStatusCode.BadRequest);
            }
            // Check expired time
            if (existingUser.RefreshTokenExpiryTime < DateTime.Now)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Refresh token is expired.", System.Net.HttpStatusCode.BadRequest);
            }
            // Generate new refresh token
            var refreshTokenData = GenerateRefreshToken();
            var accessTokenData = await GenerateAccessTokenAsync(existingUser);
            existingUser.RefreshToken = refreshTokenData.Item1;
            existingUser.RefreshTokenExpiryTime = refreshTokenData.Item2;
            await _userManager.UpdateAsync(existingUser);
            // Response to client
            var response = _mapper.Map<UserLoginResponseModel>(existingUser);
            response.AccessToken = accessTokenData.Item1;
            response.AccessTokenExpiredTime = accessTokenData.Item2;
            return new ApiSuccessResult<UserLoginResponseModel>(response, "Refresh token successfully.");
        }

        public async Task<ApiResult<object>> ForgotPassword(ForgotPasswordRequest request)
        {
            // Check existed email
            var email = request.Email;
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("Email is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            var isConfirmed = await _userManager.IsEmailConfirmedAsync(existingUser);
            if (!isConfirmed)
            {
                return new ApiErrorResult<object>("Email is not confirm.", System.Net.HttpStatusCode.NotFound);

            }
            // Generate token
            var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
            // Send email
            // Correct relative path from current directory to the HTML file


            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "SendCodeCustomer.html");
            path = Path.GetFullPath(path);


            if (!System.IO.File.Exists(path))
            {
                return new ApiErrorResult<object>("System error, try later", System.Net.HttpStatusCode.NotFound);
            }
            var frontEndUrl = _configuration["URL:FrontEnd"];
            var fullForgotPasswordUrl = frontEndUrl + "/auth/new-password?email=" + email + "&token=" + token;
            string contentCustomer = System.IO.File.ReadAllText(path);
            contentCustomer = contentCustomer.Replace("{{VerifyCode}}", fullForgotPasswordUrl);
            var sendMailResult = DoingMail.SendMail("BabyCare", "Yêu cầu thay đổi mật khẩu", contentCustomer, email);
            if (!sendMailResult)
            {
                return new ApiErrorResult<object>("Lỗi hệ thống. Vui lòng thử lại sau", System.Net.HttpStatusCode.NotFound);
            }
            return new ApiSuccessResult<object>("Please check your mail to reset password.");
        }

        public async Task<ApiResult<object>> ResetPassword(ResetPasswordRequestModel request)
        {
            // Check existed email
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("Email is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Valid token
            var result = await _userManager.ResetPasswordAsync(existingUser, request.Token, request.Password);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<object>("Reset password unsuccesfully", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            return new ApiSuccessResult<object>("Reset password successfully.");

        }

        public async Task<ApiResult<EmployeeLoginResponseModel>> EmployeeLogin(EmployeeLoginRequestModel request)
        {
            // Check valid username
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser == null)
            {
                return new ApiErrorResult<EmployeeLoginResponseModel>("Username or password is not correct.", System.Net.HttpStatusCode.NotFound);
            }
            if (existingUser.DeletedBy != null)
            {
                return new ApiErrorResult<EmployeeLoginResponseModel>("Username or password is not correct.", System.Net.HttpStatusCode.NotFound);
            }
            // Check valid password
            var validPassword = await _userManager.CheckPasswordAsync(existingUser, request.Password);
            if (!validPassword)
            {
                return new ApiErrorResult<EmployeeLoginResponseModel>("Username or password is not correct.", System.Net.HttpStatusCode.NotFound);
            }
            // Check valid role doctor or admin
            var userRoles = await _userManager.GetRolesAsync(existingUser);
            if (!userRoles.Contains("Doctor") && !userRoles.Contains("Admin"))
            {
                return new ApiErrorResult<EmployeeLoginResponseModel>("Username or password is not correct.", System.Net.HttpStatusCode.NotFound);
            }
            if (existingUser.Status == ((int)SystemConstant.EmployeeStatus.InActive))
            {
                return new ApiErrorResult<EmployeeLoginResponseModel>("You cannot access system.", System.Net.HttpStatusCode.NotFound);

            }
            // Generate refresh token
            var refreshTokenData = GenerateRefreshToken();
            var accessTokenData = await GenerateAccessTokenAsync(existingUser);
            existingUser.RefreshToken = refreshTokenData.Item1;
            existingUser.RefreshTokenExpiryTime = refreshTokenData.Item2;
            await _userManager.UpdateAsync(existingUser);
            // Response to client
            var response = _mapper.Map<EmployeeLoginResponseModel>(existingUser);
            response.AccessToken = accessTokenData.Item1;
            response.AccessTokenExpiredTime = accessTokenData.Item2;
            return new ApiSuccessResult<EmployeeLoginResponseModel>(response, "Login successfully.");
        }

        public async Task<ApiResult<object>> EmployeeForgotPassword(ForgotPasswordRequest request)
        {
            var email = request.Email;
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("Email is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
            // Correct relative path from current directory to the HTML file
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "SendCode.html");
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                return new ApiErrorResult<object>("Không tìm thấy file gửi mail");
            }

            var frontEndUrl = _configuration["URL:FrontEnd"];
            var fullForgotPasswordUrl = frontEndUrl + "/reset-password?email=" + email + "&token=" + token;
            string contentCustomer = System.IO.File.ReadAllText(path);
            contentCustomer = contentCustomer.Replace("{{VerifyCode}}", fullForgotPasswordUrl);
            var sendMailResult = DoingMail.SendMail("BabyCare", "Yêu cầu thay đổi mật khẩu", contentCustomer, email);
            if (!sendMailResult)
            {
                return new ApiErrorResult<object>("Lỗi hệ thống. Vui lòng thử lại sau", System.Net.HttpStatusCode.NotFound);
            }
            return new ApiSuccessResult<object>("Please check your mail to reset password.");
        }

        public async Task<ApiResult<object>> EmployeeResetPassword(ResetPasswordRequestModel request)
        {
            // Check existed email
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("Email is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check valid role Doctor or Admin
            var userRoles = await _userManager.GetRolesAsync(existingUser);
            if (!userRoles.Contains("Doctor") && !userRoles.Contains("Admin"))
            {
                return new ApiErrorResult<object>("Email is not existed.", System.Net.HttpStatusCode.NotFound);
            }

            // Valid token
            var result = await _userManager.ResetPasswordAsync(existingUser, request.Token, request.Password);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<object>("Reset password unsuccesfully", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            return new ApiSuccessResult<object>("Reset password successfully.");
        }

        public async Task<ApiResult<object>> UpdateUserProfile(UpdateUserProfileRequest request)
        {
            // check existed user
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Update user profile by mapper
            _mapper.Map(request, existingUser);
            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<object>("Update profile unsuccesfully", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            return new ApiSuccessResult<object>("Update profile successfully.");
        }


        public async Task<ApiResult<BasePaginatedList<UserResponseModel>>> GetUserPagination(BaseSearchRequest request)
        {
            // all user


            var userRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == SystemConstant.Role.USER);


            // Filter users

            var doctorUserIds = await _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
        .Where(ur => ur.RoleId == userRole.Id)
        .Select(ur => ur.UserId)
        .ToListAsync();

            // Lọc danh sách user theo UserId từ bảng User
            var users = _userManager.Users
                .Where(u => doctorUserIds.Contains(u.Id) && u.DeletedBy == null);
            // filter by search 
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                users = users.Where(x => x.FullName.ToLower().Contains(request.SearchValue.ToLower()) || x.Email.ToLower().Contains(request.SearchValue.ToLower()));
            }
            // paging
            var currentPage = request.PageIndex ?? 1;
            var pageSize = request.PageSize ?? SystemConstant.PAGE_SIZE;
            var total = users.Count();
            var data = await users.Skip((currentPage - 1) * currentPage).Take(pageSize).ToListAsync();
            // calculate total page

            var items = data.Select(x => new UserResponseModel
            {
                Address = x.Address,
                DateOfBirth = x.DateOfBirth,
                FullName = x.FullName,
                Gender = x.Gender,
                Image = x.Image,
                Id = x.Id,
                Status = Enum.IsDefined(typeof(EmployeeStatus), x.Status)
                               ? ((EmployeeStatus)x.Status).ToString()
                                  : "Unknown",
                BloodGroup = x.BloodGroup,
                Email = x.Email,
            }).ToList();
            var response = new BasePaginatedList<UserResponseModel>(items, total, currentPage, pageSize);
            // return to client
            return new ApiSuccessResult<BasePaginatedList<UserResponseModel>>(response);

        }

        public async Task<ApiResult<UserResponseModel>> GetUserById(Guid Id)
        {
            // Check existed user
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == Id);
            if (existingUser == null)
            {
                return new ApiErrorResult<UserResponseModel>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check isUser
            var isValidUser = await _userManager.GetRolesAsync(existingUser);
            foreach (var item in isValidUser)
            {
                if (item != SystemConstant.Role.USER)
                {
                    return new ApiErrorResult<UserResponseModel>("User is not existed.", System.Net.HttpStatusCode.NotFound);
                }
            }
            // Response to client
            var response = _mapper.Map<UserResponseModel>(existingUser);
            if (Enum.IsDefined(typeof(UserStatus), existingUser.Status))
            {
                response.Status = ((UserStatus)existingUser.Status).ToString();
            }
            else
            {
                response.Status = "Unknown";
            }
            return new ApiSuccessResult<UserResponseModel>(response);

        }

        public async Task<ApiResult<object>> DeleteUser(DeleteUserRequest request)
        {
            // Check existed user
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Delete user
            var result = await _userManager.DeleteAsync(existingUser);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<object>("Delete usere unsuccesfully", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            return new ApiSuccessResult<object>("Delete user successfully.");
        }

        public async Task<ApiResult<object>> UpdateUserStatus(UpdateUserStatusRequest request)
        {
            // Check existed user
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (existingUser == null)
            {
                return new ApiErrorResult<object>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check status included on enum
            if (!Enum.IsDefined(typeof(SystemConstant.UserStatus), request.Status))
            {
                return new ApiErrorResult<object>("Status is not correct.", System.Net.HttpStatusCode.BadRequest);
            }

            // Update status
            existingUser.Status = (int)request.Status;
            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<object>("Update status unsuccesfully", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            return new ApiSuccessResult<object>("Update status successfully.");

        }

        public async Task<ApiResult<object>> CreateEmployee(CreateEmployeeRequest request)
        {
            // Check existed username
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName);
            if (existingUser != null)
            {
                return new ApiErrorResult<object>("Username is existed.", System.Net.HttpStatusCode.Conflict);
            }
            // Check existed email
            var existingEmail = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (existingEmail != null)
            {
                return new ApiErrorResult<object>("Email is existed.", System.Net.HttpStatusCode.Conflict);
            }

            // Createe user use mapper
            var user = _mapper.Map<ApplicationUsers>(request);
            user.Status = (int)EmployeeStatus.Active;
            if (request.Image != null)
            {
                user.Image = await BabyCare.Core.Firebase.ImageHelper.Upload(request.Image);
            }
            var result = await _userManager.CreateAsync(user, request.Password);
            // Find role
            var role = await _roleManager.Roles.FirstOrDefaultAsync(x => x.Id == request.RoleId);

            // Add user role


            await _userManager.AddToRoleAsync(user, role.Name);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<object>("Create user unsuccessfully.", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            return new ApiSuccessResult<object>("Create user successfully.");
        }

        public async Task<ApiResult<object>> UpdateEmployeeProfile(UpdateEmployeeProfileRequest request)
        {
            // Check existed user
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Update user profile by mapper
            _mapper.Map(request, existingUser);
            existingUser.LastUpdatedTime = DateTime.Now;
            existingUser.LastUpdatedBy = Guid.Parse(_contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value);

            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<object>("Update profile unsuccesfully", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            return new ApiSuccessResult<object>("Update profile successfully.");
        }

        public async Task<ApiResult<object>> UpdateEmployeeStatus(UpdateUserStatusRequest request)
        {
            // Check existed user
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check status included on enum
            if (!Enum.IsDefined(typeof(SystemConstant.EmployeeStatus), request.Status))
            {
                return new ApiErrorResult<object>("Status is not correct.", System.Net.HttpStatusCode.BadRequest);
            }
            // Update status
            existingUser.Status = (int)request.Status;
            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<object>("Update status unsuccesfully", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            return new ApiSuccessResult<object>("Update status successfully.");

        }

        public async Task<ApiResult<object>> DeleteEmployee(DeleteUserRequest request)
        {
            // Check existed user
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingUser == null)
            {
                return new ApiErrorResult<object>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Delete user
            existingUser.DeletedBy = Guid.Parse(_contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value);
            existingUser.DeletedTime = DateTime.Now;
            var result = await _userManager.UpdateAsync(existingUser);
            // Return to client
            if (!result.Succeeded)
            {
                return new ApiErrorResult<object>("Delete usere unsuccesfully", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            return new ApiSuccessResult<object>("Delete user successfully.");

        }

        public async Task<ApiResult<BasePaginatedList<EmployeeResponseModel>>> GetDoctorPagination(BaseSearchRequest request)
        {
            // all user doctor

            var doctorRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == SystemConstant.Role.DOCTOR);


            // Filter users

            var doctorUserIds = await _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
        .Where(ur => ur.RoleId == doctorRole.Id)
        .Select(ur => ur.UserId)
        .ToListAsync();

            // Lọc danh sách user theo UserId từ bảng User
            var users = _userManager.Users
                .Where(u => doctorUserIds.Contains(u.Id) && u.DeletedBy == null);

            // filter by search 
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                users = users.Where(x => x.FullName.ToLower().Contains(request.SearchValue.ToLower()) || x.Email.ToLower().Contains(request.SearchValue.ToLower()));
            }
            // paging
            var rs = await users.ToListAsync();
            var currentPage = request.PageIndex ?? 1;
            var pageSize = request.PageSize ?? SystemConstant.PAGE_SIZE;
            var total = rs.Count();
            var data = await users.Skip((currentPage - 1) * currentPage).Take(pageSize).ToListAsync();
            // calculate total page

            var items = data.Select(x => new EmployeeResponseModel
            {
                Address = x.Address,
                DateOfBirth = x.DateOfBirth,
                FullName = x.FullName,
                Gender = x.Gender,
                Image = x.Image,
                Id = x.Id,
                Status = Enum.IsDefined(typeof(EmployeeStatus), x.Status)
                               ? ((EmployeeStatus)x.Status).ToString()
                                  : "Unknown",
                Role = new ModelViews.RoleModelViews.RoleModelView()
                {
                    Id = doctorRole.Id.ToString(),
                    Name = doctorRole.Name,
                },

            }).ToList();

            var response = new BasePaginatedList<EmployeeResponseModel>(items, total, currentPage, pageSize);
            // return to client
            return new ApiSuccessResult<BasePaginatedList<EmployeeResponseModel>>(response);
        }

        public async Task<ApiResult<EmployeeResponseModel>> GetEmployeeById(Guid Id)
        {
            // Check existed user
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == Id);
            if (existingUser == null)
            {
                return new ApiErrorResult<EmployeeResponseModel>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            if (existingUser.Status == (int)SystemConstant.EmployeeStatus.InActive || existingUser.DeletedBy != null)
            {
                return new ApiErrorResult<EmployeeResponseModel>("User is not existed.", System.Net.HttpStatusCode.NotFound);

            }
            // Check role Admin or Doctor
            var userRoles = await _userManager.GetRolesAsync(existingUser);
            if (!userRoles.Contains("Doctor") && !userRoles.Contains("Admin"))
            {
                return new ApiErrorResult<EmployeeResponseModel>("User is not valid.", System.Net.HttpStatusCode.NotFound);
            }

            // Response to client
            var response = _mapper.Map<EmployeeResponseModel>(existingUser);
            if (Enum.IsDefined(typeof(EmployeeStatus), existingUser.Status))
            {
                response.Status = ((EmployeeStatus)existingUser.Status).ToString();
            }
            else
            {
                response.Status = "Unknown";
            }
            return new ApiSuccessResult<EmployeeResponseModel>(response);
        }

        public async Task<ApiResult<UserLoginResponseModel>> ConfirmUserRegister(ConfirmUserRegisterRequest request)
        {
            // Check existed email
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (existingUser == null)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Email is not existed.", System.Net.HttpStatusCode.NotFound);
            }

            // Confirm code 
            var result = await _userManager.ConfirmEmailAsync(existingUser, request.Code);
            if (!result.Succeeded)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Confirm email unsuccessfully", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            existingUser.Status = (int)UserStatus.Active;
            var rs = await _userManager.UpdateAsync(existingUser);

            if (!rs.Succeeded)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Update unsuccessfully", result.Errors.Select(x => x.Description).ToList(), System.Net.HttpStatusCode.BadRequest);
            }
            var refreshTokenData = GenerateRefreshToken();
            var accessTokenData = await GenerateAccessTokenAsync(existingUser);
            existingUser.RefreshToken = refreshTokenData.Item1;
            existingUser.RefreshTokenExpiryTime = refreshTokenData.Item2;

            await _userManager.UpdateAsync(existingUser);
            var response = _mapper.Map<UserLoginResponseModel>(existingUser);
            response.AccessToken = accessTokenData.Item1;
            response.AccessTokenExpiredTime = accessTokenData.Item2;
            response.RefreshToken = refreshTokenData.Item1;
            response.RefreshTokenExpiryTime = refreshTokenData.Item2;
            return new ApiSuccessResult<UserLoginResponseModel>(response, "Register successfully.");

        }

        public ApiResult<List<UserStatusResponseModel>> GetUserStatus()
        {
            var statusList = Enum.GetValues(typeof(UserStatus))
                        .Cast<UserStatus>()
                        .Select(status => new UserStatusResponseModel
                        {
                            Id = (int)status,
                            Status = status.ToString()
                        })
                        .ToList();


            return new ApiSuccessResult<List<UserStatusResponseModel>>(statusList);
        }

        public ApiResult<List<UserStatusResponseModel>> GetEmployeeStatus()
        {
            var statusList = Enum.GetValues(typeof(EmployeeStatus))
                      .Cast<UserStatus>()
                      .Select(status => new UserStatusResponseModel
                      {
                          Id = (int)status,
                          Status = status.ToString()
                      })
                      .ToList();


            return new ApiSuccessResult<List<UserStatusResponseModel>>(statusList);
        }
        private async Task<string> _generateGGUsernameAsync()
        {
            Random random = new Random();
            while (true)
            {
                string username = "GG_" + random.Next(0, 999999);
                var usernameCheckExisted = await _userManager.FindByNameAsync(username);
                if (usernameCheckExisted == null)
                {
                    return username;
                }
            }
        }
        public async Task<ApiResult<UserLoginResponseModel>> UserLoginGoogle(UserLoginGoogleRequest request)
        {
            var userCheckExisted = await _userManager.FindByEmailAsync(request.Email);
            if (userCheckExisted != null)
            {
                var updateStatus = await _userManager.UpdateAsync(userCheckExisted);
                if (!updateStatus.Succeeded)
                {
                    var errorAddUser = updateStatus.Errors.Select(x => x.Description).ToList();
                    return new ApiErrorResult<UserLoginResponseModel>("Login failed.");
                }
                var refreshTokenDataLogged = GenerateRefreshToken();
                var accessTokenDataLogged = await GenerateAccessTokenAsync(userCheckExisted);
                userCheckExisted.RefreshToken = refreshTokenDataLogged.Item1;
                userCheckExisted.RefreshTokenExpiryTime = refreshTokenDataLogged.Item2;

                await _userManager.UpdateAsync(userCheckExisted);
                var responseLogged = _mapper.Map<UserLoginResponseModel>(userCheckExisted);
                responseLogged.AccessToken = accessTokenDataLogged.Item1;
                responseLogged.AccessTokenExpiredTime = accessTokenDataLogged.Item2;
                responseLogged.RefreshToken = refreshTokenDataLogged.Item1;
                responseLogged.RefreshTokenExpiryTime = refreshTokenDataLogged.Item2;

                return new ApiSuccessResult<UserLoginResponseModel>(responseLogged);
            }


            var userEntity = new ApplicationUsers()
            {
                Email = request.Email,
                EmailConfirmed = request.Email_verified,
                FullName = $"{request.Family_name} {request.Given_name} {request.Name}",
                Image = request.Picture,
                UserName = await _generateGGUsernameAsync(),
                Status = (int)UserStatus.Active
            };

            var addUserStatus = await _userManager.CreateAsync(userEntity);
            if (!addUserStatus.Succeeded)
            {
                var errorAddUser = addUserStatus.Errors.Select(x => x.Description).ToList();
                return new ApiErrorResult<UserLoginResponseModel>("Login failed.",errorAddUser);
            }
            
            var addUserToRoleStatus = await _userManager.AddToRoleAsync(userEntity, SystemConstant.Role.USER);
            if (!addUserToRoleStatus.Succeeded)
            {
                var rollbackResult = await _userManager.DeleteAsync(userEntity);
                if (!rollbackResult.Succeeded)
                {
                    var rollbackErrors = rollbackResult.Errors.Select(x => x.Description).ToList();
                    return new ApiErrorResult<UserLoginResponseModel>("Login failed and rollback failed.", rollbackErrors);
                }

                var errorAddUser = addUserToRoleStatus.Errors.Select(x => x.Description).ToList();
                return new ApiErrorResult<UserLoginResponseModel>("Login failed.", errorAddUser);
            }
            var userLoginInfo = new UserLoginInfo(Provider.GOOGLE, request.Sub, Provider.GOOGLE);

            var userLoginGoogle = await _userManager.AddLoginAsync(userEntity, userLoginInfo);

            if (!userLoginGoogle.Succeeded)
            {
                var rollbackResult = await _userManager.DeleteAsync(userEntity);
                if (!rollbackResult.Succeeded)
                {
                    var rollbackErrors = rollbackResult.Errors.Select(x => x.Description).ToList();
                    return new ApiErrorResult<UserLoginResponseModel>("Login failed and rollback failed.", rollbackErrors);
                }

                var errorAddUser = userLoginGoogle.Errors.Select(x => x.Description).ToList();
                return new ApiErrorResult<UserLoginResponseModel>("Login failed.", errorAddUser);
            }
            var refreshTokenData = GenerateRefreshToken();
            var accessTokenData = await GenerateAccessTokenAsync(userEntity);
            userEntity.RefreshToken = refreshTokenData.Item1;
            userEntity.RefreshTokenExpiryTime = refreshTokenData.Item2;

            await _userManager.UpdateAsync(userEntity);
            var response = _mapper.Map<UserLoginResponseModel>(userEntity);
            response.AccessToken = accessTokenData.Item1;
            response.AccessTokenExpiredTime = accessTokenData.Item2;
            response.RefreshToken = refreshTokenData.Item1;
            response.RefreshTokenExpiryTime = refreshTokenData.Item2;

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "Welcome.html");
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                return new ApiErrorResult<UserLoginResponseModel>("Không tìm thấy file gửi mail");
            }
            var content = File.ReadAllText(path);
            content = content.Replace("{{Name}}", userEntity.FullName);
            var resultSendMail = DoingMail.SendMail("BabyCare", "Confirm Email", content, userEntity.Email);

            return new ApiSuccessResult<UserLoginResponseModel>(response);
        }
        #endregion

        #region Authen Admin/Doctor/Employee
        #endregion
    }
}
