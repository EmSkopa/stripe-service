using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StripeApp.Data.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public PaymentUser PaymentUser { get; set; }
    }
}
