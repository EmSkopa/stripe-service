using System;
using System.ComponentModel.DataAnnotations;

namespace StripeApp.Data.Models
{
    public class PaymentEvent
    {
        [Key]
        public Guid EventId { get; set; }
        public string EventMessage { get; set; }
        public string EventStatus { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}