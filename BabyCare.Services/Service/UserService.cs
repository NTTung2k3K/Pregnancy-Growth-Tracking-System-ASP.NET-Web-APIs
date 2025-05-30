﻿using AutoMapper;
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
using BabyCare.ModelViews.ChildModelView;
using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using BabyCare.ModelViews.UserMembershipModelView.Response;
using BabyCare.ModelViews.UserModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
                return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.");
            }
            if (existingUser.DeletedBy != null)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.");
            }
            var validPassword = await _userManager.CheckPasswordAsync(existingUser, request.Password);
            if (!validPassword)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.");
            }
            var roles = await _userManager.GetRolesAsync(existingUser);
            foreach (var role in roles)
            {
                if (role != SystemConstant.Role.USER)
                {
                    return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.");
                }
            }
            var isConfirmed = await _userManager.IsEmailConfirmedAsync(existingUser);
            if (!isConfirmed)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.");

            }

            if (existingUser.Status == ((int)SystemConstant.UserStatus.InActive))
            {
                return new ApiErrorResult<UserLoginResponseModel>("You cannot access system.");

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

        public async Task<ApiResult<DashboardUserCreateResponse>> GetUsersByCreateTime()
        {
            var now = DateTime.Now;

            var users = await _userManager.Users.ToListAsync(); // Lấy tất cả users để xử lý
            var usersWithRoleUser = new List<ApplicationUsers>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(SystemConstant.Role.USER)) // Kiểm tra user có role "User"
                {
                    usersWithRoleUser.Add(user);
                }
            }

            var usersY = usersWithRoleUser.Where(u => u.CreatedTime.Year == now.Year).ToList();
            var usersM = usersWithRoleUser.Where(u => u.CreatedTime.Month == now.Month).ToList();
            var usersD = usersWithRoleUser.Where(u => u.CreatedTime.Date == now.Date).ToList();

            var res = new DashboardUserCreateResponse()
            {
                InDay = usersD.Count,
                InMonth = usersM.Count,
                InYear = usersY.Count,
            };

            return new ApiSuccessResult<DashboardUserCreateResponse>(res);
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
                Gender = (int)Gender.Female,
                FullName = request.Email,
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

        public async Task<ApiResult<EmployeeLoginResponseModel>> RefreshToken(NewRefreshTokenRequestModel request)
        {

            // Check existed user
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingUser == null)
            {
                return new ApiErrorResult<EmployeeLoginResponseModel>("User is not existed.", System.Net.HttpStatusCode.NotFound);
            }
            // Check refresh token
            if (existingUser.RefreshToken != request.RefreshToken)
            {
                return new ApiErrorResult<EmployeeLoginResponseModel>("Refresh token is not correct.", System.Net.HttpStatusCode.BadRequest);
            }
            // Check expired time
            if (existingUser.RefreshTokenExpiryTime < DateTime.Now)
            {
                return new ApiErrorResult<EmployeeLoginResponseModel>("Refresh token is expired.", System.Net.HttpStatusCode.BadRequest);
            }
            // Generate new refresh token
            var refreshTokenData = GenerateRefreshToken();
            var accessTokenData = await GenerateAccessTokenAsync(existingUser);
            existingUser.RefreshToken = refreshTokenData.Item1;
            existingUser.RefreshTokenExpiryTime = refreshTokenData.Item2;
            await _userManager.UpdateAsync(existingUser);
            // Response to client
            var response = _mapper.Map<EmployeeLoginResponseModel>(existingUser);
            response.AccessToken = accessTokenData.Item1;
            response.AccessTokenExpiredTime = accessTokenData.Item2;
            response.FullName = existingUser.FullName ?? "Unknown";

            // Take role
            var roles = await _userManager.GetRolesAsync(existingUser);
            response.Roles = roles.ToList();
            return new ApiSuccessResult<EmployeeLoginResponseModel>(response, "Refresh token successfully.");
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
            var encodedToken = Uri.EscapeDataString(token);


            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "SendCodeCustomer.html");
            path = Path.GetFullPath(path);


            if (!System.IO.File.Exists(path))
            {
                return new ApiErrorResult<object>("System error, try later", System.Net.HttpStatusCode.NotFound);
            }
            var frontEndUrl = _configuration["URL:FrontEnd"];
            var fullForgotPasswordUrl = frontEndUrl + "auth/new-password?email=" + email + "&token=" + encodedToken;
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
            if (request.ConfirmPassword != request.Password)
            {
                return new ApiErrorResult<object>("Password is not matched.", System.Net.HttpStatusCode.NotFound);
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
            response.FullName = existingUser.FullName ?? "Unknown";
            // Take role
            var roles = await _userManager.GetRolesAsync(existingUser);
            response.Roles = roles.ToList();
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
            var encodedToken = Uri.EscapeDataString(token);

            // Correct relative path from current directory to the HTML file
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "SendCode.html");
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                return new ApiErrorResult<object>("Không tìm thấy file gửi mail");
            }

            var frontEndUrl = _configuration["URL:FrontEnd"];
            var fullForgotPasswordUrl = frontEndUrl + "auth/new-password?email=" + email + "&token=" + encodedToken;
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
            if (!Enum.IsDefined(typeof(SystemConstant.Gender), request.Gender))
            {
                return new ApiErrorResult<object>("Gender is not valid.", System.Net.HttpStatusCode.BadRequest);
            }
            var existingImage = existingUser.Image;

            // Update user profile by mapper
            _mapper.Map(request, existingUser);

            if (request.Image != null)
            {
                existingUser.Image = await ImageHelper.Upload(request.Image);
            }
            else
            {
                existingUser.Image = existingImage;
            }
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
                Image = x.Image,
                Id = x.Id,
                Status = Enum.IsDefined(typeof(EmployeeStatus), x.Status)
                               ? ((EmployeeStatus)x.Status).ToString()
                                  : "Unknown",
                Gender = Enum.IsDefined(typeof(Gender), x.Gender)
                               ? ((Gender)x.Gender).ToString()
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
            var existingUser = await _userManager.Users.Include(x => x.Children).FirstOrDefaultAsync(x => x.Id == Id && x.DeletedBy == null);
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
            if (existingUser.Gender != null)
            {
                if (Enum.IsDefined(typeof(Gender), existingUser.Gender))
                {
                    response.Gender = ((Gender)existingUser.Gender).ToString();
                }
            }
            else
            {
                response.Gender = "Unknown";
            }


            var userChilds = _mapper.Map<List<ChildModelView>>(existingUser.Children);
            response.Childs = userChilds;


            response.UserMembershipResponses = new();
            foreach (var item in existingUser.UserMemberships.OrderByDescending(x => x.EndDate))
            {
                var userMembership = new UserMembershipResponse()
                {
                    AddedRecordCount = item.AddedRecordCount,
                    EndDate = item.EndDate,
                    GrowthChartShareCount = item.GrowthChartShareCount,
                    Id = item.Id,
                    StartDate = item.StartDate,
                    Status = item.Status,
                    Package = new MPResponseModel()
                    {
                        Status = Enum.IsDefined(typeof(PackageStatus), item.Package.Status)
                               ? ((PackageStatus)item.Package.Status.Value).ToString()
                                  : "Unknown",
                        Id = item.PackageId,
                        Description = item.Package.Description,
                        Discount = item.Package.Discount,
                        Duration = item.Package.Duration,
                        Price = item.Package.Price.Value,
                        PackageLevel = Enum.IsDefined(typeof(PackageLevel), item.Package.PackageLevel.Value)
                               ? ((PackageLevel)item.Package.PackageLevel.Value).ToString()
                                  : "Unknown",
                        PackageName = item.Package.PackageName,
                        OriginalPrice = item.Package.OriginalPrice,
                        ShowPriority = item.Package.ShowPriority,
                        HasGenerateAppointments = item.Package.HasGenerateAppointments,
                        HasStandardDeviationAlerts = item.Package.HasStandardDeviationAlerts,
                        HasViewGrowthChart = item.Package.HasViewGrowthChart,
                        MaxGrowthChartShares = item.Package.MaxGrowthChartShares,
                        MaxRecordAdded = item.Package.MaxRecordAdded
                    }
                };
                response.UserMembershipResponses.Add(userMembership);
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
            if (!Enum.IsDefined(typeof(SystemConstant.Gender), request.Gender))
            {
                return new ApiErrorResult<object>("Gender is not valid.", System.Net.HttpStatusCode.BadRequest);
            }

            // Createe user use mapper
            var user = _mapper.Map<ApplicationUsers>(request);
            user.Status = (int)EmployeeStatus.Active;
            user.Gender = request.Gender;

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
            if (!Enum.IsDefined(typeof(SystemConstant.Gender), request.Gender))
            {
                return new ApiErrorResult<object>("Gender is not valid.", System.Net.HttpStatusCode.BadRequest);
            }
            if (_contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value == null)
            {
                return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
            }
            var existingImage = existingUser.Image;

            // Update user profile by mapper
            _mapper.Map(request, existingUser);
            existingUser.LastUpdatedTime = DateTime.Now;
            existingUser.LastUpdatedBy = Guid.Parse(_contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value);
            existingUser.Gender = request.Gender;
            if (request.Image != null)
            {
                existingUser.Image = await ImageHelper.Upload(request.Image);
            }
            else
            {
                existingUser.Image = existingImage;
            }
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
            if (_contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value == null)
            {
                return new ApiErrorResult<object>("Plase login to use this function.", System.Net.HttpStatusCode.BadRequest);
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
                Gender = Enum.IsDefined(typeof(Gender), x.Gender)
                               ? ((Gender)x.Gender).ToString()
                                  : "Unknown",
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
            if (existingUser.DeletedBy != null)
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
            if (Enum.IsDefined(typeof(Gender), existingUser.Gender))
            {
                response.Gender = ((Gender)existingUser.Gender).ToString();
            }
            else
            {
                response.Gender = "Unknown";
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
            response.FullName = existingUser.FullName ?? "Unknown";

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
                if (userCheckExisted.Status == ((int)SystemConstant.UserStatus.InActive))
                {
                    return new ApiErrorResult<UserLoginResponseModel>("You cannot access system.", System.Net.HttpStatusCode.NotFound);
                }
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
                Status = (int)UserStatus.Active,
                Gender = 0
            };

            var addUserStatus = await _userManager.CreateAsync(userEntity);
            if (!addUserStatus.Succeeded)
            {
                var errorAddUser = addUserStatus.Errors.Select(x => x.Description).ToList();
                return new ApiErrorResult<UserLoginResponseModel>("Login failed.", errorAddUser);
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

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FormSendEmail", "WelcomeGG.html");
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                return new ApiErrorResult<UserLoginResponseModel>("Không tìm thấy file gửi mail");
            }
            var content = File.ReadAllText(path);
            content = content.Replace("{{Name}}", userEntity.FullName);
            var resultSendMail = DoingMail.SendMail("BabyCare", "Welcome", content, userEntity.Email);

            return new ApiSuccessResult<UserLoginResponseModel>(response);
        }

        public async Task<ApiResult<List<EmployeeResponseModel>>> GetAllDoctor()
        {
            var doctorRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == SystemConstant.Role.DOCTOR);


            // Filter users

            var doctorUserIds = await _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
        .Where(ur => ur.RoleId == doctorRole.Id)
        .OrderByDescending(x => x.LastUpdatedTime)
        .Select(ur => ur.UserId)
        .ToListAsync();

            // Lọc danh sách user theo UserId từ bảng User
            var users = await _userManager.Users
                .Where(u => doctorUserIds.Contains(u.Id) && u.DeletedBy == null).ToListAsync();



            var items = users.Select(x => new EmployeeResponseModel
            {
                Address = x.Address,
                DateOfBirth = x.DateOfBirth,
                FullName = x.FullName,
                Email = x.Email,
                Gender = Enum.IsDefined(typeof(Gender), x.Gender)
                               ? ((Gender)x.Gender).ToString()
                                  : "Unknown",
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

            // return to client
            return new ApiSuccessResult<List<EmployeeResponseModel>>(items);
        }

        public async Task<ApiResult<List<UserResponseModel>>> GetAllUser()
        {
            var userRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == SystemConstant.Role.USER);


            // Filter users

            var UserIds = await _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
        .Where(ur => ur.RoleId == userRole.Id)
        .Select(ur => ur.UserId)
        .ToListAsync();

            // Lọc danh sách user theo UserId từ bảng User
            var users = await _userManager.Users
                .Where(u => UserIds.Contains(u.Id) && u.DeletedBy == null)
                .OrderByDescending(x => x.LastUpdatedTime)
                .ToListAsync();



            var items = users.Select(x => new UserResponseModel
            {
                Address = x.Address,
                DateOfBirth = x.DateOfBirth,
                FullName = x.FullName,
                Gender = x.Gender == null ? "Unknown" : Enum.IsDefined(typeof(Gender), x.Gender)
                               ? ((Gender)x.Gender).ToString()
                                  : "Unknown",
                Image = x.Image,
                IsEmailConfirmed = x.EmailConfirmed,
                Id = x.Id,
                Status = Enum.IsDefined(typeof(EmployeeStatus), x.Status)
                               ? ((EmployeeStatus)x.Status).ToString()
                                  : "Unknown",
                BloodGroup = x.BloodGroup,
                CreatedBy = x.CreatedBy.ToString(),
                Email = x.Email,
                LastUpdatedBy = x.LastUpdatedBy.ToString()

            }).ToList();

            // return to client
            return new ApiSuccessResult<List<UserResponseModel>>(items);
        }

        public async Task<ApiResult<UploadImageResponseModel>> UploadImage(UploadImageRequest request)
        {
            string res = await BabyCare.Core.Firebase.ImageHelper.Upload(request.Image);
            return new ApiSuccessResult<UploadImageResponseModel>(new UploadImageResponseModel { ImageUrl = res });
        }

        public async Task<ApiResult<List<EmployeeResponseModel>>> GetAllEmployee()
        {
            // Lấy danh sách Role có tên DOCTOR hoặc ADMIN
            var doctorAdminRoles = await _roleManager.Roles
                .Where(r => r.Name == SystemConstant.Role.DOCTOR || r.Name == SystemConstant.Role.ADMIN)
                .ToListAsync();

            if (doctorAdminRoles == null || !doctorAdminRoles.Any())
            {
                return new ApiSuccessResult<List<EmployeeResponseModel>>(new List<EmployeeResponseModel>());
            }

            // Lấy danh sách RoleId tương ứng
            var roleIds = doctorAdminRoles.Select(r => r.Id).ToList();

            // Lấy danh sách UserId của những người có RoleId nằm trong danh sách roleIds
            var doctorAdminUserIds = await _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
                .Where(ur => roleIds.Contains(ur.RoleId))
                .OrderByDescending(x => x.LastUpdatedTime)
                .Select(ur => ur.UserId)
                .ToListAsync();

            // Lọc danh sách user theo UserId từ bảng User
            var users = await _userManager.Users
                .Where(u => doctorAdminUserIds.Contains(u.Id) && u.DeletedBy == null)
                .ToListAsync();

            // Trả về danh sách nhân viên
            var items = users.Select(user => new EmployeeResponseModel
            {
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                FullName = user.FullName,
                Email = user.Email,
                Gender = Enum.IsDefined(typeof(Gender), user.Gender) ? ((Gender)user.Gender).ToString() : "Unknown",
                Image = user.Image,
                Id = user.Id,
                Status = Enum.IsDefined(typeof(EmployeeStatus), user.Status) ? ((EmployeeStatus)user.Status).ToString() : "Unknown",

                // Lấy role phù hợp của user
                Role = new ModelViews.RoleModelViews.RoleModelView()
                {
                    Id = doctorAdminRoles.FirstOrDefault(r => r.Id ==
                         _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
                         .Where(ur => ur.UserId == user.Id)
                         .Select(ur => ur.RoleId)
                         .FirstOrDefault()
                    )?.Id.ToString(),
                    Name = doctorAdminRoles.FirstOrDefault(r => r.Id ==
                         _unitOfWork.GetRepository<ApplicationUserRoles>().Entities
                         .Where(ur => ur.UserId == user.Id)
                         .Select(ur => ur.RoleId)
                         .FirstOrDefault()
                    )?.Name
                }
            }).ToList();

            return new ApiSuccessResult<List<EmployeeResponseModel>>(items);
        }

        #endregion


    }
}
