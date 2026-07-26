using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class SubOrder : Entity<Guid>
    {
        public Guid OrderId { get; set; }
        public Guid StoreId { get; set; }
        
        public OrderStatus Status { get; set; }
        public PayoutStatus PayoutStatus { get; set; } = PayoutStatus.Unpaid;

        public decimal Subtotal { get; set; }
        public decimal PromotionDiscountAmount { get; set; }
        public decimal SellerVoucherDiscountAmount { get; set; }
        public decimal CommissionableAmount { get; set; }
        public decimal CommissionRateSnapshot { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public Store Store { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public PayoutItem? PayoutItem { get; set; }
    }
}
