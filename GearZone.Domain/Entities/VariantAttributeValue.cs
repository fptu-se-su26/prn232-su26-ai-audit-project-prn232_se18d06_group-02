using System;

namespace GearZone.Domain.Entities
{
    public class VariantAttributeValue : Entity<Guid>
    {
        public Guid VariantId { get; set; }
        public int CategoryAttributeId { get; set; }
        public int CategoryAttributeOptionId { get; set; }

        public ProductVariant Variant { get; set; } = null!;
        public CategoryAttribute CategoryAttribute { get; set; } = null!;
        public CategoryAttributeOption CategoryAttributeOption { get; set; } = null!;
    }
}
