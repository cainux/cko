namespace PG.Core.Services.Requests
{
    public class ProcessPaymentResponse
    {
        public long PaymentId { get; set; }
        public PaymentStatus StatusCode { get; set; }
        public string StatusText => StatusCode.ToString();
    }
}
