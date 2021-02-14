using System.Threading.Tasks;
using PG.Core.Entities;
using PG.Core.Services.Requests;

namespace PG.Core.Services
{
    public interface IPaymentGatewayService
    {
        Task<Payment> GetAsync(long paymentId);
        Task<ProcessPaymentResponse> ProcessAsync(ProcessPaymentRequest request);
    }
}
