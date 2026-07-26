using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GearZone.Application.Features.Seller.Dtos
{
    public class SellerProductListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int TotalStock { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PrimaryImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SellerProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int SoldCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string> Specifications { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(5000, ErrorMessage = "Description is too long")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid brand")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Base price is required")]
        [Range(0, 1000000000, ErrorMessage = "Price must be between 0 and 1,000,000,000")]
        public decimal BasePrice { get; set; }

        public bool IsDraft { get; set; }

        public List<ProductSpecDto> Specifications { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
        public List<IFormFile> Images { get; set; } = new();
        /// <summary>Remote image URLs (e.g. from bulk import) to fetch and re-host. Combined with Images, capped at 5.</summary>
        public List<string> ImageUrls { get; set; } = new();
    }

    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(5000, ErrorMessage = "Description is too long")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid brand")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Base price is required")]
        [Range(0, 1000000000, ErrorMessage = "Price must be between 0 and 1,000,000,000")]
        public decimal BasePrice { get; set; }

        public bool IsDraft { get; set; }

        public List<ProductVariantDto> Variants { get; set; } = new();
        public List<IFormFile> NewImages { get; set; } = new();
        public List<ProductSpecDto> Specifications { get; set; } = new();
        public List<string> ExistingImageUrls { get; set; } = new();
    }

    public class ProductSpecDto
    {
        public int AttributeId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public string? ValueType { get; set; }
    }

    public class ProductVariantDto
    {
        [Required(ErrorMessage = "Variant name is required")]
        public string VariantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required")]
        public string Sku { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 1000000000, ErrorMessage = "Price must be between 0 and 1,000,000,000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 1000000, ErrorMessage = "Stock must be between 0 and 1,000,000")]
        public int StockQuantity { get; set; }
        
        // For dynamic attributes per variant
        public List<AttributeSelectionDto> Attributes { get; set; } = new();
    }

    public class AttributeSelectionDto
    {
        public int AttributeId { get; set; }
        public int OptionId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string OptionValue { get; set; } = string.Empty;
    }

    public class CategoryAttributeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FilterType { get; set; } = string.Empty;
        public List<CategoryAttributeOptionDto> Options { get; set; } = new();
    }

    public class CategoryAttributeOptionDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}

