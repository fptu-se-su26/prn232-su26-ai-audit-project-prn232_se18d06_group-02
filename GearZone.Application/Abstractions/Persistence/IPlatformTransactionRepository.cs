using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IPlatformTransactionRepository
    {
        Task<PagedResult<PlatformTransactionDto>> GetPagedTransactionsAsync(
            PlatformTransactionQuery query,
            CancellationToken ct = default);

        Task<AdminPlatformTransactionSummaryDto> GetSummaryAsync(
            PlatformTransactionQuery query,
            CancellationToken ct = default);
    }
}
