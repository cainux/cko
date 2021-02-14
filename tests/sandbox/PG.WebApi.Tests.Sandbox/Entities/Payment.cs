namespace PG.WebApi.Tests.Sandbox.Entities
{
    public class Payment
    {
        public long Id { get; set; }
        public string MerchantId { get; set; }

        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }

        public string CreditCardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Cvv { get; set; }

        public string BankIdentifier { get; set; }
        public int PaymentStatus { get; set; }
    }
}
