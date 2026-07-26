using System.ComponentModel.DataAnnotations;
using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Promotions.Dtos
{
    public class PromotionCampaignInputDto
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        [Range(0.01, 1_000_000_000)]
        public decimal DiscountValue { get; set; }

        [Range(1, 1_000_000)]
        public int TotalQuantityLimit { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public bool IsEnabled { get; set; } = true;

        [MinLength(1, ErrorMessage = "Select at least one product.")]
        public List<Guid> ProductIds { get; set; } = new();
    }

    public class SellerPromotionQueryDto : PaginationRequest
    {
        public string? Search { get; set; }
        public PromotionStatus? Status { get; set; }
    }

    public class PromotionProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal MinimumVariantPrice { get; set; }
    }

    public class PromotionCampaignDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public int TotalQuantityLimit { get; set; }
        public int ReservedQuantity { get; set; }
        public int RedeemedQuantity { get; set; }
        public int RemainingQuantity { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public bool IsEnabled { get; set; }
        public PromotionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PromotionProductDto> Products { get; set; } = new();
    }

    public class SellerPromotionSummaryDto
    {
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public int ReservedUnits { get; set; }
        public int RedeemedUnits { get; set; }
    }

    public class SellerPromotionListDto
    {
        public PagedResult<PromotionCampaignDto> Campaigns { get; set; } = new();
        public SellerPromotionSummaryDto Summary { get; set; } = new();
    }

    public class PromotionPriceDto
    {
        public Guid VariantId { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal EffectivePrice { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public Guid? CampaignId { get; set; }
        public string? CampaignName { get; set; }
        public DateTime? CampaignEndAt { get; set; }
        public bool HasPromotion => CampaignId.HasValue && DiscountPerUnit > 0;
    }
}
