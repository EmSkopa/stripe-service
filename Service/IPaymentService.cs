using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StripeApp.Data.Dtos;

namespace StripeApp.Service
{
    public interface IPaymentService
    {
        Task<ServiceResponse<CardInfoResponseDTO>> GetCardInfo(string cardId);
        Task<ServiceResponse<CardInfoResponseDTO>> AddNewCardInfo(CardInfoDTO cardInfo);
        Task<ServiceResponse<CardInfoResponseDTO>> UpdateCardInfo(string cardId, CardInfoUpdateDTO cardInfoUpdate);
        Task<ServiceResponse<bool>> DeleteCardInfo(string cardId);
        Task<ServiceResponse<PaymentIntentResponseDTO>> CreatePaymentTransaction(PaymentInfoDTO paymentInfo);
        Task<ServiceResponse<PaymentInfoResponseDTO>> UpdatePaymentTransaction(Guid paymentId, PaymentInfoUpdateDTO paymentInfo);
        Task<ServiceResponse<ICollection<PaymentInfoResponseDTO>>> GetAllTransactionByUserId(Guid userId);
    }
}