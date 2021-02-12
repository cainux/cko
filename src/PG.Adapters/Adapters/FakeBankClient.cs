using System;
using System.Threading.Tasks;
using PG.Core;
using PG.Core.Abstractions.AcquiringBank;
using PG.Core.Requests;

namespace PG.Adapters.Adapters
{
    public class FakeBankClient : IAcquiringBankClient
    {
        public async Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            if (request.MerchantId == "ErrorMerchant")
            {
                throw new Exception("Simulated error");
            }

            var response = new ProcessPaymentResponse
            {
                BankIdentifier = Guid.NewGuid().ToString(),
                PaymentStatus = request.MerchantId == "FailMerchant" ? PaymentStatus.Failed : PaymentStatus.Succeeded
            };

            if (request.MerchantId == "EdgeCaseMerchant")
            {
                response.PaymentStatus = PaymentStatus.Ready;
            }

            return await Task.Run(() => response);
        }
    }
}
