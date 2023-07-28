using System;
using System.ComponentModel.DataAnnotations;

namespace StripeApp.Data.Models
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }
        public string PaymentIntentId { get; set; }
        public string PaymentUserId { get; set; }
        public double Amount { get; set; }
        public double? TaxedAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentProcessorId { get; set; } // dont know what is this
        public Guid? RouteBookingId { get; set; }
        public int PaymentStatusId { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string CardId { get; set; }
        public PaymentUserCard PaymentUserCard { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

    }
}