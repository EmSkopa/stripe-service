using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StripeApp.Data.Models;

namespace StripeApp.Data.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly StripeContext _stripeContext;

        public PaymentRepository(StripeContext stripeContext)
        {
            _stripeContext = stripeContext;
        }

        public async Task<PaymentUserCard> GetCardInfoByCardIdAsync(string cardId)
        {
            try
            {
                return await _stripeContext.PaymentUserCard
                                    .Include(cc => cc.PaymentUser)
                                        .ThenInclude(c => c.User)
                                    .FirstOrDefaultAsync(c => c.CardId == cardId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve card with id '{cardId}': {ex.Message}");
            }
        }
        public async Task<PaymentUserCard> GetCardByFingerprintIdAsync(string fingerprintId)
        {
            try
            {
                return await _stripeContext.PaymentUserCard
                                    .FirstOrDefaultAsync(c => c.CardFingerprintId == fingerprintId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve card with fingerprintId '{fingerprintId}': {ex.Message}");
            }
        }
        public async Task<User> GetUserPaymentInfoDTOByUserIdAsync(Guid userId)
        {
            try
            {
                return await _stripeContext.User
                            .Include(u => u.PaymentUser)
                                .ThenInclude(pu => pu.PaymentUserCard)
                            .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't retrieve user with id '{userId}': {ex.Message}");
            }
        }

        public async Task<PaymentUser> AddNewPaymentUserAsync(PaymentUser paymentUser)
        {
            if (paymentUser == null)
            {
                throw new ArgumentNullException($"{nameof(AddNewPaymentUserAsync)} user must not be null");
            }

            try
            {
                await _stripeContext.PaymentUser.AddAsync(paymentUser);
                await _stripeContext.SaveChangesAsync();

                return paymentUser;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(paymentUser)} could not be saved: {ex.Message}");
            }
        }

        public async Task<PaymentUserCard> AddNewPaymentCardUserAsync(PaymentUserCard paymentUserCard)
        {
            if (paymentUserCard == null)
            {
                throw new ArgumentNullException($"{nameof(AddNewPaymentCardUserAsync)} card must not be null");
            }

            try
            {
                await _stripeContext.PaymentUserCard.AddAsync(paymentUserCard);
                await _stripeContext.SaveChangesAsync();

                return paymentUserCard;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(paymentUserCard)} could not be saved: {ex.Message}");
            }
        }

        public async Task<bool> DeleteCardByIdAsync(string cardId)
        {
            try
            {
                var card = await _stripeContext.PaymentUserCard.FirstOrDefaultAsync(pc => pc.CardId == cardId);
                if (card == null)
                {
                    throw new ArgumentNullException($"{nameof(DeleteCardByIdAsync)} card with id '{cardId}' can not be null");
                }
                
                _stripeContext.PaymentUserCard.Remove(card);
                await _stripeContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't delete card with id '{cardId}': {ex.Message}");
            }
        }

        public async Task<PaymentUserCard> UpdateDefaultCardByIdAsync(string cardId)
        {
            try
            {
                var card = await _stripeContext.PaymentUserCard.FirstOrDefaultAsync(pc => pc.CardId == cardId);
                if (card == null)
                {
                    throw new ArgumentNullException($"{nameof(UpdateDefaultCardByIdAsync)} card with id '{cardId}' can not be null");
                }
                card.IsDefault = !card.IsDefault;
                _stripeContext.PaymentUserCard.Update(card);
                await _stripeContext.SaveChangesAsync();

                return card;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't delete card with id '{cardId}': {ex.Message}");
            }
        }
        public async Task<Payment> GetTransactionByIdAsync(Guid paymentId)
        {
            try
            {
                var transaction = await _stripeContext.Payment.FirstOrDefaultAsync(pc => pc.PaymentId == paymentId);
                if (transaction == null)
                {
                    throw new ArgumentNullException($"{nameof(UpdateDefaultCardByIdAsync)} transaction with id '{paymentId}' can not be null");
                }

                return transaction;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't delete card with id '{paymentId}': {ex.Message}");
            }
        }

        public async Task<Payment> AddNewTransactionAsync(Payment transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException($"{nameof(AddNewTransactionAsync)} transaction must not be null");
            }

            try
            {
                await _stripeContext.Payment.AddAsync(transaction);
                await _stripeContext.SaveChangesAsync();

                return transaction;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(transaction)} could not be saved: {ex.Message}");
            }
        }
        public async Task<Payment> UpdateTransactionAsync(Payment transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException($"{nameof(UpdateTransactionAsync)} transaction must not be null");
            }

            try
            {
                _stripeContext.Payment.Update(transaction);
                await _stripeContext.SaveChangesAsync();

                return transaction;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(transaction)} could not be updated: {ex.Message}");
            }
        }
        public async Task<ICollection<Payment>> GetAllTransactionByUserId(Guid userId)
        {
            try
            {
                var paymentUser = await _stripeContext.PaymentUser.FirstOrDefaultAsync(pu => pu.UserId == userId);
                if (paymentUser == null)
                {
                    throw new ArgumentNullException($"{nameof(GetAllTransactionByUserId)} user with id '{userId}' can not be null");
                }
                var transactions = await _stripeContext.Payment
                                                    .Where(trx => trx.PaymentUserId == paymentUser.PaymentUserId)
                                                    .ToListAsync();
                await _stripeContext.SaveChangesAsync();

                return transactions;
            }
            catch (Exception ex)
            {
                throw new Exception($"Transactions can not be retrieved: {ex.Message}");
            }
        }
    }
}