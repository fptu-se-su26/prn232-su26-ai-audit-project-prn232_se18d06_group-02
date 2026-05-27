using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class Payment : Entity<Guid>
    {
        public Guid OrderId { get; set; }
        public string PaymentCode { get; set; } = string.Empty;
        public PaymentMethod Method { get; set; }

        public string Provider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionRef { get; set; }
        public string? PaymentLinkId { get; set; }
        public string? CheckoutUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
    }
}
