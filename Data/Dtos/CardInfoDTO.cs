using System;

namespace StripeApp.Data.Dtos
{
    public class CardInfoDTO
    {
        public string CardNumber { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string Cvc { get; set; }
        public Guid UserId { get; set; }
    }
}