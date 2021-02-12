using System.Threading.Tasks;
using PG.Core.Entities;
using PG.Core.Requests;

namespace PG.Core.Services
{
    public interface IPaymentGatewayService
    {
        Task<Payment> GetAsync(string paymentId, string merchantId);
        Task<Payment> ProcessAsync(ProcessPaymentRequest paymentRequest);
    }
}
