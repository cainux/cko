using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PG.Core;
using PG.Core.Services;
using PG.Core.Services.Requests;

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
        public async Task<IActionResult> Get(string paymentId)
        {
            var payment = await _paymentGatewayService.GetAsync(new Guid(paymentId));

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
            var response = await _paymentGatewayService.ProcessAsync(request);

            return response.PaymentStatus switch
            {
                // These status codes are up for debate
                PaymentStatus.Succeeded => Accepted(response),
                PaymentStatus.Failed => Created(string.Empty, response),
                PaymentStatus.Errored => StatusCode(500, response),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
