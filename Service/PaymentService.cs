using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StripeApp.Data.Dtos;
using StripeApp.Data.Models;
using StripeApp.Data.Repositories;
using Stripe.Data;
using AutoMapper;
using System.Collections.Generic;
using StripeApp.Stripe;

namespace StripeApp.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly IStripeProcessor _stripeProcessor;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository repository,
            IStripeProcessor stripeProcessor,
            IMapper mapper,
            ILogger<PaymentService> logger)
        {
            _repository = repository;
            _stripeProcessor = stripeProcessor;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ServiceResponse<CardInfoResponseDTO>> GetCardInfo(string cardId)
        {
            try
            {
                var cardInfo = await _repository.GetCardInfoByCardIdAsync(cardId);
                if (cardInfo == null)
                {
                    _logger.LogWarning($"Card with Id={cardId} doesn't exist");
                    return new ServiceResponse<CardInfoResponseDTO>(
                        StatusCodes.Status404NotFound,
                        $"Non-existent card"
                    );
                }

                var extCard = await _stripeProcessor.GetCard(cardId, cardInfo.PaymentUser.PaymentUserId);
                if (extCard == null)
                {
                    _logger.LogWarning($"Card with Id={cardId} doesn't exist as external on account");
                    return new ServiceResponse<CardInfoResponseDTO>(
                        StatusCodes.Status404NotFound,
                        $"Non-existent card on external account"
                    );
                }

                var cardDto = _mapper.Map<CardInfoResponseDTO>(cardInfo);
                cardDto.LastFour = extCard.Last4;
                cardDto.Brand = extCard.Brand;
                cardDto.IsExpired = (DateTime.Now < new DateTime((int)extCard.ExpYear, (int)extCard.ExpMonth, 1)) ? false : true;
                return new ServiceResponse<CardInfoResponseDTO>(cardDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Internal Server Error occurred while getting info of card with Id={cardId}");
                return new ServiceResponse<CardInfoResponseDTO>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error occurred while getting info of card"
                );
            }
        }

        public async Task<ServiceResponse<CardInfoResponseDTO>> AddNewCardInfo(CardInfoDTO cardInfo)
        {
            try
            {
                var currentUser =  await _repository.GetUserPaymentInfoDTOByUserIdAsync(cardInfo.UserId);
                if (currentUser.PaymentUser == null)
                {
                    var newCustomer = await _stripeProcessor.CreateCustomer(
                            currentUser.FirstName,
                            currentUser.LastName,
                            currentUser.Email);

                    var paymentUser = new PaymentUser
                    {
                        PaymentUserId = newCustomer.Id,
                        PaymentProcessor = "",
                        User = currentUser
                    };

                    await _repository.AddNewPaymentUserAsync(paymentUser);

                    var card = await _stripeProcessor.CreateCard(cardInfo, paymentUser.PaymentUserId);
                    var isExistingCard = await _repository.GetCardByFingerprintIdAsync(card.Fingerprint);
                    if(isExistingCard != null)
                    {
                        await _stripeProcessor.DeleteCard(card.Id, currentUser.PaymentUser.PaymentUserId);
                        _logger.LogWarning($"Card with Id={card.Id} already exist on account");
                        return new ServiceResponse<CardInfoResponseDTO>(
                            StatusCodes.Status404NotFound,
                            $"Card is already assigned to another account"
                        );
                    }

                    var paymentCard = new PaymentUserCard
                    {
                        CardId = card.Id,
                        IsDefault = true,
                        CardFingerprintId = card.Fingerprint,
                        PaymentUser = paymentUser
                    };

                    await _repository.AddNewPaymentCardUserAsync(paymentCard);

                    var cardDto = _mapper.Map<CardInfoResponseDTO>(paymentCard);
                    cardDto.LastFour = card.Last4;
                    cardDto.Brand = card.Brand;
                    cardDto.IsExpired = (DateTime.Now < new DateTime((int)card.ExpYear, (int)card.ExpMonth, 1)) ? false : true;
                    return new ServiceResponse<CardInfoResponseDTO>(cardDto);
                }
                else
                {
                    var card = await _stripeProcessor.CreateCard(cardInfo, currentUser.PaymentUser.PaymentUserId);
                    var isExistingCard = await _repository.GetCardByFingerprintIdAsync(card.Fingerprint);
                    if(isExistingCard != null)
                    {
                        await _stripeProcessor.DeleteCard(card.Id, currentUser.PaymentUser.PaymentUserId);
                        _logger.LogWarning($"Card with Id={card.Id} already exist on account");
                        return new ServiceResponse<CardInfoResponseDTO>(
                            StatusCodes.Status404NotFound,
                            $"Card is already assigned to another account"
                        );
                    }
                    
                    var paymentCard = new PaymentUserCard
                    {
                        CardId = card.Id,
                        IsDefault = currentUser.PaymentUser.PaymentUserCard != null ? false : true,
                        CardFingerprintId = card.Fingerprint,
                        PaymentUser = currentUser.PaymentUser
                    };

                    await _repository.AddNewPaymentCardUserAsync(paymentCard);

                    var cardDto = _mapper.Map<CardInfoResponseDTO>(paymentCard);
                    cardDto.LastFour = card.Last4;
                    cardDto.Brand = card.Brand;
                    cardDto.IsExpired = (DateTime.Now < new DateTime((int)card.ExpYear, (int)card.ExpMonth, 1)) ? false : true;
                    return new ServiceResponse<CardInfoResponseDTO>(cardDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Internal Server Error occurred while adding new card");
                return new ServiceResponse<CardInfoResponseDTO>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error occurred while adding new card"
                );
            }
        }

        public async Task<ServiceResponse<bool>> DeleteCardInfo(string cardId)
        {
            try
            {
                var cardInfo = await _repository.GetCardInfoByCardIdAsync(cardId);
                if (cardInfo == null)
                {
                    _logger.LogWarning($"Card with Id={cardId} doesn't exist");
                    return new ServiceResponse<bool>(
                        StatusCodes.Status404NotFound,
                        $"Non-existent card"
                    );
                }
                var status = await _stripeProcessor.DeleteCard(cardId, cardInfo.PaymentUser.PaymentUserId);
                if (status)
                {
                    await _repository.DeleteCardByIdAsync(cardId);

                    var currentUser =  await _repository.GetUserPaymentInfoDTOByUserIdAsync(cardInfo.PaymentUser.User.UserId);
                    var cards = currentUser.PaymentUser.PaymentUserCard
                                        .OrderBy(c => c.CreatedAt)
                                        .ToList();
                    if (cards != null)
                    {
                        await _repository.UpdateDefaultCardByIdAsync(cards[0].CardId);
                    }
                }
                return new ServiceResponse<bool>(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Internal Server Error occurred while deleting card with Id={cardId}");
                return new ServiceResponse<bool>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error occurred while deleting card"
                );
            }
        }

        //TODO: Don't know what exactly we want to update
        public async Task<ServiceResponse<CardInfoResponseDTO>> UpdateCardInfo(string cardId, CardInfoUpdateDTO cardInfoUpdate)
        {
            try
            {
                var cardInfo = await _repository.GetCardInfoByCardIdAsync(cardId);
                if (cardInfo == null)
                {
                    _logger.LogWarning($"Card with Id={cardId} doesn't exist");
                    return new ServiceResponse<CardInfoResponseDTO>(
                        StatusCodes.Status404NotFound,
                        $"Non-existent card"
                    );
                }
                else if (cardInfo.IsDefault && cardInfoUpdate.IsDefault)
                {
                    _logger.LogWarning($"Card with Id={cardId} is already default");
                    return new ServiceResponse<CardInfoResponseDTO>(
                        StatusCodes.Status400BadRequest,
                        $"Card with Id={cardId} is already default"
                    );
                }
                else if (!cardInfo.IsDefault && !cardInfoUpdate.IsDefault)
                {
                    _logger.LogWarning($"Card with Id={cardId} is already not default");
                    return new ServiceResponse<CardInfoResponseDTO>(
                        StatusCodes.Status400BadRequest,
                        $"Card with Id={cardId} is already not default"
                    );
                }
                var currentUser =  await _repository.GetUserPaymentInfoDTOByUserIdAsync(cardInfo.PaymentUser.User.UserId);
                var cards = currentUser.PaymentUser.PaymentUserCard.ToList();
                for (int i = 0; i < cards.Count; i++)
                {
                    if(cardInfoUpdate.IsDefault)
                    {
                        if (cards[i].CardId == cardId)
                        {
                            await _stripeProcessor.UpdateCard(cards[i].CardId, currentUser.PaymentUser.PaymentUserId);
                            await _repository.UpdateDefaultCardByIdAsync(cards[i].CardId);
                        }
                        else if(cards[i].IsDefault)
                        {
                            await _repository.UpdateDefaultCardByIdAsync(cards[i].CardId);
                        }
                    }
                    else
                    {
                        if (cards[i].CardId == cardId)
                        {
                            await _repository.UpdateDefaultCardByIdAsync(cards[i].CardId);
                        }
                        else if(i == 0 && cards[i].CardId != cardId ||
                                i == 1 && cards[i - 1].CardId == cardId)
                        {
                            await _stripeProcessor.UpdateCard(cards[i].CardId, currentUser.PaymentUser.PaymentUserId);
                            await _repository.UpdateDefaultCardByIdAsync(cards[i].CardId);
                        }
                    }
                }

                var cardDto = _mapper.Map<CardInfoResponseDTO>(cardInfo);
                return new ServiceResponse<CardInfoResponseDTO>(cardDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Internal Server Error occurred while updating info of card with Id={cardId}");
                return new ServiceResponse<CardInfoResponseDTO>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error occurred while updating info of card"
                );
            }
        }

        public async Task<ServiceResponse<PaymentIntentResponseDTO>> CreatePaymentTransaction(PaymentInfoDTO paymentInfo)
        {
            try
            {
                var currentUser =  await _repository.GetUserPaymentInfoDTOByUserIdAsync(paymentInfo.UserId);
                if (currentUser.PaymentUser.PaymentUserCard == null || 
                    currentUser.PaymentUser.PaymentUserCard.FirstOrDefault(c => c.CardId == paymentInfo.CardId) == null)
                {
                    _logger.LogWarning($"User with Id={paymentInfo.UserId} doesn't have a card");
                    return new ServiceResponse<PaymentIntentResponseDTO>(
                        StatusCodes.Status404NotFound,
                        $"Non-existent card"
                    );
                }
                var paymentIntent = await _stripeProcessor.CreateNewTransactionIntent(
                                                            currentUser.PaymentUser.PaymentUserId,
                                                            paymentInfo.CardId,
                                                            paymentInfo.OrderId,
                                                            paymentInfo.Amount);
                var transaction = new Payment
                {
                    PaymentIntentId = paymentIntent.Id,
                    PaymentUserId = paymentIntent.CustomerId,
                    Amount = paymentInfo.Amount,
                    User = currentUser,
                    PaymentUserCard = currentUser.PaymentUser.PaymentUserCard.FirstOrDefault(uc => uc.CardId == paymentInfo.CardId)
                };

                if (paymentIntent.Status == "requires_action" &&
                    paymentIntent.NextAction.Type == "use_stripe_sdk")
                {
                    transaction.PaymentStatusId = (int)EPaymentStatus.Pending;
                    await _repository.AddNewTransactionAsync(transaction);

                    var response = new PaymentIntentResponseDTO
                    {
                        Success = false,
                        RequiresAction = true,
                        PaymentIntentClientSecret = paymentIntent.ClientSecret,
                        TransactionId = transaction.PaymentId
                    };

                    return new ServiceResponse<PaymentIntentResponseDTO>(response);
                }
                else if (paymentIntent.Status == "succeeded")
                {
                    transaction.PaymentStatusId = (int)EPaymentStatus.Approved;
                    await _repository.AddNewTransactionAsync(transaction);

                    var response = new PaymentIntentResponseDTO
                    {
                        Success = true,
                        RequiresAction = false,
                        PaymentIntentClientSecret = "",
                        TransactionId = transaction.PaymentId
                    };

                    return new ServiceResponse<PaymentIntentResponseDTO>(response);
                }
                else
                {
                    transaction.PaymentStatusId = (int)EPaymentStatus.Declined;
                    await _repository.AddNewTransactionAsync(transaction);

                    _logger.LogError($"Invalid PaymentIntent status");
                    return new ServiceResponse<PaymentIntentResponseDTO>(
                        StatusCodes.Status500InternalServerError,
                        "Invalid PaymentIntent status"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Internal Server Error occurred while trying to create a new transaction");
                return new ServiceResponse<PaymentIntentResponseDTO>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error occurred while trying to create a new transaction"
                );
            }
        }

        public async Task<ServiceResponse<PaymentInfoResponseDTO>> UpdatePaymentTransaction(Guid paymentId, PaymentInfoUpdateDTO paymentInfo)
        {
            try
            {
                var transaction = await _repository.GetTransactionByIdAsync(paymentId);
                if (transaction == null)
                {
                    _logger.LogWarning($"Transaction with Id={paymentId} doesn't exist");
                    return new ServiceResponse<PaymentInfoResponseDTO>(
                        StatusCodes.Status404NotFound,
                        $"Transaction with Id={paymentId} doesn't exist"
                    );
                }

                if (paymentInfo.IsRefund == null || paymentInfo.IsRefund == false)
                {
                    await _stripeProcessor.ConfirmTranscationIntent(paymentInfo.PaymentIntentId);
                    transaction.PaymentStatusId = (int)EPaymentStatus.Approved;
                }
                else
                {
                    await _stripeProcessor.RefundTransactionIntent(paymentInfo.PaymentIntentId);
                    transaction.PaymentStatusId = (int)EPaymentStatus.Refunded;
                }

                await _repository.UpdateTransactionAsync(transaction);

                var transactionDto = _mapper.Map<PaymentInfoResponseDTO>(transaction);
                return new ServiceResponse<PaymentInfoResponseDTO>(transactionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Internal Server Error occurred while trying to update transaction");
                return new ServiceResponse<PaymentInfoResponseDTO>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error occurred while trying to update transaction"
                );
            }
        }

        public async Task<ServiceResponse<ICollection<PaymentInfoResponseDTO>>> GetAllTransactionByUserId(Guid userId)
        {
            try
            {
                var transactions = await _repository.GetAllTransactionByUserId(userId);
                if (transactions == null)
                {
                    _logger.LogWarning($"There is no transactions for user with id {userId}");
                    return new ServiceResponse<ICollection<PaymentInfoResponseDTO>>(
                        StatusCodes.Status404NotFound,
                        $"There is no transactions for user with id {userId}"
                    );
                }

                ICollection<PaymentInfoResponseDTO> transactionsDto =  new List<PaymentInfoResponseDTO>();
                foreach (var trx in transactions)
                {
                    transactionsDto.Add(_mapper.Map<PaymentInfoResponseDTO>(trx));
                }
                return new ServiceResponse<ICollection<PaymentInfoResponseDTO>>(transactionsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Internal Server Error occurred while trying to get all transactions for user with id {userId}");
                return new ServiceResponse<ICollection<PaymentInfoResponseDTO>>(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error occurred while trying to get all transactions"
                );
            }
        }
    }
}