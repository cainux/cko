using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PG.Core.Abstractions.Repositories;
using PG.Core.Entities;
using Xunit;
using Xunit.Abstractions;

namespace PG.WebApi.Tests.Unit.Payments
{
    public class ProcessPaymentTests : PaymentTestsBase, IClassFixture<TestFixture>
    {
        public ProcessPaymentTests(TestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

        [Fact]
        public async Task Process_Returns_BadRequest_When_Request_Invalid()
        {
            // This just gives a rough idea of the tests that need to be created
            // We'll probably want to _really_ test the validation with loads of
            // different variations of the payload.

            // Arrange
            var request = RequestGenerator.Generate();
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
            var request = RequestGenerator.Generate();

            Mocker.GetMock<IPaymentRepository>()
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.PostAsJsonAsync("/payment/process", request);

            // Assert
            Mocker.VerifyAll();
            actual.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task Process_Returns_Created_When_Request_Fails()
        {
            // Arrange
            var request = RequestGenerator.Generate();
            request.MerchantId = "FailMerchant";

            Mocker.GetMock<IPaymentRepository>()
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.PostAsJsonAsync("/payment/process", request);

            // Assert
            Mocker.VerifyAll();
            actual.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Process_Returns_InternalServerError_When_Request_Errors()
        {
            // Arrange
            var request = RequestGenerator.Generate();
            request.MerchantId = "ErrorMerchant";

            Mocker.GetMock<IPaymentRepository>()
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.PostAsJsonAsync("/payment/process", request);

            // Assert
            Mocker.VerifyAll();
            actual.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Process_Returns_InternalServerError_When_Bank_Returns_Invalid_Status()
        {
            // Arrange
            var request = RequestGenerator.Generate();
            request.MerchantId = "EdgeCaseMerchant";

            Mocker.GetMock<IPaymentRepository>()
                .Setup(x => x.UpsertAsync(It.IsAny<Payment>()))
                .ReturnsAsync((Payment p) => p)
                .Verifiable();

            // Act
            var actual = await SUT.PostAsJsonAsync("/payment/process", request);

            // Assert
            Mocker.VerifyAll();
            actual.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
