using System;
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

        public async Task<Payment> GetAsync(long paymentId)
        {
            _logger.LogDebug("Fetching Payment from database");

            using var session = _documentStore.LightweightSession();

            var payment = await session.LoadAsync<Payment>(paymentId);

            return payment;
        }

        public async Task<Payment> UpsertAsync(Payment payment)
        {
            _logger.LogDebug("Storing Payment {PaymentId} to database", payment.Id);

            using var session = _documentStore.LightweightSession();

            session.Store(payment);
            await session.SaveChangesAsync();

            return payment;
        }
    }
}
