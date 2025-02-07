using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.ModelViews.AuthModelViews.Request;
using BabyCare.ModelViews.AuthModelViews.Response;
using BabyCare.ModelViews.UserModelViews.Request;
using BabyCare.ModelViews.UserModelViews.Response;

namespace BabyCare.Contract.Services.Interface
{
    public interface IUserService
    {
        Task<ApiResult<EmployeeLoginResponseModel>> RefreshToken(NewRefreshTokenRequestModel request);
        Task<ApiResult<DashboardUserCreateResponse>> GetUsersByCreateTime();


        #region Authen User
        Task<ApiResult<UserLoginResponseModel>> UserLogin(UserLoginRequestModel request);
        Task<ApiResult<UserLoginResponseModel>> UserLoginGoogle(UserLoginGoogleRequest request);

        Task<ApiResult<object>> UserRegister(UserRegisterRequestModel request);
        Task<ApiResult<UserLoginResponseModel>> ConfirmUserRegister(ConfirmUserRegisterRequest request);

        Task<ApiResult<object>> ForgotPassword(ForgotPasswordRequest request);
        Task<ApiResult<object>> ResetPassword(ResetPasswordRequestModel request);
        #endregion

        #region Authen Admin/Doctor/Employee
        Task<ApiResult<EmployeeLoginResponseModel>> EmployeeLogin(EmployeeLoginRequestModel request);
        Task<ApiResult<object>> EmployeeForgotPassword(ForgotPasswordRequest request);
        Task<ApiResult<object>> EmployeeResetPassword(ResetPasswordRequestModel request);
        #endregion

        #region User

        Task<ApiResult<object>> UpdateUserProfile(UpdateUserProfileRequest request);
        // Get user pagination 
        Task<ApiResult<BasePaginatedList<UserResponseModel>>> GetUserPagination(BaseSearchRequest request);
        Task<ApiResult<UserResponseModel>> GetUserById(Guid Id);
        Task<ApiResult<object>> DeleteUser(DeleteUserRequest request);
        Task<ApiResult<object>> UpdateUserStatus(UpdateUserStatusRequest request);
        ApiResult<List<UserStatusResponseModel>> GetUserStatus();



        #endregion

        #region Admin/Doctor/Employee
        ApiResult<List<UserStatusResponseModel>> GetEmployeeStatus();

        Task<ApiResult<object>> CreateEmployee(CreateEmployeeRequest request);
        Task<ApiResult<object>> UpdateEmployeeProfile(UpdateEmployeeProfileRequest request);
        Task<ApiResult<object>> UpdateEmployeeStatus(UpdateUserStatusRequest request);
        Task<ApiResult<object>> DeleteEmployee(DeleteUserRequest request);
        Task<ApiResult<BasePaginatedList<EmployeeResponseModel>>> GetDoctorPagination(BaseSearchRequest request);
        Task<ApiResult<List<EmployeeResponseModel>>> GetAllDoctor();
        Task<ApiResult<List<UserResponseModel>>> GetAllUser();

        Task<ApiResult<EmployeeResponseModel>> GetEmployeeById(Guid Id);
        Task<ApiResult<UploadImageResponseModel>> UploadImage(UploadImageRequest  request);



        #endregion



    }
}
