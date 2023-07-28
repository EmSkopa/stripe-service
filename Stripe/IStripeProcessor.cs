using System.Threading.Tasks;
using Stripe;
using StripeApp.Data.Dtos;
using StripeApp.Data.Models;

namespace StripeApp.Stripe
{
    public interface IStripeProcessor
    {
        Task<Customer> CreateCustomer(string firstName, string lastName, string email);
        Task<Card> CreateCard(CardInfoDTO cardInfo, string paymentUserId);
        Task<bool> DeleteCard(string paymentUserCardId, string paymentUserId);
        Task<bool> UpdateCard(string paymentUserCardId, string paymentUserId);
        Task<Card> GetCard(string paymentUserCardId, string paymentUserId);
        Task<PaymentIntent> CreateNewTransactionIntent(
            string paymentUserId, 
            string cardId,
            string orderId, 
            double amount);
        Task<PaymentIntent> ConfirmTranscationIntent(string paymentIntentId);
        Task<Refund> RefundTransactionIntent(string paymentIntentId);
    }
}