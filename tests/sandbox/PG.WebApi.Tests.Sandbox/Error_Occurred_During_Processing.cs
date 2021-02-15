using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using PG.WebApi.Tests.Sandbox.Entities;
using PG.WebApi.Tests.Sandbox.Requests;
using Xunit;

namespace PG.WebApi.Tests.Sandbox
{
    public class Error_Occurred_During_Processing : TestBase, IClassFixture<TestFixture>
    {
        private readonly HttpResponseMessage _httpResponseMessage;
        private readonly ProcessPaymentResponse _processPaymentResponse;
        private readonly ProcessPaymentRequest _request;

        public Error_Occurred_During_Processing(TestFixture fixture) : base(fixture)
        {
            _request = RequestGenerator.Generate();
            _request.MerchantId = "ErrorMerchant";
            _httpResponseMessage = HttpClient.PostAsJsonAsync("/payments", _request).Result;
            _processPaymentResponse = DeserializeJson<ProcessPaymentResponse>(_httpResponseMessage.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Http_Status_Code_Returned_Is_InternalServerError()
        {
            _httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void Payment_Status_Is_Errored()
        {
            _processPaymentResponse.StatusCode.Should().Be(30);
            _processPaymentResponse.StatusText.Should().Be("Errored");
        }

        [Fact]
        public void Payment_Id_Is_Returned()
        {
            _processPaymentResponse.PaymentId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Fetched_Payment_Has_Correct_Data()
        {
            var paymentId = _processPaymentResponse.PaymentId;
            var payment = await DeserializeJsonAsync<Payment>(
                await HttpClient.GetStreamAsync($"/payments/{paymentId}")
            );

            payment.Should().BeEquivalentTo(new
            {
                Id = paymentId,
                MerchantId = _request.MerchantId,
                Amount = _request.Amount,
                CurrencyCode = _request.CurrencyCode,
                CreditCardNumber = "****",
                ExpiryMonth = _request.ExpiryMonth,
                ExpiryYear = _request.ExpiryYear,
                Cvv = _request.Cvv,
                PaymentStatus = 30
            });

            payment.BankIdentifier.Should().BeNull();
        }
    }
}
