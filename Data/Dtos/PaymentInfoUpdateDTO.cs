using System;
namespace StripeApp.Data.Dtos
{
    public class PaymentInfoUpdateDTO
    {
        public string PaymentIntentId { get; set; }
        public bool? IsRefund { get; set; }
    }
}