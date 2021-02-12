using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using PG.Core.Abstractions.Repositories;
using PG.Core.Entities;
using PG.Core.Requests;
using Xunit;
using Xunit.Abstractions;

namespace PG.WebApi.Tests.Unit
{
    public class PaymentControllerTests : IDisposable
    {
        private readonly AutoMocker _mocker;
        private readonly Faker<ProcessPaymentRequest> _requestGenerator;
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly HttpClient SUT;

        public PaymentControllerTests(ITestOutputHelper output)
        {
            _mocker = new AutoMocker();

            _requestGenerator = new Faker<ProcessPaymentRequest>()
                .RuleFor(t => t.PaymentId, _ => Guid.NewGuid().ToString())
                .RuleFor(t => t.MerchantId, _ => Guid.NewGuid().ToString())
                .RuleFor(t => t.CreditCardNumber, f => f.Finance.CreditCardNumber())
                .RuleFor(t => t.ExpiryMonth, f => f.Date.Future().Month)
                .RuleFor(t => t.ExpiryYear, f => f.Date.Future().Year)
                .RuleFor(t => t.Amount, f => f.Finance.Amount())
                .RuleFor(t => t.CurrencyCode, f => f.Finance.Currency().Code)
                .RuleFor(t => t.Cvv, f => f.Finance.CreditCardCvv());

            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder
                        .ConfigureLogging(logging =>
                        {
                            logging.AddXunit(output);
                        })
                        .ConfigureTestServices(services =>
                        {
                            services.RemoveAll<IPaymentRepository>();
                            services.AddSingleton(_mocker.GetMock<IPaymentRepository>().Object);
                        });
                });

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            SUT = _factory.CreateClient();
        }

        public void Dispose()
        {
            SUT?.Dispose();
            _factory?.Dispose();
        }

        [Fact]
        public async Task Get_Returns_Ok_When_Payment_Exists()
        {
            // Arrange
            var paymentId = Guid.NewGuid().ToString();
            var merchantId = Guid.NewGuid().ToString();

            _mocker.GetMock<IPaymentRepository>()
                .Setup(x => x.GetAsync(paymentId, merchantId))
                .ReturnsAsync(new Payment
                {
                    Id = paymentId,
                    MerchantId = merchantId
                })
                .Verifiable();

            // Act
            var response = await SUT.GetAsync($"/payment?paymentId={paymentId}&merchantId={merchantId}");
            var actual = await JsonSerializer.DeserializeAsync<Payment>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            // Assert
            _mocker.VerifyAll();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            actual.Should().BeEquivalentTo(new
            {
                Id = paymentId,
                MerchantId = merchantId
            });
        }

        [Fact]
        public async Task Get_Returns_NotFound_When_Payment_Doesnt_Exist()
        {
            // Arrange
            var paymentId = Guid.NewGuid().ToString();
            var merchantId = Guid.NewGuid().ToString();

            // Act
            var actual = await SUT.GetAsync($"/payment?paymentId={paymentId}&merchantId={merchantId}");

            // Assert
            actual.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Process_Returns_BadRequest_When_Request_Invalid()
        {
            // This just gives a rough idea of the tests that can be created
            // We'll probably want to _really_ test the validation with loads of
            // different variations of the payload

            // Arrange
            var request = _requestGenerator.Generate();
            request.MerchantId = null;

            // Act
            var actual = await SUT.PostAsJsonAsync("/payment/process", request);

            // Assert
            actual.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Process_Returns_Accepted_When_Request_Succeeds()
        {
            // Arrange
            var request = _requestGenerator.Generate();

            _mocker.GetMock<IPaymentRepository>()
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.PostAsJsonAsync("/payment/process", request);

            // Assert
            _mocker.VerifyAll();
            actual.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task Process_Returns_Created_When_Request_Fails()
        {
            // Arrange
            var request = _requestGenerator.Generate();
            request.MerchantId = "FailMerchant";

            _mocker.GetMock<IPaymentRepository>()
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.PostAsJsonAsync("/payment/process", request);

            // Assert
            _mocker.VerifyAll();
            actual.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Process_Returns_InternalServerError_When_Request_Errors()
        {
            // Arrange
            var request = _requestGenerator.Generate();
            request.MerchantId = "ErrorMerchant";

            _mocker.GetMock<IPaymentRepository>()
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.PostAsJsonAsync("/payment/process", request);

            // Assert
            _mocker.VerifyAll();
            actual.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Process_Returns_InternalServerError_When_Bank_Returns_Invalid_Status()
        {
            // Arrange
            var request = _requestGenerator.Generate();
            request.MerchantId = "EdgeCaseMerchant";

            _mocker.GetMock<IPaymentRepository>()
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.PostAsJsonAsync("/payment/process", request);

            // Assert
            _mocker.VerifyAll();
            actual.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
