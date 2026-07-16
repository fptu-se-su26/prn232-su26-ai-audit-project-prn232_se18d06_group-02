using System.Collections.Generic;

namespace GearZone.Application.Features.Seller.Dtos
{
    public class SellerProductStatsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int OutofStockProducts { get; set; }
        public int DraftProducts { get; set; }
        public int PendingProducts { get; set; }
    }

    /// <summary>Filtered + paged seller products plus the header stat tiles.</summary>
    public class SellerProductListResponseDto
    {
        public SellerProductStatsDto Stats { get; set; } = new();
        public int TotalCount { get; set; }
        public List<SellerProductListDto> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class SellerCategoryOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }

    public class SellerBrandOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Category + brand options for the product filters and forms. Deliberately flat DTOs:
    /// the Category entity self-references (Parent/Children), so serializing entities here
    /// produces a JSON reference cycle.
    /// </summary>
    public class SellerProductMetadataDto
    {
        public List<SellerCategoryOptionDto> Categories { get; set; } = new();
        public List<SellerBrandOptionDto> Brands { get; set; } = new();
    }
}
