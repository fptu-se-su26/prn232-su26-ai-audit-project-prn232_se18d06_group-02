using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Checkout.Dtos
{
    public class CheckoutQuoteRequestDto
    {
        public List<Guid> CartItemIds { get; set; } = new();
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? OrderVoucherCode { get; set; }
        public string? ShippingVoucherCode { get; set; }
    }

    public class CheckoutQuoteLineDto
    {
        public Guid CartItemId { get; set; }
        public Guid VariantId { get; set; }
        public Guid ProductId { get; set; }
        public Guid StoreId { get; set; }
        public int CategoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal OriginalUnitPrice { get; set; }
        public decimal EffectiveUnitPrice { get; set; }
        public decimal PromotionDiscountAmount { get; set; }
        public Guid? PromotionCampaignId { get; set; }
        public string? PromotionName { get; set; }
        public DateTime? PromotionEndAt { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class AppliedVoucherDto
    {
        public Guid VoucherId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public VoucherScope Scope { get; set; }
        public Guid? StoreId { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class CheckoutQuoteDto
    {
        public bool Success { get; set; }
        public bool IsConflict { get; set; }
        public string? ErrorMessage { get; set; }
        public List<CheckoutQuoteLineDto> Lines { get; set; } = new();
        public decimal MerchandiseSubtotalBeforePromotion { get; set; }
        public decimal PromotionDiscountAmount { get; set; }
        public decimal MerchandiseSubtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal OrderVoucherDiscountAmount { get; set; }
        public decimal ShippingVoucherDiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public AppliedVoucherDto? OrderVoucher { get; set; }
        public AppliedVoucherDto? ShippingVoucher { get; set; }
        public List<AvailableVoucherDto> AvailableOrderVouchers { get; set; } = new();
        public List<AvailableVoucherDto> AvailableShippingVouchers { get; set; } = new();
        public List<GearZone.Application.Features.Shipping.Dtos.StoreShippingFeeDto> StoreShippingFees { get; set; } = new();
    }
}
