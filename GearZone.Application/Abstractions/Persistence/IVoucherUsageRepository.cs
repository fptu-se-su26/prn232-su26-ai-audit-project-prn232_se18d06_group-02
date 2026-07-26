using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IVoucherUsageRepository : IRepository<VoucherUsage, Guid>
    {
        Task<int> GetUsageCountByUserAsync(Guid voucherId, string userId);
        Task<List<VoucherUsage>> GetByOrderAsync(Guid orderId, CancellationToken ct = default);
    }
}
