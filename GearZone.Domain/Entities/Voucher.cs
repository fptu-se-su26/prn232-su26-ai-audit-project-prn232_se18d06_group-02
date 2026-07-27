using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class Voucher : Entity<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public VoucherType Type { get; set; }          // Order / Shipping
        public DiscountType DiscountType { get; set; } // Percent / Fixed

        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }

        public decimal? MinOrderAmount { get; set; }

        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }

        public int MaxUsagePerUser { get; set; }

        public VoucherScope Scope { get; set; } // Platform / Seller

        public Guid? StoreId { get; set; }      // nếu là seller voucher
        public int? CategoryId { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public bool IsActive { get; set; }
        public VoucherStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation
        public Store? Store { get; set; }
        public Category? Category { get; set; }
        public ICollection<VoucherUsage> Usages { get; set; } = new List<VoucherUsage>();
    }
}
