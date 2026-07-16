using System.Collections.Generic;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Reviews.Dtos;

namespace GearZone.Application.Features.Catalog.DTOs
{
    /// <summary>Everything the public product detail screen needs, in one payload.</summary>
    public class ProductDetailPageDto
    {
        public ProductDetailDto Product { get; set; } = new();
        public PagedResult<ProductReviewListItemDto> Reviews { get; set; } = new();
        public List<CatalogProductDto> RelatedProducts { get; set; } = new();
    }
}
