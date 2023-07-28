using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using StripeApp.Data.Dtos;
using StripeApp.Service;

namespace StripeApp.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("card/{cardId}")]
        public async Task<ActionResult> GetCardInfo([FromRoute]string cardId)
        {
            var serviceResponse = await _paymentService.GetCardInfo(cardId);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            var responseDtos = serviceResponse.Data;
            return Ok(responseDtos);
        }

        [HttpPost("card")]
        public async Task<ActionResult> AddNewCardInfo([FromBody]CardInfoDTO cardInfo)
        {
            var serviceResponse = await _paymentService.AddNewCardInfo(cardInfo);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            var responseDtos = serviceResponse.Data;
            return CreatedAtAction(nameof(GetCardInfo), new { cardId = responseDtos.CardId}, responseDtos);
        }

        [HttpPut("card/{cardId}")]
        public async Task<ActionResult> UpdateCardInfo([FromRoute]string cardId, [FromBody]CardInfoUpdateDTO cardInfo)
        {
            var serviceResponse = await _paymentService.UpdateCardInfo(cardId, cardInfo);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            var responseDtos = serviceResponse.Data;
            return CreatedAtAction(nameof(GetCardInfo), new { cardId = responseDtos.CardId}, responseDtos);
        }

        [HttpDelete("card/{cardId}")]
        public async Task<ActionResult> DeleteCardInfo([FromRoute]string cardId)
        {
            var serviceResponse = await _paymentService.DeleteCardInfo(cardId);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            return NoContent();
        }

        [HttpPost("pay")]
        public async Task<ActionResult> CreatePaymentTransaction([FromBody]PaymentInfoDTO paymentInfo)
        {
            var serviceResponse = await _paymentService.CreatePaymentTransaction(paymentInfo);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            var responseDtos = serviceResponse.Data;
            return Ok(responseDtos);
        }

        [HttpPut("pay/{paymentId}")]
        public async Task<ActionResult> UpdatePaymentTransaction(
                [FromRoute]Guid paymentId, 
                [FromBody]PaymentInfoUpdateDTO paymentInfo)
        {
            var serviceResponse = await _paymentService.UpdatePaymentTransaction(paymentId, paymentInfo);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            return NoContent();
        }

        [HttpGet("pay/{userId}")]
        public async Task<ActionResult> GetAllTransactionByUserId([FromRoute]Guid userId)
        {
            var serviceResponse = await _paymentService.GetAllTransactionByUserId(userId);
            
            if (!serviceResponse.Success)
            {
                return StatusCode(serviceResponse.StatusCode, serviceResponse.Message);
            }

            var responseDtos = serviceResponse.Data;
            return Ok(responseDtos);
        }
    }
}