using System;
using System.ComponentModel.DataAnnotations;

namespace PG.Core.Services.Requests
{
    public class ProcessPaymentRequest
    {
        [Required]
        public string MerchantId { get; set; }

        [Required, CreditCard]
        public string CreditCardNumber { get; set; }

        [Required, Range(1, 12)]
        public int ExpiryMonth { get; set; }

        [Required]
        public int ExpiryYear { get; set; }

        [Required, Range(0, Double.PositiveInfinity)]
        public decimal Amount { get; set; }

        [Required]
        public string CurrencyCode { get; set; }

        [Required]
        public string Cvv { get; set; }
    }
}
