using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PG.Core;
using PG.Core.Services;
using PG.Core.Services.Requests;

namespace PG.WebApi.Controllers
{
    [ApiController]
    [Route("payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentGatewayService _paymentGatewayService;

        public PaymentsController(IPaymentGatewayService paymentGatewayService)
        {
            _paymentGatewayService = paymentGatewayService;
        }

        [HttpGet]
        [Route("{paymentId}")]
        [Produces("application/json")]
        public async Task<IActionResult> Get([FromRoute] int paymentId)
        {
            var payment = await _paymentGatewayService.GetAsync(paymentId);

            if (payment != null)
            {
                return Ok(payment);
            }

            return NotFound();
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] ProcessPaymentRequest request)
        {
            var response = await _paymentGatewayService.ProcessAsync(request);

            return response.StatusCode switch
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
