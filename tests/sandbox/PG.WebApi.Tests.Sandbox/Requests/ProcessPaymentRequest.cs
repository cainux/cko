namespace PG.WebApi.Tests.Sandbox.Requests
{
    public class ProcessPaymentRequest
    {
        public string MerchantId { get; set; }
        public string CreditCardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string Cvv { get; set; }
    }
}
