using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.PaymentModelView.Response;

namespace BabyCare.Contract.Services.Interface
{
    public interface IPaymentService
    {
        Task<ApiResult<List<PaymentResponseModel>>> GetAll();
        Task<ApiResult<PaymentResponseModel>> GetById(int id);
        ApiResult<List<object>> GetMonthlyPaymentStatistics();
        Task<ApiResult<List<PaymentResponseModel>>> GetRecentTransactions(int quantity);
        ApiResult<decimal> GetTotalRevenueForCurrentYear();
        ApiResult<object> GetMonthWithMaxTransactions();
    }
}
