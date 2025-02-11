using BabyCare.Contract.Repositories.Entity;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.Core.Utils;
using BabyCare.ModelViews.MembershipPackageModelViews.Request;
using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Services.Interface
{
    public interface IMembershipPackageService
    {
        Task<ApiResult<object>> CreateMembershipPackage(CreateMPRequest request);
        Task<ApiResult<object>> UpdateMembershipPackage(UpdateMPRequest request);
        Task<ApiResult<BasePaginatedList<MPResponseModel>>> GetMembershipPackagePagination(BaseSearchRequest request);
        Task<ApiResult<object>> DeleteMembershipPackage(DeleteMPRequest id);
        Task<ApiResult<MPResponseModel>> GetMembershipPackageById(int id);
        Task<ApiResult<List<MPResponseModel>>> GetAll();
        ApiResult<MPStatusHandleResponseModel> GetMembershipPackageStatusHandler();
        Task<ApiResult<object>> HandleIpnActionVNpay(VNPayCallbackResponse request);
        Task<ApiResult<string>> HandleIpnActionVNpayBackEnd(IQueryCollection query);
        

        Task<ApiResult<BuyPackageResponse>> BuyPackage(BuyPackageRequest request, string ipAddress);




        // ✅ Lấy gói hiện tại của user (nếu có)
        Task<UserMembership?> GetUserActiveMembership(Guid userId);
        // ✅ Lấy gói MembershipPackage hiện tại của user
        Task<MembershipPackage?> GetUserActivePackage(Guid userId);

        Task<bool> AddedRecord(Guid userId);



        //  Lấy số lần chia sẻ Growth Chart tối đa dựa trên gói của user
        Task<int> GetMaxGrowthChartShares(Guid userId);


        //  Kiểm tra xem user còn quyền chia sẻ Growth Chart không
        Task<bool> CanShareGrowthChart(Guid userId);


        //  Tăng số lần chia sẻ Growth Chart
        Task<bool> ShareGrowthChart(Guid userId);

        //  Kiểm tra user có thể tạo lịch hẹn không
        Task<ApiResult<bool>> CanGenerateAppointments(Guid userId);

        Task<ApiResult<bool>> CanViewGrowthChart(Guid userId);

        Task<bool> CanAddedRecord(Guid userId);
        //  Kiểm tra user có thông báo lệch chuẩn không
        Task<bool> HasStandardDeviationAlerts(Guid userId);




    }

}
