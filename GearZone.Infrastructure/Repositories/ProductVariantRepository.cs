using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class ProductVariantRepository : Repository<ProductVariant, Guid>, IProductVariantRepository
    {
        public ProductVariantRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> TryReserveStockAsync(
            Guid variantId,
            int quantity,
            CancellationToken ct = default)
        {
            if (quantity <= 0)
                return false;

            var affected = await _dbSet
                .Where(x =>
                    x.Id == variantId &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    x.StockQuantity >= quantity)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        x => x.StockQuantity,
                        x => x.StockQuantity - quantity)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

            if (affected == 1)
            {
                // ExecuteUpdate bypasses the change tracker. Keep an entity that
                // was loaded for checkout in sync so a later compensation in the
                // same request restores exactly the reserved quantity.
                var tracked = _context.ChangeTracker
                    .Entries<ProductVariant>()
                    .FirstOrDefault(x => x.Entity.Id == variantId);
                if (tracked != null)
                {
                    var newStock = tracked.Entity.StockQuantity - quantity;
                    tracked.Entity.StockQuantity = newStock;
                    tracked.Property(x => x.StockQuantity).OriginalValue = newStock;
                    tracked.Property(x => x.StockQuantity).IsModified = false;
                }
            }

            return affected == 1;
        }
    }
}
