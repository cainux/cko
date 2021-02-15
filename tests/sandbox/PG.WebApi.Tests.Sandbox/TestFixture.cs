using System;
using System.Net.Http;
using System.Text.Json;
using Bogus;
using Microsoft.Extensions.Configuration;
using PG.WebApi.Tests.Sandbox.Requests;

namespace PG.WebApi.Tests.Sandbox
{
    public class TestFixture : IDisposable
    {
        public HttpClient HttpClient { get; }
        public JsonSerializerOptions JsonSerializerOptions { get; }
        public Faker<ProcessPaymentRequest> RequestGenerator { get; }

        public TestFixture()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json", true)
                .AddEnvironmentVariables()
                .Build();

            var paymentGatewayUri = configuration.GetValue<string>("PaymentGatewayUri");

            HttpClient = new HttpClient { BaseAddress = new Uri(paymentGatewayUri) };

            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            RequestGenerator = new Faker<ProcessPaymentRequest>()
                .RuleFor(t => t.MerchantId, _ => Guid.NewGuid().ToString())
                .RuleFor(t => t.CreditCardNumber, f => f.Finance.CreditCardNumber())
                .RuleFor(t => t.ExpiryMonth, f => f.Date.Future().Month)
                .RuleFor(t => t.ExpiryYear, f => f.Date.Future().Year)
                .RuleFor(t => t.Amount, f => f.Finance.Amount())
                .RuleFor(t => t.CurrencyCode, f => f.Finance.Currency().Code)
                .RuleFor(t => t.Cvv, f => f.Finance.CreditCardCvv());
        }

        public void Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}
