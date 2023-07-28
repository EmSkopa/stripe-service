using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StripeApp.Data.Models
{
    public class PaymentUserCard
    {
        [Key]
        public string CardId { get; set; }
        public string CardFingerprintId { get; set; }
        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Guid PaymentUserId { get; set; }
        public PaymentUser PaymentUser { get; set; }
    }
}
