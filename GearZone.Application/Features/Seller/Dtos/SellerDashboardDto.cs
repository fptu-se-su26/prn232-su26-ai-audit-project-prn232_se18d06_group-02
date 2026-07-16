using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Seller.Dtos
{
    public class SellerDashboardMonthlyPointDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class SellerDashboardRecentOrderDto
    {
        public Guid SubOrderId { get; set; }
        public long OrderCode { get; set; }
        public string BuyerName { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; }

        public decimal Subtotal { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SellerDashboardRecentPayoutDto
    {
        public string TransactionCode { get; set; } = string.Empty;
        public decimal NetAmount { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PayoutTransactionStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Everything the seller dashboard screen shows, in one payload.</summary>
    public class SellerDashboardDto
    {
        public bool HasStore { get; set; }
        public string StoreName { get; set; } = "Your Store";
        public int CustomerConversationCount { get; set; }
        public int CustomerUnreadCount { get; set; }

        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int FulfilledOrders { get; set; }

        public decimal GrossRevenue { get; set; }
        public decimal PaidOutAmount { get; set; }
        public decimal PendingPayoutAmount { get; set; }

        public List<SellerDashboardMonthlyPointDto> RevenueByMonth { get; set; } = new();
        public List<SellerDashboardRecentOrderDto> RecentOrders { get; set; } = new();
        public List<SellerDashboardRecentPayoutDto> RecentPayouts { get; set; } = new();
    }
}
