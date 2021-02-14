using System.Threading.Tasks;
using PG.Core.Entities;

namespace PG.Core.Abstractions.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> GetAsync(long paymentId);
        Task<Payment> UpsertAsync(Payment payment);
    }
}
