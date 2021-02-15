using System;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using PG.Core.Abstractions.AcquiringBank;
using PG.Core.Abstractions.Repositories;
using PG.Core.Entities;
using PG.Core.Services;
using PG.Core.Services.Requests;
using Xunit;

namespace PG.Core.Tests.Unit
{
    public class PaymentGatewayServiceTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IPaymentRepository> _mockRepository;
        private readonly Mock<IBankClient> _mockBankClient;

        private readonly Faker<ProcessPaymentRequest> _requestGenerator;

        private readonly PaymentGatewayService SUT;

        public PaymentGatewayServiceTests()
        {
            _mocker = new AutoMocker();
            _mockRepository = _mocker.GetMock<IPaymentRepository>();
            _mockBankClient = _mocker.GetMock<IBankClient>();

            _requestGenerator = new Faker<ProcessPaymentRequest>()
                .RuleFor(t => t.MerchantId, _ => Guid.NewGuid().ToString())
                .RuleFor(t => t.CreditCardNumber, f => f.Finance.CreditCardNumber())
                .RuleFor(t => t.ExpiryMonth, f => f.Date.Future().Month)
                .RuleFor(t => t.ExpiryYear, f => f.Date.Future().Year)
                .RuleFor(t => t.Amount, f => f.Finance.Amount())
                .RuleFor(t => t.CurrencyCode, f => f.Finance.Currency().Code)
                .RuleFor(t => t.Cvv, f => f.Finance.CreditCardCvv());

            SUT = _mocker.CreateInstance<PaymentGatewayService>();
        }

        [Fact]
        public async Task Get_Payment()
        {
            // Arrange
            var paymentId = 1000;
            var merchantId = Guid.NewGuid().ToString();

            _mockRepository
                .Setup(x => x.GetAsync(paymentId))
                .ReturnsAsync(new Payment
                {
                    Id = paymentId,
                    MerchantId = merchantId
                })
                .Verifiable();

            // Act
            var actual = await SUT.GetAsync(paymentId);

            // Assert
            _mocker.VerifyAll();
            actual.Id.Should().Be(paymentId);
            actual.MerchantId.Should().Be(merchantId);
        }

        [Fact]
        public async Task Process_Payment_Failed()
        {
            // Arrange
            var paymentRequest = _requestGenerator.Generate();
            var bankResponse = new BankResponse
            {
                PaymentStatus = PaymentStatus.Failed
            };

            _mockBankClient
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(bankResponse)
                .Verifiable();

            _mockRepository
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.ProcessAsync(paymentRequest);

            // Assert
            _mocker.VerifyAll();
            actual.StatusCode.Should().Be(PaymentStatus.Failed);
            actual.StatusText.Should().Be("Failed");
        }

        [Fact]
        public async Task Process_Payment_Succeeded()
        {
            // Arrange
            var paymentRequest = _requestGenerator.Generate();
            var bankResponse = new BankResponse
            {
                PaymentStatus = PaymentStatus.Succeeded
            };

            _mockBankClient
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<Payment>()))
                .ReturnsAsync(bankResponse)
                .Verifiable();

            _mockRepository
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.ProcessAsync(paymentRequest);

            // Assert
            _mocker.VerifyAll();
            actual.StatusCode.Should().Be(PaymentStatus.Succeeded);
            actual.StatusText.Should().Be("Succeeded");
        }

        [Fact]
        public async Task Process_Payment_Errored()
        {
            // Arrange
            var paymentRequest = _requestGenerator.Generate();

            _mockBankClient
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<Payment>()))
                .Throws(new Exception())
                .Verifiable();

            _mockRepository
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.ProcessAsync(paymentRequest);

            // Assert
            _mocker.VerifyAll();
            actual.StatusCode.Should().Be(PaymentStatus.Errored);
            actual.StatusText.Should().Be("Errored");
        }

        [Fact]
        public async Task Credit_Card_Number_Should_Be_Masked_When_Fetched()
        {
            // Arrange
            var paymentId = 1000;

            _mockRepository
                .Setup(x => x.GetAsync(paymentId))
                .ReturnsAsync(new Payment
                {
                    CreditCardNumber = "3494-554249-61247"
                })
                .Verifiable();

            // Act
            var actual = await SUT.GetAsync(paymentId);

            // Assert
            _mocker.VerifyAll();
            actual.CreditCardNumber.Should().Be("****");
        }
    }
}
