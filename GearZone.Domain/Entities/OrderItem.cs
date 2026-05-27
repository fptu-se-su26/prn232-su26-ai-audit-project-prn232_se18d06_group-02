using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class OrderItem : Entity<Guid>
    {
        public Guid SubOrderId { get; set; }
        public Guid VariantId { get; set; }

        public string ProductNameSnapshot { get; set; } = string.Empty;
        public string VariantNameSnapshot { get; set; } = string.Empty;
        public string SkuSnapshot { get; set; } = string.Empty;
        public decimal UnitPriceSnapshot { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }

        // Navigation
        public SubOrder SubOrder { get; set; } = null!;
        public ProductVariant Variant { get; set; } = null!;
        public ProductReview? Review { get; set; }
    }

}
