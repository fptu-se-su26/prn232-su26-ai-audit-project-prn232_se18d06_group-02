using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Orders.Dtos
{
    public class UserOrderTrackingDto
    {
        public Guid SubOrderId { get; set; }
        public Guid OrderId { get; set; }
        public long OrderCode { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreSlug { get; set; } = string.Empty;

        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }

        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? ShippingProvider { get; set; }
        public string? TrackingNumber { get; set; }

        public List<UserOrderTrackingItemDto> Items { get; set; } = new();
        public List<UserOrderTrackingStatusHistoryDto> StatusHistory { get; set; } = new();
    }

    public class UserOrderTrackingItemDto
    {
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class UserOrderTrackingStatusHistoryDto
    {
        public DateTime ChangedAt { get; set; }
        public OrderStatus? OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string ChangedByDisplayName { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
