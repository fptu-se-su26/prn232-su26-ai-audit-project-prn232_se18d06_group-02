using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Repositories
{
    public class CartItemRepository : Repository<CartItem, Guid>, ICartItemRepository
    {
        public CartItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<CartItem>> GetCartItemsForCheckoutAsync(
            IEnumerable<Guid> cartItemIds,
            string userId,
            CancellationToken ct = default)
        {
            return await _dbSet
                .Include(ci => ci.Cart)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Store)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .Where(ci => cartItemIds.Contains(ci.Id) && ci.Cart.UserId == userId)
                .ToListAsync(ct);
        }

        public async Task DeleteRangeByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var itemIds = ids.Distinct().ToArray();
            if (itemIds.Length == 0)
            {
                return;
            }

            // Clearing purchased cart lines is intentionally idempotent. A tracked
            // RemoveRange issues one DELETE per entity and EF throws a concurrency
            // exception when another checkout request has already removed a line.
            // A conditional set-based delete treats "already gone" as success.
            await _dbSet
                .Where(ci => itemIds.Contains(ci.Id))
                .ExecuteDeleteAsync(ct);
        }
    }
}
