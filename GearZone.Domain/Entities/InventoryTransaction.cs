using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class InventoryTransaction : Entity<Guid>
    {
        public Guid VariantId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int QuantityChange { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;

        // Navigation
        public ProductVariant Variant { get; set; } = null!;
        public ApplicationUser CreatedByUser { get; set; } = null!;
    }

}
