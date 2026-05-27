using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class CartItem : Entity<Guid>
    {
        public Guid CartId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }

        // Navigation
        public Cart Cart { get; set; } = null!;
        public ProductVariant Variant { get; set; } = null!;
    }
}
