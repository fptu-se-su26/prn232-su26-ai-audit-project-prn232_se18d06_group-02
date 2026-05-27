using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IWalletTransactionRepository : IRepository<WalletTransaction, Guid>
    {
        Task<PagedResult<WalletTransactionDto>> GetPagedAsync(
            WalletTransactionQuery query,
            CancellationToken ct = default);

        Task<List<WalletTransactionDto>> GetRecentAsync(int count = 30, CancellationToken ct = default);
        Task<List<WalletTransactionDto>> GetCompletedSinceAsync(DateTime fromUtc, CancellationToken ct = default);
        Task<WalletTransaction?> GetLastCompletedTransactionAsync(CancellationToken ct = default);
    }
}
