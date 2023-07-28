using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StripeApp.Data.Models;

namespace StripeApp.Data.Repositories
{
    public interface IPaymentRepository
    {
        Task<PaymentUserCard> GetCardInfoByCardIdAsync(string cardId);
        Task<PaymentUserCard> GetCardByFingerprintIdAsync(string fingerprintId);
        Task<User> GetUserPaymentInfoDTOByUserIdAsync(Guid userId);
        Task<PaymentUser> AddNewPaymentUserAsync(PaymentUser paymentUser);
        Task<PaymentUserCard> AddNewPaymentCardUserAsync(PaymentUserCard paymentUserCard);
        Task<bool> DeleteCardByIdAsync(string cardId);
        Task<PaymentUserCard> UpdateDefaultCardByIdAsync(string cardId);
        Task<Payment> GetTransactionByIdAsync(Guid paymentId);
        Task<Payment> AddNewTransactionAsync(Payment transaction);
        Task<Payment> UpdateTransactionAsync(Payment transaction);
        Task<ICollection<Payment>> GetAllTransactionByUserId(Guid userId);
    }
}