using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PG.Core.Abstractions.Repositories;
using PG.Core.Entities;
using Xunit;
using Xunit.Abstractions;

namespace PG.WebApi.Tests.Unit.Payments
{
    public class GetPaymentTests : PaymentTestsBase, IClassFixture<TestFixture>
    {
        public GetPaymentTests(TestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

        [Fact]
        public async Task Returns_Ok_When_Payment_Exists()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var merchantId = Guid.NewGuid().ToString();

            Mocker.GetMock<IPaymentRepository>()
                .Setup(x => x.GetAsync(paymentId))
                .ReturnsAsync(new Payment
                {
                    Id = paymentId,
                    MerchantId = merchantId,
                    CreditCardNumber = "3494-554249-61247"
                })
                .Verifiable();

            // Act
            var response = await SUT.GetAsync($"/payment?paymentId={paymentId}");
            var actual = await JsonSerializer.DeserializeAsync<Payment>(await response.Content.ReadAsStreamAsync(), JsonSerializerOptions);

            // Assert
            Mocker.VerifyAll();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            actual.Should().BeEquivalentTo(new
            {
                Id = paymentId,
                MerchantId = merchantId,
                CreditCardNumber = "****"
            });
        }

        [Fact]
        public async Task Returns_NotFound_When_Payment_Doesnt_Exist()
        {
            // Arrange
            var paymentId = Guid.NewGuid().ToString();

            // Act
            var actual = await SUT.GetAsync($"/payment?paymentId={paymentId}");

            // Assert
            actual.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
