using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IPromotionCampaignRepository : IRepository<PromotionCampaign, Guid>
    {
        Task<List<PromotionCampaign>> GetActiveForProductsAsync(
            IReadOnlyCollection<Guid> productIds,
            DateTime utcNow,
            CancellationToken ct = default);

        Task<bool> HasEnabledOverlapAsync(
            Guid storeId,
            IReadOnlyCollection<Guid> productIds,
            DateTime startAt,
            DateTime endAt,
            Guid? excludeCampaignId = null,
            CancellationToken ct = default);

        Task<bool> TryReserveQuantityAsync(
            Guid campaignId,
            int quantity,
            DateTime utcNow,
            CancellationToken ct = default);
    }
}
