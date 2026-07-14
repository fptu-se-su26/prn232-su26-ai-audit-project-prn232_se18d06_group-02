using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Api.Controllers.Seller;

[Authorize(Roles = "Store Owner")]
[Route("api/seller/revenue")]
[ApiController]
public class RevenueController : BaseApiController
{
    private readonly IStoreRepository _storeRepository;
    private readonly IPayoutTransactionRepository _payoutTransactionRepository;
    private const int PageSize = 10;

    public RevenueController(IStoreRepository storeRepository, IPayoutTransactionRepository payoutTransactionRepository)
    {
        _storeRepository = storeRepository;
        _payoutTransactionRepository = payoutTransactionRepository;
    }

    // GET /api/seller/revenue?status=&search=&minAmount=&maxAmount=&dateRangeShortcut=&dateRange=&sort=&dir=&page=
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] PayoutTransactionStatus? status,
        [FromQuery] string? searchTerm,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] string? dateRangeShortcut,
        [FromQuery] string? dateRange,
        [FromQuery] string sortBy = "date",
        [FromQuery] string sortDir = "desc",
        [FromQuery] int page = 1)
    {
        var store = await _storeRepository.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return OkResponse(new { hasStore = false });

        if (page < 1) page = 1;

        var (startDate, endDate) = ResolveDateRange(dateRangeShortcut, dateRange);

        var query = _payoutTransactionRepository.Query().Where(x => x.StoreId == store.Id);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var s = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(x => x.TransactionCode.ToLower().Contains(s) || (x.Batch != null && x.Batch.BatchCode.ToLower().Contains(s)));
        }
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (startDate.HasValue) query = query.Where(x => x.CreatedAt >= startDate.Value);
        if (endDate.HasValue) { var end = endDate.Value.Date.AddDays(1).AddTicks(-1); query = query.Where(x => x.CreatedAt <= end); }
        if (minAmount.HasValue) query = query.Where(x => x.NetAmount >= minAmount.Value);
        if (maxAmount.HasValue) query = query.Where(x => x.NetAmount <= maxAmount.Value);

        var summary = new
        {
            totalTransactions = await query.CountAsync(),
            totalNetAmount = await query.SumAsync(x => (decimal?)x.NetAmount) ?? 0m,
            pendingCount = await query.CountAsync(x => x.Status == PayoutTransactionStatus.Queued || x.Status == PayoutTransactionStatus.Processing || x.Status == PayoutTransactionStatus.ManualRequired),
            completedCount = await query.CountAsync(x => x.Status == PayoutTransactionStatus.Success),
            failedCount = await query.CountAsync(x => x.Status == PayoutTransactionStatus.Failed || x.Status == PayoutTransactionStatus.Excluded),
            pendingAmount = await query.Where(x => x.Status == PayoutTransactionStatus.Queued || x.Status == PayoutTransactionStatus.Processing || x.Status == PayoutTransactionStatus.ManualRequired).SumAsync(x => (decimal?)x.NetAmount) ?? 0m,
            paidAmount = await query.Where(x => x.Status == PayoutTransactionStatus.Success).SumAsync(x => (decimal?)x.NetAmount) ?? 0m
        };

        var isAsc = sortDir == "asc";
        query = sortBy.ToLowerInvariant() switch
        {
            "code" => isAsc ? query.OrderBy(x => x.TransactionCode) : query.OrderByDescending(x => x.TransactionCode),
            "net" => isAsc ? query.OrderBy(x => x.NetAmount) : query.OrderByDescending(x => x.NetAmount),
            "status" => isAsc ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
            _ => isAsc ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = summary.totalTransactions;
        var items = await query.Skip((page - 1) * PageSize).Take(PageSize)
            .Select(x => new
            {
                x.Id, x.TransactionCode,
                batchCode = x.Batch != null ? x.Batch.BatchCode : string.Empty,
                x.OrderCount, x.GrossAmount, x.CommissionAmount, x.NetAmount,
                x.BankName, x.BankAccountNumber,
                status = x.Status.ToString(),
                x.CreatedAt, x.ProcessedAt
            }).ToListAsync();

        return OkResponse(new
        {
            hasStore = true,
            storeName = store.StoreName,
            summary,
            transactions = new { items, totalCount, page, pageSize = PageSize, totalPages = (int)Math.Ceiling(totalCount / (double)PageSize) }
        });
    }

    private static (DateTime? start, DateTime? end) ResolveDateRange(string? shortcut, string? range)
    {
        if (string.IsNullOrWhiteSpace(shortcut)) return (null, null);
        var today = DateTime.UtcNow.Date;
        return shortcut.ToLowerInvariant() switch
        {
            "today" => (today, today),
            "week" => (today.AddDays(-7), today),
            "month" => (today.AddDays(-30), today),
            "custom" => ParseRange(range),
            _ => (null, null)
        };
    }

    private static (DateTime?, DateTime?) ParseRange(string? range)
    {
        if (string.IsNullOrWhiteSpace(range)) return (null, null);
        var parts = range.Split(" to ", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
            return (DateTime.TryParse(parts[0], out var s) ? s : null, DateTime.TryParse(parts[1], out var e) ? e : null);
        if (parts.Length == 1 && DateTime.TryParse(parts[0], out var d))
            return (d, d);
        return (null, null);
    }
}
