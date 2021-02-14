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
        private readonly IBankClient _bankClient;
        private readonly ILogger<PaymentGatewayService> _logger;

        public PaymentGatewayService(IPaymentRepository paymentRepository, IBankClient bankClient, ILogger<PaymentGatewayService> logger)
        {
            _paymentRepository = paymentRepository;
            _bankClient = bankClient;
            _logger = logger;
        }

        public async Task<Payment> GetAsync(Guid paymentId)
        {
            _logger.LogInformation("Getting Payment: {Id}", paymentId);
            var payment = await _paymentRepository.GetAsync(paymentId);

            if (payment != null)
            {
                payment.CreditCardNumber = "****";
            }

            return payment;
        }

        public async Task<ProcessPaymentResponse> ProcessAsync(ProcessPaymentRequest request)
        {
            _logger.LogInformation("Processing new Payment");

            var payment = await _paymentRepository.UpsertAsync(new Payment
            {
                MerchantId = request.MerchantId,
                Amount = request.Amount,
                CurrencyCode = request.CurrencyCode,
                CreditCardNumber = request.CreditCardNumber,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Cvv = request.Cvv,
                PaymentStatus = PaymentStatus.Unprocessed
            });

            _logger.LogInformation("Payment created with Id: {PaymentId}, forwarding request to Acquiring Bank", payment.Id);

            try
            {
                var bankResponse = await _bankClient.ProcessPaymentAsync(payment);
                payment.BankIdentifier = bankResponse.BankIdentifier;
                payment.PaymentStatus = bankResponse.PaymentStatus;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Encountered error during bank processing of Payment {PaymentId}", payment.Id);
                payment.PaymentStatus = PaymentStatus.Errored;
            }

            _logger.LogInformation("Saving processed Payment with status of {PaymentStatus}", payment.PaymentStatus);
            await _paymentRepository.UpsertAsync(payment);

            return new ProcessPaymentResponse
            {
                PaymentId = payment.Id,
                PaymentStatus = payment.PaymentStatus
            };
        }
    }
}
