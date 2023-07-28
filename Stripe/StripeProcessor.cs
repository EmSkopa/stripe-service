using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using StripeApp.Data.Dtos;
using StripeApp.Data.Models;

namespace StripeApp.Stripe
{
    public class StripeProcessor : IStripeProcessor
    {
        private readonly StripeParameters _stripeParameters;

        public StripeProcessor(StripeParameters stripeParameters)
        {
            _stripeParameters = stripeParameters;
        }
        public async Task<Customer> CreateCustomer(string firstName, string lastName, string email)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeParameters.SecretKey;
                var options = new CustomerCreateOptions
                {
                    Name = firstName + " " + lastName,
                    Email = email
                };
                var service = new CustomerService();
                return await service.CreateAsync(options);
            }
            catch (Exception ex)
            {
                throw new Exception($"Customer could not be created: {ex.Message}");
            }
        }

        public async Task<Card> CreateCard(CardInfoDTO cardInfo, string paymentUserId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeParameters.SecretKey;
                // Get token for the card
                var tokenOptions = new TokenCreateOptions
                {
                    Card = new TokenCardOptions
                    {
                        Number = cardInfo.CardNumber,
                        ExpMonth = cardInfo.ExpMonth,
                        ExpYear = cardInfo.ExpYear,
                        Cvc = cardInfo.Cvc
                    },
                };
                var tokenService = new TokenService();
                var token = await tokenService.CreateAsync(tokenOptions);

                // Use the token to create a card
                var cardOptions = new CardCreateOptions
                {
                    Source = token.Id,
                };
                var cardService = new CardService();
                return await cardService.CreateAsync(paymentUserId, cardOptions);
            }
            catch (Exception ex)
            {
                throw new Exception($"Card could not be created: {ex.Message}");
            }
        }

        public async Task<bool> DeleteCard(string paymentUserCardId, string paymentUserId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeParameters.SecretKey;
                var service = new CardService();
                var deletedCard = await service.DeleteAsync(paymentUserId, paymentUserCardId);
                return deletedCard.Deleted == null ? false : (bool)deletedCard.Deleted;
            }
            catch (Exception ex)
            {
                throw new Exception($"Card could not be deleted: {ex.Message}");
            }
        }

        public async Task<bool> UpdateCard(string paymentUserCardId, string paymentUserId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeParameters.SecretKey;
                var options = new CustomerUpdateOptions
                {
                    DefaultSource = paymentUserCardId
                };

                var service = new CustomerService();
                await service.UpdateAsync(paymentUserId, options);
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Card could not be deleted: {ex.Message}");
            }
        }
        
        public async Task<Card> GetCard(string paymentUserCardId, string paymentUserId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeParameters.SecretKey;
                var cardService = new CardService();
                return await cardService.GetAsync(paymentUserId, paymentUserCardId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Card could not be retrieved: {ex.Message}");
            }
        }

        public async Task<PaymentIntent> CreateNewTransactionIntent(
            string paymentUserId, 
            string cardId,
            string orderId, 
            double amount)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeParameters.SecretKey;
                var paymentIntentService = new PaymentIntentService();
                var amountLong = (long)Math.Round(amount, 2) * 100;

                var createOptions = new PaymentIntentCreateOptions
                {
                    Customer = paymentUserId,
                    PaymentMethod = cardId,
                    Amount = amountLong,
                    Currency = "usd",
                    Confirm = true,
                    Metadata = new Dictionary<string, string>
                    {
                        { "OrderId", orderId },
                    }
                };
                var paymentIntent = await paymentIntentService.CreateAsync(createOptions);
                
                return paymentIntent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Transaction could not be posted: {ex.Message}");
            }
        }

        public async Task<PaymentIntent> ConfirmTranscationIntent(string paymentIntentId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeParameters.SecretKey;
                var paymentIntentService = new PaymentIntentService();

                var confirmOptions = new PaymentIntentConfirmOptions{};

                var paymentIntent = await paymentIntentService.ConfirmAsync(
                    paymentIntentId,
                    confirmOptions);
                
                return paymentIntent;
            }
            catch (Exception ex)
            {
                throw new Exception($"Transaction could not be confirmed: {ex.Message}");
            }
        }

        public async Task<Refund> RefundTransactionIntent(string paymentIntentId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeParameters.SecretKey;
                var refundService = new RefundService();

                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId
                };

                var refund = await refundService.CreateAsync(options);
                
                return refund;
            }
            catch (Exception ex)
            {
                throw new Exception($"Transaction could not be refunded: {ex.Message}");
            }
        }
    }
}