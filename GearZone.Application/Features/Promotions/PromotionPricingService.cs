using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Promotions.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Promotions
{
    public class PromotionPricingService : IPromotionPricingService
    {
        private readonly IPromotionCampaignRepository _campaigns;
        private readonly TimeProvider _timeProvider;

        public PromotionPricingService(
            IPromotionCampaignRepository campaigns,
            TimeProvider timeProvider)
        {
            _campaigns = campaigns;
            _timeProvider = timeProvider;
        }

        public async Task<IReadOnlyDictionary<Guid, PromotionPriceDto>> GetPricesAsync(
            IReadOnlyCollection<ProductVariant> variants,
            DateTime? utcNow = null,
            CancellationToken ct = default)
        {
            if (variants.Count == 0)
            {
                return new Dictionary<Guid, PromotionPriceDto>();
            }

            var now = utcNow ?? _timeProvider.GetUtcNow().UtcDateTime;
            var campaigns = await _campaigns.GetActiveForProductsAsync(
                variants.Select(x => x.ProductId).Distinct().ToArray(), now, ct);

            var campaignByProduct = campaigns
                .SelectMany(c => c.Products.Select(p => new { p.ProductId, Campaign = c }))
                .GroupBy(x => x.ProductId)
                .ToDictionary(x => x.Key, x => x.OrderBy(y => y.Campaign.EndAt).First().Campaign);

            return variants.ToDictionary(
                x => x.Id,
                x => Calculate(
                    x,
                    campaignByProduct.GetValueOrDefault(x.ProductId)));
        }

        public PromotionPriceDto Calculate(ProductVariant variant, PromotionCampaign? campaign)
        {
            var original = Math.Max(0, variant.Price);
            if (campaign == null)
            {
                return new PromotionPriceDto
                {
                    VariantId = variant.Id,
                    OriginalPrice = original,
                    EffectivePrice = original
                };
            }

            var discount = campaign.DiscountType == DiscountType.Percent
                ? original * campaign.DiscountValue / 100m
                : campaign.DiscountValue;

            discount = Math.Min(original, Math.Max(0, discount));
            discount = Math.Round(discount, 2, MidpointRounding.AwayFromZero);

            return new PromotionPriceDto
            {
                VariantId = variant.Id,
                OriginalPrice = original,
                EffectivePrice = Math.Max(0, original - discount),
                DiscountPerUnit = discount,
                CampaignId = campaign.Id,
                CampaignName = campaign.Name,
                CampaignEndAt = campaign.EndAt
            };
        }

        public async Task<IReadOnlyDictionary<Guid, PromotionPriceDto>> GetProductPricesAsync(
            IReadOnlyDictionary<Guid, decimal> productPrices,
            DateTime? utcNow = null,
            CancellationToken ct = default)
        {
            if (productPrices.Count == 0)
            {
                return new Dictionary<Guid, PromotionPriceDto>();
            }

            var now = utcNow ?? _timeProvider.GetUtcNow().UtcDateTime;
            var campaigns = await _campaigns.GetActiveForProductsAsync(
                productPrices.Keys.ToArray(), now, ct);
            var campaignByProduct = campaigns
                .SelectMany(c => c.Products.Select(p => new { p.ProductId, Campaign = c }))
                .GroupBy(x => x.ProductId)
                .ToDictionary(x => x.Key, x => x.OrderBy(y => y.Campaign.EndAt).First().Campaign);

            return productPrices.ToDictionary(
                x => x.Key,
                x =>
                {
                    var variant = new ProductVariant
                    {
                        Id = x.Key,
                        ProductId = x.Key,
                        Price = x.Value
                    };
                    return Calculate(
                        variant,
                        campaignByProduct.GetValueOrDefault(x.Key));
                });
        }
    }
}
