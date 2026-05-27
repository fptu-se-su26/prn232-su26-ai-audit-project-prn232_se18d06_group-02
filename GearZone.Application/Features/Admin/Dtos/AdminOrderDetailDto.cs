using System;
using System.Collections.Generic;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminOrderDetailDto
    {
        public Guid Id { get; set; }
        public long OrderCode { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        // Shipping Information
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? ShippingProvider { get; set; }
        public string? TrackingNumber { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<AdminSubOrderDto> SubOrders { get; set; } = new();
        public List<AdminOrderStatusHistoryDto> StatusHistories { get; set; } = new();
        public List<AdminOrderPaymentDto> Payments { get; set; } = new();
    }

    public class AdminSubOrderDto
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreEmail { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public List<AdminOrderItemDto> Items { get; set; } = new();
    }

    public class AdminOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class AdminOrderStatusHistoryDto
    {
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string? Note { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public class AdminOrderPaymentDto
    {
        public Guid Id { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TransactionRef { get; set; }
        public string? CheckoutUrl { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
