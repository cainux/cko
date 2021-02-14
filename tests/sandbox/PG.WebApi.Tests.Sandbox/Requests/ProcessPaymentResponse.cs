using System;

namespace PG.WebApi.Tests.Sandbox.Requests
{
    public class ProcessPaymentResponse
    {
        public Guid PaymentId { get; set; }
        public int PaymentStatus { get; set; }
    }
}
