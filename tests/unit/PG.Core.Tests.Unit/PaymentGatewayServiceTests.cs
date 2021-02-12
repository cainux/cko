using System;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using PG.Core.Abstractions.AcquiringBank;
using PG.Core.Abstractions.Repositories;
using PG.Core.Entities;
using PG.Core.Requests;
using PG.Core.Services;
using Xunit;

namespace PG.Core.Tests.Unit
{
    public class PaymentGatewayServiceTests
    {
        private readonly AutoMocker _mocker;
        private readonly Mock<IPaymentRepository> _mockRepository;
        private readonly Mock<IAcquiringBankClient> _mockBankClient;

        private readonly Faker<ProcessPaymentRequest> _requestGenerator;

        private readonly PaymentGatewayService SUT;

        public PaymentGatewayServiceTests()
        {
            _mocker = new AutoMocker();
            _mockRepository = _mocker.GetMock<IPaymentRepository>();
            _mockBankClient = _mocker.GetMock<IAcquiringBankClient>();

            _requestGenerator = new Faker<ProcessPaymentRequest>()
                .RuleFor(t => t.PaymentId, _ => Guid.NewGuid().ToString())
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
            var paymentId = Guid.NewGuid().ToString();
            var merchantId = Guid.NewGuid().ToString();

            _mockRepository
                .Setup(x => x.GetAsync(paymentId, merchantId))
                .ReturnsAsync(new Payment
                {
                    Id = paymentId,
                    MerchantId = merchantId
                })
                .Verifiable();

            // Act
            var actual = await SUT.GetAsync(paymentId, merchantId);

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
            var bankResponse = new ProcessPaymentResponse
            {
                BankIdentifier = Guid.NewGuid().ToString(),
                PaymentStatus = PaymentStatus.Failed
            };

            _mockBankClient
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
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
            actual.BankIdentifier.Should().Be(bankResponse.BankIdentifier);
            actual.PaymentStatus.Should().Be(PaymentStatus.Failed);
        }

        [Fact]
        public async Task Process_Payment_Succeeded()
        {
            // Arrange
            var paymentRequest = _requestGenerator.Generate();
            var bankResponse = new ProcessPaymentResponse
            {
                BankIdentifier = Guid.NewGuid().ToString(),
                PaymentStatus = PaymentStatus.Succeeded
            };

            _mockBankClient
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
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
            actual.BankIdentifier.Should().Be(bankResponse.BankIdentifier);
            actual.PaymentStatus.Should().Be(PaymentStatus.Succeeded);
        }

        [Fact]
        public async Task Process_Payment_Errored()
        {
            // Arrange
            var paymentRequest = _requestGenerator.Generate();

            _mockBankClient
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
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
            actual.PaymentStatus.Should().Be(PaymentStatus.Errored);
        }

        [Fact]
        public async Task Credit_Card_Number_Should_Be_Stored_Masked()
        {
            // Arrange
            var paymentRequest = _requestGenerator.Generate();
            paymentRequest.CreditCardNumber = "3494-554249-61247";

            var bankResponse = new ProcessPaymentResponse
            {
                BankIdentifier = Guid.NewGuid().ToString(),
                PaymentStatus = PaymentStatus.Succeeded
            };

            _mockBankClient
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
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
            actual.MaskedCreditCardNumber.Should().Be("*************1247");
        }
    }
}
