using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PG.Core;
using PG.Core.Abstractions.AcquiringBank;
using PG.Core.Entities;

namespace PG.Adapters.AcquiringBank
{
    public class FakeBankClient : IBankClient
    {
        private readonly ILogger<FakeBankClient> _logger;

        public FakeBankClient(ILogger<FakeBankClient> logger)
        {
            _logger = logger;
        }

        public async Task<BankResponse> ProcessPaymentAsync(Payment request)
        {
            _logger.LogInformation("Forwarding Request of Payment: {PaymentId} for Merchant: {MerchantId}", request.Id, request.MerchantId);

            if (request.MerchantId == "ErrorMerchant")
            {
                throw new Exception("Simulated error");
            }

            var response = new BankResponse
            {
                BankIdentifier = Guid.NewGuid().ToString(),
                PaymentStatus = request.MerchantId == "FailMerchant" ? PaymentStatus.Failed : PaymentStatus.Succeeded
            };

            if (request.MerchantId == "EdgeCaseMerchant")
            {
                response.PaymentStatus = PaymentStatus.Unprocessed;
            }

            return await Task.Run(() => response);
        }
    }
}
