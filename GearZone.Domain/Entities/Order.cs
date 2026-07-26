using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class Order : Entity<Guid>
    {
        public long OrderCode { get; set; }
        public string UserId { get; set; } = string.Empty;
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
        public Guid? CheckoutRequestId { get; set; }

        // Voucher / Discount
        public Guid? OrderVoucherId { get; set; }
        public decimal OrderDiscountAmount { get; set; }
        public string? OrderVoucherCodeSnapshot { get; set; }
        public VoucherScope? OrderVoucherScopeSnapshot { get; set; }
        public Guid? ShippingVoucherId { get; set; }
        public decimal ShippingDiscountAmount { get; set; }
        public string? ShippingVoucherCodeSnapshot { get; set; }
        public VoucherScope? ShippingVoucherScopeSnapshot { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Voucher? OrderVoucher { get; set; }
        public Voucher? ShippingVoucher { get; set; }
        public ICollection<SubOrder> SubOrders { get; set; } = new List<SubOrder>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
        public ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<PromotionReservation> PromotionReservations { get; set; } = new List<PromotionReservation>();
    }

}
