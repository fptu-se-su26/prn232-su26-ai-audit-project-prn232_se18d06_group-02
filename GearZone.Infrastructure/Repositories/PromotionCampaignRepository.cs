using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class PromotionCampaignRepository
        : Repository<PromotionCampaign, Guid>, IPromotionCampaignRepository
    {
        public PromotionCampaignRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<List<PromotionCampaign>> GetActiveForProductsAsync(
            IReadOnlyCollection<Guid> productIds,
            DateTime utcNow,
            CancellationToken ct = default)
        {
            if (productIds.Count == 0)
            {
                return Task.FromResult(new List<PromotionCampaign>());
            }

            return _dbSet
                .AsNoTracking()
                .Include(x => x.Products)
                .Where(x =>
                    x.IsEnabled &&
                    x.StartAt <= utcNow &&
                    x.EndAt > utcNow &&
                    x.ReservedQuantity + x.RedeemedQuantity < x.TotalQuantityLimit &&
                    x.Products.Any(p => productIds.Contains(p.ProductId)))
                .ToListAsync(ct);
        }

        public Task<bool> HasEnabledOverlapAsync(
            Guid storeId,
            IReadOnlyCollection<Guid> productIds,
            DateTime startAt,
            DateTime endAt,
            Guid? excludeCampaignId = null,
            CancellationToken ct = default)
        {
            if (productIds.Count == 0)
            {
                return Task.FromResult(false);
            }

            return _dbSet.AsNoTracking().AnyAsync(x =>
                x.StoreId == storeId &&
                x.IsEnabled &&
                (!excludeCampaignId.HasValue || x.Id != excludeCampaignId.Value) &&
                x.StartAt < endAt &&
                x.EndAt > startAt &&
                x.Products.Any(p => productIds.Contains(p.ProductId)), ct);
        }

        public async Task<bool> TryReserveQuantityAsync(
            Guid campaignId,
            int quantity,
            DateTime utcNow,
            CancellationToken ct = default)
        {
            if (quantity <= 0)
            {
                return false;
            }

            var affected = await _dbSet
                .Where(x =>
                    x.Id == campaignId &&
                    x.IsEnabled &&
                    x.StartAt <= utcNow &&
                    x.EndAt > utcNow &&
                    x.ReservedQuantity + x.RedeemedQuantity + quantity <= x.TotalQuantityLimit)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.ReservedQuantity, x => x.ReservedQuantity + quantity)
                    .SetProperty(x => x.UpdatedAt, utcNow), ct);

            return affected == 1;
        }
    }
}
