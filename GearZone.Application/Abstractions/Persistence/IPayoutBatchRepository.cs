using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Abstractions.Persistence;

public interface IPayoutBatchRepository : IRepository<PayoutBatch, Guid>
{
    Task<PayoutBatch?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByPeriodAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);

    Task<PagedResult<PayoutBatch>> GetPagedAsync(AdminPayoutBatchQueryDto query, CancellationToken ct = default);
    Task<AdminPayoutBatchSummaryDto> GetSummaryAsync(AdminPayoutBatchQueryDto query, CancellationToken ct = default);
    Task<decimal> GetTotalNetAmountByStatusesAsync(PayoutBatchStatus[] statuses, CancellationToken ct = default);
}
