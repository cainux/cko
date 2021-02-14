using System;
using System.Threading.Tasks;
using PG.Core.Entities;
using PG.Core.Requests;

namespace PG.Core.Services
{
    public interface IPaymentGatewayService
    {
        Task<Payment> GetAsync(Guid paymentId);
        Task<ProcessPaymentResponse> ProcessAsync(ProcessPaymentRequest request);
    }
}
