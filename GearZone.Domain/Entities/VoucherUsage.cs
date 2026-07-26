using System;
using System.Collections.Generic;
using System.Text;
using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class VoucherUsage : Entity<Guid>
    {
        public Guid VoucherId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid OrderId { get; set; }

        public decimal DiscountAmount { get; set; }

        public DateTime UsedAt { get; set; }
        public VoucherUsageStatus Status { get; set; } = VoucherUsageStatus.Reserved;
        public DateTime? RedeemedAt { get; set; }
        public DateTime? ReleasedAt { get; set; }

        // Navigation
        public Voucher Voucher { get; set; } = null!;
        public Order Order { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
