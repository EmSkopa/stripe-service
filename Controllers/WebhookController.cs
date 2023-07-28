using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using StripeApp.Data;
using StripeApp.Data.Models;
using StripeApp.Stripe;

namespace StripeApp.Controllers
{
    [Route("api/webhook")]
    public class WebhookController : Controller
    {
        private readonly StripeParameters _stripeParameters;
        private readonly StripeContext _context;

        public WebhookController(StripeParameters stripeParameters, StripeContext stripeContext)
        {
            _stripeParameters = stripeParameters;
            _context = stripeContext;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                        json, 
                        Request.Headers["Stripe-Signature"], 
                        _stripeParameters.WebhookKey);
                // Handle the event
                if (stripeEvent.Type == Events.AccountUpdated)
                {
                    Console.WriteLine("Account was successfully updated!");

                    await this.SaveChangesToDb(json, Events.AccountUpdated);
                }
                else if (stripeEvent.Type == Events.AccountApplicationAuthorized)
                {
                    Console.WriteLine("Application was authorized!");

                    await this.SaveChangesToDb(json, Events.AccountApplicationAuthorized);
                }
                else if (stripeEvent.Type == Events.ChargeCaptured)
                {
                    Console.WriteLine("Charge was captured!");

                    await this.SaveChangesToDb(json, Events.ChargeCaptured);
                }
                else if (stripeEvent.Type == Events.ChargeExpired)
                {
                    Console.WriteLine("Charge was expired!");

                    await this.SaveChangesToDb(json, Events.ChargeExpired);
                }
                else if (stripeEvent.Type == Events.ChargeFailed)
                {
                    Console.WriteLine("Charge was failed!");

                    await this.SaveChangesToDb(json, Events.ChargeFailed);
                }
                else if (stripeEvent.Type == Events.ChargePending)
                {
                    Console.WriteLine("Charge was pending!");

                    await this.SaveChangesToDb(json, Events.ChargePending);
                }
                else if (stripeEvent.Type == Events.ChargeRefunded)
                {
                    Console.WriteLine("Charge was refunded!");

                    await this.SaveChangesToDb(json, Events.ChargeRefunded);
                }
                else if (stripeEvent.Type == Events.ChargeSucceeded)
                {
                    Console.WriteLine("Charge was succeeded!");

                    await this.SaveChangesToDb(json, Events.ChargeSucceeded);
                }
                else if (stripeEvent.Type == Events.ChargeSucceeded)
                {
                    Console.WriteLine("Charge was succeeded!");

                    await this.SaveChangesToDb(json, Events.ChargeSucceeded);
                }
                else if (stripeEvent.Type == Events.ChargeUpdated)
                {
                    Console.WriteLine("Charge was updated!");

                    await this.SaveChangesToDb(json, Events.ChargeUpdated);
                }
                else if (stripeEvent.Type == Events.PaymentIntentAmountCapturableUpdated)
                {
                    Console.WriteLine("PaymentIntent amount capture was updated!");

                    await this.SaveChangesToDb(json, Events.PaymentIntentAmountCapturableUpdated);
                }
                else if (stripeEvent.Type == Events.PaymentIntentCanceled)
                {
                    Console.WriteLine("PaymentIntent was canceled!");

                    await this.SaveChangesToDb(json, Events.PaymentIntentCanceled);
                }
                else if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    Console.WriteLine("PaymentIntent was successfull!");

                    await this.SaveChangesToDb(json, Events.PaymentIntentSucceeded);
                }
                else if (stripeEvent.Type == Events.PaymentMethodAttached)
                {
                    Console.WriteLine("PaymentMethod was attached to a Customer!");

                    await this.SaveChangesToDb(json, Events.PaymentIntentSucceeded);
                }
                else if (stripeEvent.Type == Events.PaymentMethodAttached)
                {
                    Console.WriteLine("PaymentMethod was attached to a Customer!");

                    await this.SaveChangesToDb(json, Events.PaymentIntentSucceeded);
                }
                // rest of the wanted events
                else if (
                    stripeEvent.Type == Events.PaymentIntentRequiresAction ||
                    stripeEvent.Type == Events.PaymentIntentProcessing ||
                    stripeEvent.Type == Events.PaymentMethodAttached ||
                    stripeEvent.Type == Events.PaymentMethodAutomaticallyUpdated ||
                    stripeEvent.Type == Events.SourceCanceled ||
                    stripeEvent.Type == Events.SourceChargeable ||
                    stripeEvent.Type == Events.ChargeDisputeClosed ||
                    stripeEvent.Type == Events.ChargeDisputeCreated ||
                    stripeEvent.Type == Events.ChargeDisputeUpdated ||
                    stripeEvent.Type == Events.ChargeRefundUpdated ||
                    stripeEvent.Type == Events.CustomerCreated ||
                    stripeEvent.Type == Events.CustomerDeleted ||
                    stripeEvent.Type == Events.CustomerUpdated ||
                    stripeEvent.Type == Events.CustomerSourceCreated ||
                    stripeEvent.Type == Events.CustomerSourceDeleted ||
                    stripeEvent.Type == Events.CustomerSourceExpiring ||
                    stripeEvent.Type == Events.CustomerSourceUpdated ||
                    stripeEvent.Type == Events.OrderCreated ||
                    stripeEvent.Type == Events.OrderPaymentFailed ||
                    stripeEvent.Type == Events.OrderPaymentSucceeded ||
                    stripeEvent.Type == Events.OrderUpdated ||
                    stripeEvent.Type == Events.OrderReturnCreated
                    )
                {
                    Console.WriteLine("Other handled event type: {0}", stripeEvent.Type);
                    await this.SaveChangesToDb(json, stripeEvent.Type);
                }
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }
                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }

        private async Task<IActionResult> SaveChangesToDb(string json, string eventStatus)
        {
            var eventObject = new PaymentEvent
            {
                EventMessage = json,
                EventStatus = eventStatus
            };
            
            await _context.PaymentEvent.AddAsync(eventObject);
            await _context.SaveChangesAsync();

            return null;
        }
    }
}