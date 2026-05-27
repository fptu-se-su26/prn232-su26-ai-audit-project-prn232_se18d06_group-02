namespace GearZone.Application.Features.Payment.Dtos
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? CheckoutUrl { get; set; }
        public string? PaymentLinkId { get; set; }
        public string? ErrorMessage { get; set; }

        public string? Bin { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public long? Amount { get; set; }
        public string? Description { get; set; }
        public string? QrCode { get; set; }

        public PaymentResult(bool success, string? checkoutUrl, string? paymentLinkId = null, string? errorMessage = null)
        {
            Success = success;
            CheckoutUrl = checkoutUrl;
            PaymentLinkId = paymentLinkId;
            ErrorMessage = errorMessage;
        }
    }

    public class PaymentVerificationResult
    {
        public bool Success { get; set; }
        public System.Guid? OrderId { get; set; }
        public string? ErrorMessage { get; set; }

        public static PaymentVerificationResult Ok(System.Guid orderId) => new()
        {
            Success = true,
            OrderId = orderId
        };

        public static PaymentVerificationResult Fail(string error) => new()
        {
            Success = false,
            ErrorMessage = error
        };
    }
}
