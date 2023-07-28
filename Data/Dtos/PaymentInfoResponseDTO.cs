using System;

namespace StripeApp.Data.Dtos
{
    public class PaymentInfoResponseDTO
    {
        public Guid PaymentId { get; set; }
        public string PaymentIntentId { get; set; }
        public int PaymentStatusId { get; set; }
        public double Amount { get; set; }
    }
}