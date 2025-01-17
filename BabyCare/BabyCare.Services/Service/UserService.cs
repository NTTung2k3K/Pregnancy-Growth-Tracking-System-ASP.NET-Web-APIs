using AutoMapper;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.ModelViews.UserModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public UserService(IUnitOfWork unitOfWork, UserManager<ApplicationUsers> userManager, IMapper mapper) 
        { 
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public Task<IList<UserResponseModel>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResult<UserLoginResponseModel>> UserLogin(UserLoginRequestModel request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser == null)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.",System.Net.HttpStatusCode.NotFound);
            }
            var validPassword = await _userManager.CheckPasswordAsync(existingUser, request.Password);
            if (!validPassword)
            {
                return new ApiErrorResult<UserLoginResponseModel>("Email hoặc mật khẩu không đúng.", System.Net.HttpStatusCode.NotFound);
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
            return new ApiSuccessResult<UserLoginResponseModel>(response, "Đăng nhập thành công." );

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
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("FullName", user.FullName),
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

        
    }
}
