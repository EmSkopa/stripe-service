using AutoMapper;
using StripeApp.Data.Dtos;
using StripeApp.Data.Models;

namespace StripeApp.Data.Profiles
{
    public class StripeProfiles : Profile
    {
        public StripeProfiles()
        {
            CreateMap<PaymentUserCard, CardInfoResponseDTO>();
            CreateMap<Payment, PaymentInfoResponseDTO>();
        }
    }
}