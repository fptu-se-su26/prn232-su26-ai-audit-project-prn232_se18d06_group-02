using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Checkout.Dtos
{
    public class AvailableVoucherDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public VoucherType Type { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTime EndAt { get; set; }
        public int UsedCount { get; set; }
        public int UsageLimit { get; set; }
        public bool IsEligible { get; set; }
        public string? IneligibleReason { get; set; }
    }
}
