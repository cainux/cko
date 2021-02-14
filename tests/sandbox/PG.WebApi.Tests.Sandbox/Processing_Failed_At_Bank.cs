using System.Net;
using System.Net.Http;
using System.Text.Json;
using FluentAssertions;
using PG.WebApi.Tests.Sandbox.Entities;
using PG.WebApi.Tests.Sandbox.Requests;
using Xunit;

namespace PG.WebApi.Tests.Sandbox
{
    public class Processing_Failed_At_Bank : TestBase
    {
        private readonly ProcessPaymentRequest _request;
        private readonly HttpResponseMessage _httpResponseMessage;
        private readonly Payment _payment;

        public Processing_Failed_At_Bank()
        {
            _request = RequestGenerator.Generate();
            _request.MerchantId = "FailMerchant";

            _httpResponseMessage = HttpClient.PostAsJsonAsync("/payment/process", _request).Result;
            _payment = JsonSerializer.Deserialize<Payment>(_httpResponseMessage.Content.ReadAsStringAsync().Result, JsonSerializerOptions);
        }

        [Fact]
        public void Http_Status_Code_Returned_Is_Created()
        {
            _httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public void Payment_Status_Is_Failed()
        {
            _payment.PaymentStatus.Should().Be(20);
        }

        [Fact]
        public void Payment_Returned_Should_Match_Request()
        {
            _payment.Should().BeEquivalentTo(_request);
        }
    }
}
