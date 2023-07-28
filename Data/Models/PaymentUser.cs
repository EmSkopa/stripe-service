using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StripeApp.Data.Models
{
    public class PaymentUser
    {
        [Key]
        public string PaymentUserId { get; set; }
        public string PaymentProcessor { get; set; } // Don't know reason for this parameter

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
        public ICollection<PaymentUserCard> PaymentUserCard { get; set; }
    }
}
