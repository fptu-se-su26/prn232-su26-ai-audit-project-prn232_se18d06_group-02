using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Seller.Dtos
{
    /// <summary>Reference to an existing variant in the seller's store, keyed by SKU.</summary>
    public class StoreVariantRefDto
    {
        public Guid VariantId { get; set; }
        public Guid ProductId { get; set; }
        public int StockQuantity { get; set; }
    }

    /// <summary>A raw row parsed from the import spreadsheet (all values as strings).</summary>
    public class ProductImportRawRow
    {
        /// <summary>1-based spreadsheet row number (as the seller sees it), for error messages.</summary>
        public int RowNumber { get; set; }
        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public string? BasePrice { get; set; }
        public string? ImageUrls { get; set; }
        public string? VariantName { get; set; }
        public string? Sku { get; set; }
        public string? Price { get; set; }
        public string? Stock { get; set; }
    }

    /// <summary>One validated variant line shown in the preview table.</summary>
    public class ProductImportRowResultDto
    {
        public int RowNumber { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsValid { get; set; }
        /// <summary>What this valid row will do: "New product", "New variant", or "Restock".</summary>
        public string Action { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>Result of validating an uploaded file, without writing anything.</summary>
    public class ProductImportPreviewDto
    {
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        /// <summary>Distinct new products that would be created from the valid rows.</summary>
        public int ProductCount { get; set; }
        /// <summary>New variants that would be added to already-existing products.</summary>
        public int NewVariants { get; set; }
        /// <summary>Existing variants (matched by SKU) that would be restocked.</summary>
        public int Restocks { get; set; }
        public List<ProductImportRowResultDto> Rows { get; set; } = new();
    }

    /// <summary>Outcome of actually committing an import.</summary>
    public class ProductImportResultDto
    {
        public int ProductsCreated { get; set; }
        public int VariantsCreated { get; set; }
        /// <summary>Existing variants whose stock was increased (matched by SKU).</summary>
        public int VariantsRestocked { get; set; }
        public int RowsSkipped { get; set; }
        /// <summary>Product-level failures encountered while writing (name + reason).</summary>
        public List<string> Errors { get; set; } = new();
    }
}
