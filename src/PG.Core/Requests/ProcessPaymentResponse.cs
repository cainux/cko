using PG.Core.Entities;

namespace PG.Core.Requests
{
    public class ProcessPaymentResponse
    {
        public string BankIdentifier { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}
