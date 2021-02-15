namespace PG.WebApi.Tests.Sandbox.Requests
{
    public class ProcessPaymentResponse
    {
        public long PaymentId { get; set; }
        public int StatusCode { get; set; }
        public string StatusText { get; set; }
    }
}
