using System.Threading.Tasks;
using PG.Core.Entities;

namespace PG.Core.Abstractions.AcquiringBank
{
    public interface IBankClient
    {
        Task<BankResponse> ProcessPaymentAsync(Payment request);
    }
}
