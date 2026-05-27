using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence;

public interface IPayoutItemRepository : IRepository<PayoutItem, Guid>
{
    Task<List<PayoutItem>> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct = default);
    Task<List<Guid>> GetSubOrderIdsByTransactionIdAsync(Guid transactionId, CancellationToken ct = default);

    Task<List<Guid>> GetSubOrderIdsByTransactionIdsAsync(List<Guid> transactionIds, CancellationToken ct = default);
}
