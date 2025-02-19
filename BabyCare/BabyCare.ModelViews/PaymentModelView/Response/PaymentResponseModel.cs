using BabyCare.ModelViews.UserMembershipModelView.Response;

namespace BabyCare.ModelViews.PaymentModelView.Response
{
    public class PaymentResponseModel
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string? Status { get; set; }
        public UserMembershipResponse UserMembership { get; set; }
    }
}
