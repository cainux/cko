using System;
using System.Threading.Tasks;
using PG.Core;
using PG.Core.Abstractions.AcquiringBank;
using PG.Core.Entities;

namespace PG.Adapters.Adapters
{
    public class FakeBankClient : IBankClient
    {
        public async Task<BankResponse> ProcessPaymentAsync(Payment request)
        {
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
