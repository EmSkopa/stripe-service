using System;

namespace StripeApp.Data.Dtos
{
    public class PaymentIntentResponseDTO
    {
        public bool Success { get; set; }
        public bool RequiresAction { get; set; }
        public string PaymentIntentClientSecret { get; set; }
        public Guid TransactionId { get; set; }
    }
}