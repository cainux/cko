namespace PG.Core.Services.Requests
{
    public class ProcessPaymentResponse
    {
        public long PaymentId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}
