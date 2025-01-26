using BabyCare.Core.APIResponse;
using BabyCare.ModelViews.PaymentModelView.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BabyCare.Contract.Services.Interface
{
    public interface IPaymentService
    {
        Task<ApiResult<List<PaymentResponseModel>>> GetAll();
        Task<ApiResult<PaymentResponseModel>> GetById(int id);

    }
}
