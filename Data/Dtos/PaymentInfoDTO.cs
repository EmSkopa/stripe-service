using System;

namespace StripeApp.Data.Dtos
{
    public class PaymentInfoDTO
    {
        public Guid UserId { get; set; }
        public string CardId { get; set; }
        public string OrderId { get; set; }
        public long Amount { get; set; }
    }
}