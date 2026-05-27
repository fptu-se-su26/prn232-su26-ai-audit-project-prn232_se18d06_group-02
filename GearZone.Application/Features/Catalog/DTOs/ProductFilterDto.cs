using GearZone.Application.Common.Models;
using System.Collections.Generic;

namespace GearZone.Application.Features.Catalog.DTOs
{
    public class ProductFilterDto
    {
        public Guid? StoreId { get; set; }
        public string? Search { get; set; }
        public string? CategorySlug { get; set; }
        public List<string>? BrandSlugs { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStockOnly { get; set; }
        public Dictionary<string, List<string>>? Attributes { get; set; } // e.g. "VRAM" => ["12GB", "16GB"]
        public string? SortBy { get; set; } // "popular", "newest", "price_asc", "price_desc", "rating"
        public string ViewMode { get; set; } = "grid"; // "grid" or "list"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
