using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PG.Core.Abstractions.AcquiringBank;
using PG.Core.Requests;
using PG.Core.Abstractions.Repositories;
using PG.Core.Entities;

namespace PG.Core.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAcquiringBankClient _acquiringBankClient;
        private readonly ILogger<PaymentGatewayService> _logger;

        public PaymentGatewayService(IPaymentRepository paymentRepository, IAcquiringBankClient acquiringBankClient, ILogger<PaymentGatewayService> logger)
        {
            _paymentRepository = paymentRepository;
            _acquiringBankClient = acquiringBankClient;
            _logger = logger;
        }

        public async Task<Payment> GetAsync(string paymentId, string merchantId)
        {
            _logger.LogInformation("Getting Payment: {Id}", paymentId);
            return await _paymentRepository.GetAsync(paymentId, merchantId);
        }

        public async Task<Payment> ProcessAsync(ProcessPaymentRequest paymentRequest)
        {
            _logger.LogInformation("Processing new Payment for Merchant {MerchantId}", paymentRequest.MerchantId);

            var payment = await _paymentRepository.UpsertAsync(new Payment
            {
                Id = paymentRequest.PaymentId,
                MerchantId = paymentRequest.MerchantId,
                Amount = paymentRequest.Amount,
                CurrencyCode = paymentRequest.CurrencyCode,
                MaskedCreditCardNumber = Mask(paymentRequest.CreditCardNumber),
                ExpiryMonth = paymentRequest.ExpiryMonth,
                ExpiryYear = paymentRequest.ExpiryYear,
                Cvv = paymentRequest.Cvv,
                PaymentStatus = PaymentStatus.Ready
            });

            _logger.LogDebug("Payment created, forwarding request to Acquiring Bank");

            try
            {
                var bankResponse = await _acquiringBankClient.ProcessPaymentAsync(paymentRequest);
                payment.BankIdentifier = bankResponse.BankIdentifier;
                payment.PaymentStatus = bankResponse.PaymentStatus;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Encountered error during processing of Payment {PaymentId} for Merchant {MerchantId}", paymentRequest.PaymentId, paymentRequest.MerchantId);
                payment.PaymentStatus = PaymentStatus.Errored;
            }

            _logger.LogDebug("Saving processed Payment with status of {PaymentStatus}", payment.PaymentStatus);
            return await _paymentRepository.UpsertAsync(payment);
        }

        private static string Mask(string input)
        {
            var maskSize = input.Length - 4;
            var partial = input.Substring(maskSize);
            var mask = new string('*', maskSize);
            return $"{mask}{partial}";
        }
    }
}
