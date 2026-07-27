using GearZone.Application.Features.Promotions.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Services
{
    public interface IPromotionPricingService
    {
        Task<IReadOnlyDictionary<Guid, PromotionPriceDto>> GetPricesAsync(
            IReadOnlyCollection<ProductVariant> variants,
            DateTime? utcNow = null,
            CancellationToken ct = default);

        Task<IReadOnlyDictionary<Guid, PromotionPriceDto>> GetProductPricesAsync(
            IReadOnlyDictionary<Guid, decimal> productPrices,
            DateTime? utcNow = null,
            CancellationToken ct = default);

        PromotionPriceDto Calculate(ProductVariant variant, PromotionCampaign? campaign);
    }
}
