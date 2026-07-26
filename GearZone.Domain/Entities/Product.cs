using System;
using System.Collections.Generic;
using System.Text;

using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class Product : Entity<Guid>
    {
        public Guid StoreId { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SpecsJson { get; set; } = "{}";
        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public string? StatusReason { get; set; }
        public decimal BasePrice { get; set; }
        public int SoldCount { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Store Store { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public Brand Brand { get; set; } = null!;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();
    }

}
