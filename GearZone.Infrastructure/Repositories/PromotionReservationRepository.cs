using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class PromotionReservationRepository
        : Repository<PromotionReservation, Guid>, IPromotionReservationRepository
    {
        public PromotionReservationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<List<PromotionReservation>> GetByOrderAsync(
            Guid orderId,
            CancellationToken ct = default)
        {
            return _dbSet
                .Include(x => x.Campaign)
                .Include(x => x.OrderItem)
                    .ThenInclude(x => x.SubOrder)
                .Where(x => x.OrderId == orderId)
                .ToListAsync(ct);
        }
    }
}
