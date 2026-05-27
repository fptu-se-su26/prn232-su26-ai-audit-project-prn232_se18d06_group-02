using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories;

public class PayoutBatchRepository : Repository<PayoutBatch, Guid>, IPayoutBatchRepository
{
    public PayoutBatchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PayoutBatch?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Transactions)
                .ThenInclude(t => t.Store)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> ExistsByPeriodAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(x => x.PeriodStart == periodStart && x.PeriodEnd == periodEnd, ct);
    }

    public async Task<PagedResult<PayoutBatch>> GetPagedAsync(AdminPayoutBatchQueryDto queryDto, CancellationToken ct = default)
    {
        var query = ApplyFilters(_dbSet.AsQueryable(), queryDto);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync(ct);

        return new PagedResult<PayoutBatch>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
    }

    public async Task<AdminPayoutBatchSummaryDto> GetSummaryAsync(AdminPayoutBatchQueryDto queryDto, CancellationToken ct = default)
    {
        var query = ApplyFilters(_dbSet.AsQueryable(), queryDto);

        var summary = await query
            .GroupBy(x => 1)
            .Select(g => new AdminPayoutBatchSummaryDto
            {
                TotalGrossAmount = g.Sum(x => x.TotalGrossAmount),
                TotalCommissionAmount = g.Sum(x => x.TotalCommissionAmount),
                TotalNetAmount = g.Sum(x => x.TotalNetAmount),
                BatchCount = g.Count()
            })
            .FirstOrDefaultAsync(ct);

        return summary ?? new AdminPayoutBatchSummaryDto();
    }

    private static IQueryable<PayoutBatch> ApplyFilters(IQueryable<PayoutBatch> query, AdminPayoutBatchQueryDto queryDto)
    {
        if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
        {
            query = query.Where(x => x.BatchCode.Contains(queryDto.SearchTerm));
        }

        if (queryDto.Status.HasValue)
        {
            query = query.Where(x => x.Status == queryDto.Status.Value);
        }

        if (queryDto.StartDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= queryDto.StartDate.Value);
        }

        if (queryDto.EndDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= queryDto.EndDate.Value);
        }

        return query;
    }

    public async Task<decimal> GetTotalNetAmountByStatusesAsync(PayoutBatchStatus[] statuses, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => statuses.Contains(x.Status))
            .SumAsync(x => x.TotalNetAmount, ct);
    }
}
