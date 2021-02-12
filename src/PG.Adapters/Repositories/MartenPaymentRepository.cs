using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Marten;
using Microsoft.Extensions.Logging;
using PG.Core.Abstractions.Repositories;
using PG.Core.Entities;

namespace PG.Adapters.Repositories
{
    [ExcludeFromCodeCoverage]
    public class MartenPaymentRepository : IPaymentRepository
    {
        private readonly IDocumentStore _documentStore;
        private readonly ILogger<MartenPaymentRepository> _logger;

        public MartenPaymentRepository(IDocumentStore documentStore, ILogger<MartenPaymentRepository> logger)
        {
            _documentStore = documentStore;
            _logger = logger;
        }

        public async Task<Payment> GetAsync(string paymentId, string merchantId)
        {
            _logger.LogDebug("Fetching Payment from database");

            using var session = _documentStore.LightweightSession();

            var queryResult = await session
                .Query<Payment>()
                .SingleOrDefaultAsync(x => x.Id == paymentId && x.MerchantId == merchantId);

            return queryResult;
        }

        public async Task<Payment> UpsertAsync(Payment payment)
        {
            _logger.LogDebug("Storing Payment to database");

            using var session = _documentStore.LightweightSession();

            session.Store(payment);
            await session.SaveChangesAsync();

            return payment;
        }
    }
}
