using System;

namespace StripeApp.Data.Dtos
{
    public class CardInfoResponseDTO
    {
        public string CardId { get; set; }
        public string LastFour { get; set; }
        public string Brand { get; set; }
        public bool IsDefault { get; set; }
        public bool IsExpired { get; set; }
    }
}