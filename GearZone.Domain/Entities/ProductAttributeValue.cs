using System;

namespace GearZone.Domain.Entities
{
    public class ProductAttributeValue : Entity<Guid>
    {
        public Guid ProductId { get; set; }
        public int CategoryAttributeId { get; set; }
        public int? CategoryAttributeOptionId { get; set; }
        public string? Value { get; set; } // For custom values (e.g., "16384" for CUDA Cores)

        public Product Product { get; set; } = null!;
        public CategoryAttribute CategoryAttribute { get; set; } = null!;
        public CategoryAttributeOption? CategoryAttributeOption { get; set; }
    }
}
