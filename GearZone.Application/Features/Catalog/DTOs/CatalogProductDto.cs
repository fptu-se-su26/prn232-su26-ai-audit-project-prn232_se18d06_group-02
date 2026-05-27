using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Catalog.DTOs
{
    public class CatalogProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal? OriginalPrice { get; set; } // Assuming BasePrice is sale price and Original is higher, or we just map it.
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreSlug { get; set; } = string.Empty;
        public string StoreLogoUrl { get; set; } = string.Empty;
        public List<string> SaleBadges { get; set; } = new List<string>();
        public List<string> HighlightTags { get; set; } = new List<string>(); // e.g. "12GB VRAM"
        public bool IsInStock { get; set; }
        public Guid DefaultVariantId { get; set; }
    }
}
