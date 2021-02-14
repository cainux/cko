using System;
using System.Net.Http;
using System.Text.Json;
using Bogus;
using Moq.AutoMock;
using PG.Core.Requests;
using Xunit.Abstractions;

namespace PG.WebApi.Tests.Unit.Payments
{
    public abstract class PaymentTestsBase : IDisposable
    {
        protected readonly AutoMocker Mocker;
        protected readonly Faker<ProcessPaymentRequest> RequestGenerator;
        protected readonly JsonSerializerOptions JsonSerializerOptions;
        protected readonly HttpClient SUT;

        protected PaymentTestsBase(TestFixture fixture, ITestOutputHelper output)
        {
            fixture.Output = output;

            Mocker = fixture.Mocker;

            RequestGenerator = new Faker<ProcessPaymentRequest>()
                .RuleFor(t => t.MerchantId, _ => Guid.NewGuid().ToString())
                .RuleFor(t => t.CreditCardNumber, f => f.Finance.CreditCardNumber())
                .RuleFor(t => t.ExpiryMonth, f => f.Date.Future().Month)
                .RuleFor(t => t.ExpiryYear, f => f.Date.Future().Year)
                .RuleFor(t => t.Amount, f => f.Finance.Amount())
                .RuleFor(t => t.CurrencyCode, f => f.Finance.Currency().Code)
                .RuleFor(t => t.Cvv, f => f.Finance.CreditCardCvv());

            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            SUT = fixture.WebApplicationFactory.CreateClient();
        }

        public void Dispose()
        {
            SUT?.Dispose();
        }
    }
}
