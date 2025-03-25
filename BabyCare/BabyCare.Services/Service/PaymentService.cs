using AutoMapper;
using Azure;
using BabyCare.Contract.Repositories.Entity;
using BabyCare.Contract.Repositories.Interface;
using BabyCare.Contract.Services.Interface;
using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.MembershipPackageModelViews.Response;
using BabyCare.ModelViews.PaymentModelView.Response;
using BabyCare.ModelViews.UserMembershipModelView.Response;
using BabyCare.ModelViews.UserModelViews.Response;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BabyCare.Core.Utils.SystemConstant;

namespace BabyCare.Services.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly IMapper _mapper;
        public PaymentService(IUnitOfWork unitOfWork, UserManager<ApplicationUsers> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }
        public ApiResult<List<object>> GetMonthlyPaymentStatistics()
        {
            var currentYear = DateTime.Now.Year;
            var paymentRepo = _unitOfWork.GetRepository<Payment>();
            var paymentCounts = new List<object>();

            for (int month = 1; month <= 12; month++)
            {
                var count = paymentRepo.Entities
                    .Where(p => p.PaymentDate.Year == currentYear && p.PaymentDate.Month == month && p.Status == "Success")
                    .Count();

                var totalAmount = paymentRepo.Entities
                    .Where(p => p.PaymentDate.Year == currentYear && p.PaymentDate.Month == month && p.Status == "Success")
                    .Sum(p => p.Amount);

                paymentCounts.Add(new { month, transactionCount = count, totalAmount });
            }

            return new ApiSuccessResult<List<object>>(paymentCounts);
        }
        public async Task<ApiResult<List<PaymentResponseModel>>> GetRecentTransactions(int quantity)
        {
            var paymentRepo = _unitOfWork.GetRepository<Payment>();
            var userMembershipRepo = _unitOfWork.GetRepository<UserMembership>();
            var membershipPackageRepo = _unitOfWork.GetRepository<MembershipPackage>();

            var recentTransactions =  paymentRepo.GetAll()
                .OrderByDescending(p => p.PaymentDate)
                .Take(quantity)
                .ToList();
            var response = new List<PaymentResponseModel>();
            foreach (var item in recentTransactions)
            {
                var paymentRes = _mapper.Map<PaymentResponseModel>(item);
                paymentRes.UserMembership = _mapper.Map<UserMembershipResponse>(userMembershipRepo.GetById(item.MembershipId));
                paymentRes.UserMembership.Package = _mapper.Map<MPResponseModel>(membershipPackageRepo.GetById(paymentRes.UserMembership.Package.Id));
                paymentRes.UserMembership.User =  _mapper.Map<UserResponseModel>(await(_userManager.FindByIdAsync(paymentRes.UserMembership.User.Id.ToString())));
                response.Add(paymentRes);
            }
            return new ApiSuccessResult<List<PaymentResponseModel>>(response);
        }


        public ApiResult<decimal> GetTotalRevenueForCurrentYear()
        {
            var currentYear = DateTime.Now.Year;
            var paymentRepo = _unitOfWork.GetRepository<Payment>();

            var totalRevenue =  paymentRepo.GetAll()
                .Where(p => p.PaymentDate.Year == currentYear && p.Status == "Success")
                .Sum(p => p.Amount);

            return new ApiSuccessResult<decimal>(totalRevenue);
        }

        public ApiResult<object> GetMonthWithMaxTransactions()
        {
            var currentYear = DateTime.Now.Year;
            var paymentRepo = _unitOfWork.GetRepository<Payment>();

            var payments =  paymentRepo.GetAll()
                .Where(p => p.PaymentDate.Year == currentYear)
                .ToList();

            var maxTransactions = payments
                .GroupBy(p => p.PaymentDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TransactionCount = g.Count()
                })
                .OrderByDescending(s => s.TransactionCount)
                .FirstOrDefault();

            return new ApiSuccessResult<object>(maxTransactions);
        }

        public async Task<ApiResult<List<PaymentResponseModel>>> GetAll()
        {
            var paymentRepo = _unitOfWork.GetRepository<Payment>();
            var userMembershipRepo = _unitOfWork.GetRepository<UserMembership>();
            var membershipPackageRepo = _unitOfWork.GetRepository<MembershipPackage>();
            var payment = paymentRepo.GetAll().OrderByDescending(x => x.LastUpdatedTime);
            var response = new List<PaymentResponseModel>();
            foreach (var item in payment)
            {
                var paymentRes = _mapper.Map<PaymentResponseModel>(item);
                paymentRes.UserMembership = _mapper.Map<UserMembershipResponse>(userMembershipRepo.GetById(item.MembershipId));
                paymentRes.UserMembership.Package = _mapper.Map<MPResponseModel>(membershipPackageRepo.GetById(paymentRes.UserMembership.Package.Id));
                paymentRes.UserMembership.User = _mapper.Map<UserResponseModel>(await (_userManager.FindByIdAsync(paymentRes.UserMembership.User.Id.ToString())));
                response.Add(paymentRes);
            }
            return new ApiSuccessResult<List<PaymentResponseModel>>(response);

        }

        public async Task<ApiResult<PaymentResponseModel>> GetById(int id)
        {
            var paymentRepo = _unitOfWork.GetRepository<Payment>();
            var userMembershipRepo = _unitOfWork.GetRepository<UserMembership>();
            var membershipPackageRepo = _unitOfWork.GetRepository<MembershipPackage>();
            var payment = await paymentRepo.GetByIdAsync(id);
            var response = _mapper.Map<PaymentResponseModel>(payment);

            response.UserMembership = _mapper.Map<UserMembershipResponse>(userMembershipRepo.GetById(payment.MembershipId));
            response.UserMembership.Package = _mapper.Map<MPResponseModel>(membershipPackageRepo.GetById(response.UserMembership.Package.Id));
            response.UserMembership.User = _mapper.Map<UserResponseModel>(await(_userManager.FindByIdAsync(response.UserMembership.User.Id.ToString())));
            return new ApiSuccessResult<PaymentResponseModel>(response);
        }
    }
}
