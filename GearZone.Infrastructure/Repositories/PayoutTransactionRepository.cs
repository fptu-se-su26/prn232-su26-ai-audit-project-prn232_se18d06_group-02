using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories;

public class PayoutTransactionRepository : Repository<PayoutTransaction, Guid>, IPayoutTransactionRepository
{
    public PayoutTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PayoutTransaction?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Batch)
            .Include(x => x.Store)
            .Include(x => x.Items)
                .ThenInclude(i => i.SubOrder)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<List<PayoutTransaction>> GetByBatchIdAsync(Guid batchId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Store)
            .Where(x => x.PayoutBatchId == batchId)
            .ToListAsync(ct);
    }

    public async Task<List<PayoutTransaction>> GetFailedWithRetryRemainingAsync(int maxRetry, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.Status == PayoutTransactionStatus.Failed && x.RetryCount < maxRetry)
            .ToListAsync(ct);
    }

    public Task UpdateRangeAsync(List<PayoutTransaction> transactions, CancellationToken ct = default)
    {
        _dbSet.UpdateRange(transactions);
        return Task.CompletedTask;
    }

    public async Task<PagedResult<PayoutTransaction>> GetByStoreIdAsync(Guid storeId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(x => x.Batch)
            .Where(x => x.StoreId == storeId);
            
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PayoutTransaction>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<PayoutTransaction>> GetPagedAsync(PayoutTransactionQueryDto query)
    {
        var dbQuery = _dbSet
            .Include(x => x.Store)
            .Include(x => x.Batch)
            .AsQueryable();

        dbQuery = ApplyFilters(dbQuery, query);

        var totalCount = await dbQuery.CountAsync();

        // Dynamic sorting
        var sortBy = (query.SortBy ?? "").ToLower();
        var isAsc = (query.SortDirection ?? "").ToLower() == "asc";

        dbQuery = sortBy switch
        {
            "code" => isAsc ? dbQuery.OrderBy(x => x.TransactionCode) : dbQuery.OrderByDescending(x => x.TransactionCode),
            "store" => isAsc ? dbQuery.OrderBy(x => x.Store.StoreName) : dbQuery.OrderByDescending(x => x.Store.StoreName),
            "gross" => isAsc ? dbQuery.OrderBy(x => x.GrossAmount) : dbQuery.OrderByDescending(x => x.GrossAmount),
            "commission" => isAsc ? dbQuery.OrderBy(x => x.CommissionAmount) : dbQuery.OrderByDescending(x => x.CommissionAmount),
            "net" => isAsc ? dbQuery.OrderBy(x => x.NetAmount) : dbQuery.OrderByDescending(x => x.NetAmount),
            "status" => isAsc ? dbQuery.OrderBy(x => x.Status) : dbQuery.OrderByDescending(x => x.Status),
            "date" => isAsc ? dbQuery.OrderBy(x => x.CreatedAt) : dbQuery.OrderByDescending(x => x.CreatedAt),
            _ => dbQuery.OrderByDescending(x => x.CreatedAt)
        };

        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<PayoutTransaction>(items, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<AdminPayoutTransactionSummaryDto> GetSummaryAsync(PayoutTransactionQueryDto query)
    {
        var dbQuery = _dbSet.AsQueryable();

        dbQuery = ApplyFilters(dbQuery, query);

        var totalTransactions = await dbQuery.CountAsync();
        var totalGrossRevenue = await dbQuery.SumAsync(x => (decimal?)x.GrossAmount) ?? 0;
        var totalCommission = await dbQuery.SumAsync(x => (decimal?)x.CommissionAmount) ?? 0;
        var totalNetDisbursed = await dbQuery.SumAsync(x => (decimal?)x.NetAmount) ?? 0;

        var pendingCount = await dbQuery.CountAsync(x => x.Status == PayoutTransactionStatus.Queued);
        var processingCount = await dbQuery.CountAsync(x => x.Status == PayoutTransactionStatus.Processing);
        var completedCount = await dbQuery.CountAsync(x => x.Status == PayoutTransactionStatus.Success);
        var failedCount = await dbQuery.CountAsync(x => x.Status == PayoutTransactionStatus.Failed || x.Status == PayoutTransactionStatus.ManualRequired);

        return new AdminPayoutTransactionSummaryDto
        {
            TotalTransactions = totalTransactions,
            TotalGrossRevenue = totalGrossRevenue,
            TotalCommission = totalCommission,
            TotalNetDisbursed = totalNetDisbursed,
            PendingCount = pendingCount,
            ProcessingCount = processingCount,
            CompletedCount = completedCount,
            FailedCount = failedCount
        };
    }

    private IQueryable<PayoutTransaction> ApplyFilters(IQueryable<PayoutTransaction> query, PayoutTransactionQueryDto filter)
    {
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(x => x.TransactionCode.ToLower().Contains(searchTerm) || 
                                     x.Store.StoreName.ToLower().Contains(searchTerm));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        // Handle DateRangeType
        var startDate = filter.StartDate;
        var endDate = filter.EndDate;

        if (!string.IsNullOrEmpty(filter.DateRangeType) && filter.DateRangeType != "Custom")
        {
            var now = DateTime.Now; 
            switch (filter.DateRangeType)
            {
                case "Today":
                    startDate = now.Date;
                    endDate = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "Week":
                    var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    startDate = now.AddDays(-1 * diff).Date;
                    endDate = now.Date.AddDays(1).AddTicks(-1);
                    break;
                case "Month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = now.Date.AddDays(1).AddTicks(-1);
                    break;
            }
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var endLocal = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.CreatedAt <= endLocal);
        }

        if (filter.MinAmount.HasValue)
        {
            query = query.Where(x => x.NetAmount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(x => x.NetAmount <= filter.MaxAmount.Value);
        }

        return query;
    }
}
