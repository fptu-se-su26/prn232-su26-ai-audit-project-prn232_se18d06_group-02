using GearZone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface ICartItemRepository : IRepository<CartItem, Guid>
    {

        Task<List<CartItem>> GetCartItemsForCheckoutAsync(
            IEnumerable<Guid> cartItemIds,
            string userId,
            CancellationToken ct = default);


        Task DeleteRangeByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}
