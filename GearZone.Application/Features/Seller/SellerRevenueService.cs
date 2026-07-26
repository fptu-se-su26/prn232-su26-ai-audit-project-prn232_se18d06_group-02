using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Seller
{
    public class SellerRevenueService : ISellerRevenueService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IPayoutTransactionRepository _payoutTransactionRepository;

        private const int PageSize = 10;

        private static readonly PayoutTransactionStatus[] PendingStatuses =
        {
            PayoutTransactionStatus.Queued,
            PayoutTransactionStatus.Processing,
            PayoutTransactionStatus.ManualRequired
        };

        private static readonly PayoutTransactionStatus[] FailedStatuses =
        {
            PayoutTransactionStatus.Failed,
            PayoutTransactionStatus.Excluded
        };

        public SellerRevenueService(
            IStoreRepository storeRepository,
            IPayoutTransactionRepository payoutTransactionRepository)
        {
            _storeRepository = storeRepository;
            _payoutTransactionRepository = payoutTransactionRepository;
        }

        public async Task<SellerRevenueDto> GetRevenueAsync(string ownerUserId, SellerRevenueQueryDto query, CancellationToken ct = default)
        {
            query ??= new SellerRevenueQueryDto();

            var store = await _storeRepository.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null) return new SellerRevenueDto { HasStore = false };

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var sortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "date" : query.SortBy;
            var isAsc = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            var (startDate, endDate) = ResolveDateRange(query.DateRangeShortcut, query.DateRange);

            var source = _payoutTransactionRepository.Query().Where(x => x.StoreId == store.Id);

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.Trim().ToLowerInvariant();
                source = source.Where(x =>
                    x.TransactionCode.ToLower().Contains(search) ||
                    (x.Batch != null && x.Batch.BatchCode.ToLower().Contains(search)));
            }

            if (query.Status.HasValue) source = source.Where(x => x.Status == query.Status.Value);
            if (startDate.HasValue) source = source.Where(x => x.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                source = source.Where(x => x.CreatedAt <= endOfDay);
            }
            if (query.MinAmount.HasValue) source = source.Where(x => x.NetAmount >= query.MinAmount.Value);
            if (query.MaxAmount.HasValue) source = source.Where(x => x.NetAmount <= query.MaxAmount.Value);

            var summary = new SellerPayoutSummaryDto
            {
                TotalTransactions = await source.CountAsync(ct),
                TotalNetAmount = await source.SumAsync(x => (decimal?)x.NetAmount, ct) ?? 0m,
                PendingCount = await source.CountAsync(x => PendingStatuses.Contains(x.Status), ct),
                CompletedCount = await source.CountAsync(x => x.Status == PayoutTransactionStatus.Success, ct),
                FailedCount = await source.CountAsync(x => FailedStatuses.Contains(x.Status), ct),
                PendingAmount = await source.Where(x => PendingStatuses.Contains(x.Status))
                    .SumAsync(x => (decimal?)x.NetAmount, ct) ?? 0m,
                PaidAmount = await source.Where(x => x.Status == PayoutTransactionStatus.Success)
                    .SumAsync(x => (decimal?)x.NetAmount, ct) ?? 0m
            };

            source = sortBy.ToLowerInvariant() switch
            {
                "code" => isAsc ? source.OrderBy(x => x.TransactionCode) : source.OrderByDescending(x => x.TransactionCode),
                "net" => isAsc ? source.OrderBy(x => x.NetAmount) : source.OrderByDescending(x => x.NetAmount),
                "status" => isAsc ? source.OrderBy(x => x.Status) : source.OrderByDescending(x => x.Status),
                _ => isAsc ? source.OrderBy(x => x.CreatedAt) : source.OrderByDescending(x => x.CreatedAt)
            };

            var items = await source
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new SellerPayoutTransactionDto
                {
                    Id = x.Id,
                    TransactionCode = x.TransactionCode,
                    BatchCode = x.Batch != null ? x.Batch.BatchCode : string.Empty,
                    OrderCount = x.OrderCount,
                    GrossAmount = x.GrossAmount,
                    CommissionAmount = x.CommissionAmount,
                    NetAmount = x.NetAmount,
                    BankName = x.BankName,
                    BankAccountNumber = x.BankAccountNumber,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    ProcessedAt = x.ProcessedAt
                })
                .ToListAsync(ct);

            return new SellerRevenueDto
            {
                HasStore = true,
                StoreName = store.StoreName,
                Summary = summary,
                Transactions = new PagedResult<SellerPayoutTransactionDto>(items, summary.TotalTransactions, pageNumber, PageSize)
            };
        }

        private static (DateTime? Start, DateTime? End) ResolveDateRange(string? shortcut, string? range)
        {
            if (string.IsNullOrWhiteSpace(shortcut)) return (null, null);

            var today = DateTime.UtcNow.Date;
            return shortcut.ToLowerInvariant() switch
            {
                "today" => (today, today),
                "week" => (today.AddDays(-7), today),
                "month" => (today.AddDays(-30), today),
                "custom" => ParseCustomRange(range),
                _ => (null, null)
            };
        }

        private static (DateTime? Start, DateTime? End) ParseCustomRange(string? range)
        {
            if (string.IsNullOrWhiteSpace(range)) return (null, null);

            var parts = range.Split(" to ", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
                return (DateTime.TryParse(parts[0], out var s) ? s : null,
                        DateTime.TryParse(parts[1], out var e) ? e : null);
            if (parts.Length == 1 && DateTime.TryParse(parts[0], out var single))
                return (single, single);

            return (null, null);
        }
    }
}
