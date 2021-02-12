using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PG.Core;
using PG.Core.Requests;
using PG.Core.Services;

namespace PG.WebApi.Controllers
{
    [ApiController]
    [Route("payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentGatewayService _paymentGatewayService;

        public PaymentController(IPaymentGatewayService paymentGatewayService)
        {
            _paymentGatewayService = paymentGatewayService;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string paymentId, string merchantId)
        {
            var payment = await _paymentGatewayService.GetAsync(paymentId, merchantId);

            if (payment != null)
            {
                return Ok(payment);
            }

            return NotFound();
        }

        [HttpPost, Route("process")]
        [Consumes("application/json"), Produces("application/json")]
        public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request)
        {
            var payment = await _paymentGatewayService.ProcessAsync(request);

            return payment.PaymentStatus switch
            {
                // These status codes are up for debate
                PaymentStatus.Succeeded => Accepted(payment),
                PaymentStatus.Failed => Created(string.Empty, payment),
                PaymentStatus.Errored => StatusCode(500, payment),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
