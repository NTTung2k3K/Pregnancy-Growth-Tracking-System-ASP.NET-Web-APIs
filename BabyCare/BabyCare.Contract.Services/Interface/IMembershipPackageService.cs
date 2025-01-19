using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
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
        ApiResult<MPStatusHandleResponseModel> GetMembershipPackageStatusHandler();
        Task<ApiResult<object>> HandleIpnActionVNpay(VNPayCallbackResponse request);


        Task<ApiResult<BuyPackageResponse>> BuyPackage(BuyPackageRequest request,string ipAddress);

    }
}
