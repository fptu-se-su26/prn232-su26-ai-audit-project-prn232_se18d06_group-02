using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using System;

using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class CartRepository : Repository<Cart, Guid>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cart?> GetDetailedCartAsync(string userId)
        {
            var query = _context.Carts.AsQueryable();

            query = query.Include(c => c.Items)
                         .ThenInclude(i => i.Variant)
                         .ThenInclude(v => v.Product)
                         .ThenInclude(p => p.Store);

            query = query.Include(c => c.Items)
                         .ThenInclude(i => i.Variant)
                         .ThenInclude(v => v.Product)
                         .ThenInclude(p => p.Images);

            query = query.Include(c => c.Items)
                         .ThenInclude(i => i.Variant)
                         .ThenInclude(v => v.AttributeValues)
                         .ThenInclude(av => av.CategoryAttributeOption);

            return await query.FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}
