using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories;

public class PayoutItemRepository : Repository<PayoutItem, Guid>, IPayoutItemRepository
{
    public PayoutItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<PayoutItem>> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct = default)
    {
        return await _context.Set<PayoutItem>()
            .Where(x => x.PayoutTransactionId == transactionId)
            .ToListAsync(ct);
    }

    public async Task<List<Guid>> GetSubOrderIdsByTransactionIdAsync(Guid transactionId, CancellationToken ct = default)
    {
        return await _context.Set<PayoutItem>()
            .Where(x => x.PayoutTransactionId == transactionId)
            .Select(x => x.SubOrderId)
            .ToListAsync(ct);
    }

    public async Task<List<Guid>> GetSubOrderIdsByTransactionIdsAsync(List<Guid> transactionIds, CancellationToken ct = default)
    {
        return await _context.Set<PayoutItem>()
            .Where(x => transactionIds.Contains(x.PayoutTransactionId))
            .Select(x => x.SubOrderId)
            .ToListAsync(ct);
    }
}
