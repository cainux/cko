using System.Threading.Tasks;
using PG.Core.Requests;

namespace PG.Core.Abstractions.AcquiringBank
{
    public interface IAcquiringBankClient
    {
        Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
    }
}
