using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StripeApp.Data.Models
{
    public class PaymentStatus
    {
        [Key]
        public int PaymentStatusId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
