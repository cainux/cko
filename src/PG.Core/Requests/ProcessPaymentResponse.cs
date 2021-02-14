using System;

namespace PG.Core.Requests
{
    public class ProcessPaymentResponse
    {
        public Guid PaymentId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}
