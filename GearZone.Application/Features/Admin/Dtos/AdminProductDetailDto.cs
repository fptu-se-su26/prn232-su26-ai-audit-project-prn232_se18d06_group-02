using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int Stock { get; set; }
        public int SoldCount { get; set; }
        public string Slug { get; set; } = string.Empty;
        public decimal CommissionRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public AdminCategoryInfoDto Category { get; set; } = null!;
        public AdminBrandInfoDto Brand { get; set; } = null!;
        public AdminStoreInfoDto Store { get; set; } = null!;

        public List<string> Images { get; set; } = new List<string>();
        /// <summary>Technical specs aggregated from all variant attribute values (attribute name → distinct option values).</summary>
        public List<AdminProductSpecDto> Specs { get; set; } = new();
        public List<AdminProductVariantDto> Variants { get; set; } = new List<AdminProductVariantDto>();
    }

    /// <summary>One row in the Technical Specifications table.</summary>
    public class AdminProductSpecDto
    {
        public string AttributeName { get; set; } = string.Empty;
        /// <summary>All unique option values that appear across every variant for this attribute.</summary>
        public List<string> Values { get; set; } = new();
    }

    public class AdminCategoryInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class AdminBrandInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class AdminStoreInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string VendorId { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
