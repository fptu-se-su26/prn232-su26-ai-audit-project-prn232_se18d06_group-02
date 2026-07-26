using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Checkout.Dtos
{
    public class VoucherValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? VoucherId { get; set; }
        public string? VoucherName { get; set; }
        public string? VoucherCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DiscountLabel { get; set; } = string.Empty;
        public VoucherType Type { get; set; }
        public VoucherScope Scope { get; set; }
        public Guid? StoreId { get; set; }

        public static VoucherValidationResult Fail(string message) => new()
        {
            IsValid = false,
            ErrorMessage = message
        };
    }
}
