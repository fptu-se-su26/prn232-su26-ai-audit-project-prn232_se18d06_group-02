using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Orders.Dtos
{
    public class UserOrderQueryDto
    {
        public string Status { get; set; } = "all";
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UserOrderDto
    {
        public Guid SubOrderId { get; set; }
        public Guid OrderId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreSlug { get; set; } = string.Empty;
        public long OrderCode { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public decimal Subtotal { get; set; }
        public bool HasAnyReviewableItem { get; set; }
        public bool HasAnyEditableReview { get; set; }
        public List<UserOrderItemDto> Items { get; set; } = new();
    }

    public class UserOrderItemDto
    {
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OriginalUnitPrice { get; set; }
        public decimal PromotionDiscountAmount { get; set; }
        public string? PromotionName { get; set; }
        public bool CanReview { get; set; }
        public bool CanEditReview { get; set; }
        public Guid? ReviewId { get; set; }
        public DateTime? ReviewDeadline { get; set; }
    }

    public class UserOrderStatusSummaryDto
    {
        public int All { get; set; }
        public int Processing { get; set; }
        public int Delivered { get; set; }
        public int Cancelled { get; set; }
        public int ToReview { get; set; }
    }
}
