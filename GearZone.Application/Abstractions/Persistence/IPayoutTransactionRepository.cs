using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence;

public interface IPayoutTransactionRepository : IRepository<PayoutTransaction, Guid>
{
    Task<PayoutTransaction?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<List<PayoutTransaction>> GetByBatchIdAsync(Guid batchId, CancellationToken ct = default);

    Task<List<PayoutTransaction>> GetFailedWithRetryRemainingAsync(int maxRetry, CancellationToken ct = default);

    Task<PagedResult<PayoutTransaction>> GetByStoreIdAsync(Guid storeId, int page, int pageSize, CancellationToken ct = default);

    Task<PagedResult<PayoutTransaction>> GetPagedAsync(PayoutTransactionQueryDto query);

    Task<AdminPayoutTransactionSummaryDto> GetSummaryAsync(PayoutTransactionQueryDto query);

    Task UpdateRangeAsync(List<PayoutTransaction> transactions, CancellationToken ct = default);
}
