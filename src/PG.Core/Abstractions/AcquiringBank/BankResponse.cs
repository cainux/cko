namespace PG.Core.Abstractions.AcquiringBank
{
    public class BankResponse
    {
        public string BankIdentifier { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}
