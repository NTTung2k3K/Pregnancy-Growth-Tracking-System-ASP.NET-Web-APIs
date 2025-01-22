﻿using AutoMapper;
using Azure.Core;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core;
using BabyCare.Core.APIResponse;
using BabyCare.Core.Base;
using BabyCare.Core.Firebase;
using BabyCare.Core.Utils;
using BabyCare.ModelViews.MembershipPackageModelViews.Request;
using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.Services.Service
{
    public class MembershipPackageService : IMembershipPackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private string _tmnCode;
        private string _hashSecret;
        private string _baseUrl;
        private string _callbackUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUsers> _userManager;

        public MembershipPackageService(UserManager<ApplicationUsers> userManager, IConfiguration configuration, IVnpay vnpay, IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _configuration = configuration;

            _tmnCode = configuration["Vnpay:TmnCode"];
            _hashSecret = configuration["Vnpay:HashSecret"];
            _baseUrl = configuration["Vnpay:BaseUrl"];
            _callbackUrl = configuration["Vnpay:ReturnUrl"];


            _vnpay = vnpay;
            _vnpay.Initialize(_tmnCode, _hashSecret, _baseUrl, _callbackUrl);
            _contextAccessor = httpContextAccessor;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<ApiResult<object>> CreateMembershipPackage(CreateMPRequest request)
        {
            var repo = _unitOfWork.GetRepository<MembershipPackage>();
            // Check name is existed
            var existingName = await repo.Entities.FirstOrDefaultAsync(x => x.PackageName == request.PackageName);
            if (existingName != null)
            {
                return new ApiErrorResult<object>("Name of package is existed.");
            }
            var membershipPackage = _mapper.Map<MembershipPackage>(request);
            membershipPackage.Price = (membershipPackage.OriginalPrice - (membershipPackage.OriginalPrice * (membershipPackage.Discount / 100)));
            if (membershipPackage.Price < 0)
            {
                return new ApiErrorResult<object>("Price is not correct");
            }
            if (request.ImageUrl != null)
            {
                membershipPackage.ImageUrl = await ImageHelper.Upload(request.ImageUrl);
            }
            await repo.InsertAsync(membershipPackage);
            await repo.SaveAsync();
            return new ApiSuccessResult<object>("Create successfully.");
        }

        public async Task<ApiResult<object>> DeleteMembershipPackage(DeleteMPRequest request)
        {
            var repo = _unitOfWork.GetRepository<MembershipPackage>();
            // Check mp is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null)
            {
                return new ApiErrorResult<object>("Package is not existed.");
            }
            existingItem.DeletedTime = DateTime.Now;
            existingItem.DeletedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;

            await repo.UpdateAsync(existingItem);
            await repo.SaveAsync();

            return new ApiSuccessResult<object>("Delete successfully.");
        }

        public async Task<ApiResult<MPResponseModel>> GetMembershipPackageById(int id)
        {
            var repo = _unitOfWork.GetRepository<MembershipPackage>();
            // Check mp is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == id);
            if (existingItem == null || existingItem.DeletedBy != null)
            {
                return new ApiErrorResult<MPResponseModel>("Package is not existed.");
            }
            var response = _mapper.Map<MPResponseModel>(existingItem);
            if (Enum.IsDefined(typeof(PackageStatus), existingItem.Status))
            {
                response.Status = ((PackageStatus)existingItem.Status).ToString();
            }
            else
            {
                response.Status = "Unknown";
            }
            if (Enum.IsDefined(typeof(PackageLevel), existingItem.PackageLevel))
            {
                response.PackageLevel = ((PackageLevel)existingItem.PackageLevel).ToString();
            }
            else
            {
                response.PackageLevel = "Unknown";
            }
            return new ApiSuccessResult<MPResponseModel>(response);

        }

        public async Task<ApiResult<BasePaginatedList<MPResponseModel>>> GetMembershipPackagePagination(BaseSearchRequest request)
        {


            var items = _unitOfWork.GetRepository<MembershipPackage>().Entities.Where(x => x.DeletedBy == null);

            // filter by search 
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                items = items.Where(x => x.PackageName.ToLower().Contains(request.SearchValue.ToLower()));
            }
            // paging
            var currentPage = request.PageIndex ?? 1;
            var pageSize = request.PageSize ?? SystemConstant.PAGE_SIZE;
            var total = items.Count();
            var data = await items.Skip((currentPage - 1) * currentPage).Take(pageSize).OrderBy(x => x.ShowPriority).ToListAsync();
            // calculate total page

            var res = data.Select(x => new MPResponseModel
            {
                Id = x.Id,
                PackageName = x.PackageName,
                Status = Enum.IsDefined(typeof(PackageStatus), x.Status)
                                 ? ((PackageStatus)x.Status).ToString()
                                    : "Unknown",
                ShowPriority = x.ShowPriority,
                Discount = x.Discount,
                Price = x.Price.Value,
                PackageLevel = Enum.IsDefined(typeof(PackageLevel), x.PackageLevel)
                                 ? ((PackageLevel)x.PackageLevel).ToString()
                                    : "Unknown",
                Description = x.Description,
                Duration = x.Duration,
                ImageUrl = x.ImageUrl,
                OriginalPrice = x.OriginalPrice

            }).ToList();

            var response = new BasePaginatedList<MPResponseModel>(res, total, currentPage, pageSize);
            // return to client
            return new ApiSuccessResult<BasePaginatedList<MPResponseModel>>(response);

        }

        public ApiResult<MPStatusHandleResponseModel> GetMembershipPackageStatusHandler()
        {
            var statusList = Enum.GetValues(typeof(PackageStatus))
                     .Cast<PackageStatus>()
                     .Select(status => new MPStatusResponseModel
                     {
                         Id = (int)status,
                         Status = status.ToString()
                     })
                     .ToList();
            var level = Enum.GetValues(typeof(PackageLevel))
                     .Cast<PackageLevel>()
                     .Select(status => new MPStatusResponseModel
                     {
                         Id = (int)status,
                         Status = status.ToString()
                     })
                     .ToList();
            var result = new MPStatusHandleResponseModel()
            {
                PackageLevel = level,
                Status = statusList,
            };
            return new ApiSuccessResult<MPStatusHandleResponseModel>(result);

        }

        public async Task<ApiResult<object>> UpdateMembershipPackage(UpdateMPRequest request)
        {
            var repo = _unitOfWork.GetRepository<MembershipPackage>();
            // Check mp is existed
            var existingItem = await repo.Entities.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (existingItem == null)
            {
                return new ApiErrorResult<object>("Package is not existed.");
            }

            _mapper.Map(request, existingItem);
            existingItem.Price = (request.OriginalPrice - (request.OriginalPrice * (request.Discount / 100)));
            if (existingItem.Price < 0)
            {
                return new ApiErrorResult<object>("Price is not correct");
            }
            existingItem.LastUpdatedTime = DateTime.Now;
            existingItem.LastUpdatedBy = _contextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            if (request.ImageUrl != null)
            {
                existingItem.ImageUrl = await ImageHelper.Upload(request.ImageUrl);
            }
            await repo.UpdateAsync(existingItem);
            await repo.SaveAsync();

            return new ApiSuccessResult<object>("Update successfully.");
        }

        public async Task<ApiResult<BuyPackageResponse>> BuyPackage(BuyPackageRequest request, string ipAddress)
        {
            var package = await _unitOfWork.GetRepository<MembershipPackage>().GetByIdAsync(request.PackageId);
            if (package == null)
            {
                return new ApiErrorResult<BuyPackageResponse>("Package is not existed");
            }
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return new ApiErrorResult<BuyPackageResponse>("User is not existed");
            }
            var res = new BuyPackageResponse()
            {
                RedirectUrlVnPay = CreatePaymentUrl((double)package.Price, $"{user.Id}|{package.Id}", ipAddress)
            };
            return new ApiSuccessResult<BuyPackageResponse>(res);

        }
        private string CreatePaymentUrl(double moneyToPay, string description, string IpAddress)
        {


            var request = new PaymentRequest
            {
                PaymentId = DateTime.Now.Ticks,
                Money = moneyToPay,
                Description = description,
                IpAddress = IpAddress,
                BankCode = BankCode.ANY, // Tùy chọn. Mặc định là tất cả phương thức giao dịch
                CreatedDate = DateTime.Now, // Tùy chọn. Mặc định là thời điểm hiện tại
                Currency = Currency.VND, // Tùy chọn. Mặc định là VND (Việt Nam đồng)
                Language = DisplayLanguage.Vietnamese // Tùy chọn. Mặc định là tiếng Việt
            };

            var paymentUrl = _vnpay.GetPaymentUrl(request);

            return paymentUrl;

        }

        public async Task<ApiResult<object>> HandleIpnActionVNpay(VNPayCallbackResponse request)
        {

            var isSuccess = request.IsSuccess;
            _unitOfWork.BeginTransaction();
            var data = request.Description.Split("|");
            var packageId = int.Parse(data.ElementAt(1));
            var package = await _unitOfWork.GetRepository<MembershipPackage>().Entities.FirstOrDefaultAsync(x => x.Id == packageId);  
            var userId = Guid.Parse(data.ElementAt(0));
            var userMembership = new UserMembership()
            {
                PackageId = packageId,
                UserId = userId,
                CreatedBy = userId.ToString(),
                CreatedTime = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(package.Duration),
                Status = isSuccess ? SystemConstant.MembershipPackageStatus.Active : MembershipPackageStatus.InActive,
            };
            await _unitOfWork.GetRepository<UserMembership>().InsertAsync(userMembership);
            await _unitOfWork.SaveAsync();


            Payment payment = null;
            if (isSuccess)
            {
                payment = new Payment()
                {
                    CreatedTime = DateTime.Now,
                    CreatedBy = data.ElementAt(0),
                    Amount = package.Price.Value,
                    PaymentMethod = request.PaymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = "Success",
                    Membership = userMembership,
                };
            }
            else
            {
                payment = new Payment()
                {
                    CreatedTime = DateTime.Now,
                    CreatedBy = data.ElementAt(0),
                    Amount = package.Price.Value,
                    PaymentMethod = request.PaymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = "Failed",
                    Membership = userMembership,
                };
            }
            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
            await _unitOfWork.SaveAsync();
            _unitOfWork.CommitTransaction();
            return new ApiSuccessResult<object>("Buy successfully.");


        }

        public async Task<ApiResult<string>> HandleIpnActionVNpayBackEnd(IQueryCollection query)
        {
            var paymentResult = _vnpay.GetPaymentResult(query);

            var isSuccess = paymentResult.IsSuccess;
            _unitOfWork.BeginTransaction();
            var data = paymentResult.Description.Split("|");
            var packageId = int.Parse(data.ElementAt(1));
            var package = await _unitOfWork.GetRepository<MembershipPackage>().Entities.FirstOrDefaultAsync(x => x.Id == packageId);
            var userId = Guid.Parse(data.ElementAt(0));
            var userMembership = new UserMembership()
            {
                PackageId = packageId,
                UserId = userId,
                CreatedBy = userId.ToString(),
                CreatedTime = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(package.Duration),
                Status = isSuccess ? SystemConstant.MembershipPackageStatus.Active : MembershipPackageStatus.InActive,
            };
            await _unitOfWork.GetRepository<UserMembership>().InsertAsync(userMembership);
            await _unitOfWork.SaveAsync();

            string url;
            Payment payment = null;
            if (isSuccess)
            {
                payment = new Payment()
                {
                    CreatedTime = DateTime.Now,
                    CreatedBy = data.ElementAt(0),
                    Amount = package.Price.Value,
                    PaymentMethod = paymentResult.PaymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = "Success",
                    Membership = userMembership,
                };
                url = _configuration["URL:FrontEnd"] + "membershippackages/payment-result?result=true";
            }
            else
            {
                payment = new Payment()
                {
                    CreatedTime = DateTime.Now,
                    CreatedBy = data.ElementAt(0),
                    Amount = package.Price.Value,
                    PaymentMethod = paymentResult.PaymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = "Failed",
                    Membership = userMembership,
                };
                url = _configuration["URL:FrontEnd"] + "membershippackages/payment-result?result=false";

            }
            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
            await _unitOfWork.SaveAsync();
            _unitOfWork.CommitTransaction();

            return new ApiSuccessResult<string>(url, "Buy successfully.");

        }
    }
}
