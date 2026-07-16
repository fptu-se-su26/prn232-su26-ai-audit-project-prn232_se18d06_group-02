using System.Collections.Generic;
using GearZone.Domain.Entities;

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

    /// <summary>Category + brand options for the product filters and forms.</summary>
    public class SellerProductMetadataDto
    {
        public List<Category> Categories { get; set; } = new();
        public List<Brand> Brands { get; set; } = new();
    }
}
