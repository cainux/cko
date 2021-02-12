using System;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Refit;
using Xunit;

namespace PG.WebApi.Tests.Sandbox
{
    public class ProcessPaymentRequest
    {
        public string PaymentId { get; set; }
        public string MerchantId { get; set; }
        public string CreditCardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string Cvv { get; set; }
    }

    public class Payment
    {
        public string Id { get; set; }
        public string MerchantId { get; set; }

        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }

        public string MaskedCreditCardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Cvv { get; set; }

        public string BankIdentifier { get; set; }
        public int PaymentStatus { get; set; }
    }

    public interface IPaymentGatewayApi
    {
        [Get("/payment?paymentId={paymentId}&merchantId={merchantId}")]
        public Task<Payment> Get(string paymentId, string merchantId);

        [Post("/payment")]
        public Task<Payment> Process([Body] ProcessPaymentRequest payment);
    }

    public class UnitTest1
    {
        private readonly Faker<ProcessPaymentRequest> _requestGenerator;

        private readonly IPaymentGatewayApi SUT;

        public UnitTest1()
        {
            SUT = RestService.For<IPaymentGatewayApi>("http://localhost:5000");

            _requestGenerator = new Faker<ProcessPaymentRequest>()
                .RuleFor(t => t.PaymentId, _ => Guid.NewGuid().ToString())
                .RuleFor(t => t.MerchantId, _ => Guid.NewGuid().ToString())
                .RuleFor(t => t.CreditCardNumber, f => f.Finance.CreditCardNumber())
                .RuleFor(t => t.ExpiryMonth, f => f.Date.Future().Month)
                .RuleFor(t => t.ExpiryYear, f => f.Date.Future().Year)
                .RuleFor(t => t.Amount, f => f.Finance.Amount())
                .RuleFor(t => t.CurrencyCode, f => f.Finance.Currency().Code)
                .RuleFor(t => t.Cvv, f => f.Finance.CreditCardCvv());
        }

        [Fact]
        public async Task Make_Payment()
        {
            var request = _requestGenerator.Generate();

            var actual = await SUT.Process(request);

            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task Test1()
        {
            var actual = await SUT.Get("foo", "bar");

            actual.Should().NotBeNull();
        }
    }
}
