using System;
using System.Collections.Generic;
using GearZone.Application.Features.Reviews.Dtos;

namespace GearZone.Application.Features.Catalog.DTOs
{
    public class ProductDetailDto
    {
        public Guid Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int SoldCount { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        
        // Brand & Category Info
        public string BrandName { get; set; } = string.Empty;
        public string BrandSlug { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        
        // Store Info
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreSlug { get; set; } = string.Empty;
        public int StoreReviewCount { get; set; }
        public int StoreProductCount { get; set; }
        public DateTime StoreCreatedAt { get; set; }
        public int StoreFollowerCount { get; set; }

        // Shared Images for all variants
        public List<string> ImageUrls { get; set; } = new();

        // Shopee-style Attributes Selection
        public List<AttributeSelectionDto> AttributeSelections { get; set; } = new();

        // All Variants Data (for JS to find matching price/sku/stock)
        public List<VariantDetailDto> Variants { get; set; } = new();

        // Aggregated Specifications
        public List<SpecificationDto> Specifications { get; set; } = new();
        public ProductReviewSummaryDto ReviewSummary { get; set; } = new();
        public EligibleReviewItemDto? EligibleReview { get; set; }
    }

    public class AttributeSelectionDto
    {
        public int AttributeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<AttributeOptionDto> Options { get; set; } = new();
    }

    public class AttributeOptionDto
    {
        public int OptionId { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public class VariantDetailDto
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
    }

    public class SpecificationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ProductSuggestionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string BrandName { get; set; } = string.Empty;
    }
}
