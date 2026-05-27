using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Repositories
{
    public class PlatformTransactionRepository : IPlatformTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public PlatformTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<PlatformTransactionDto>> GetPagedTransactionsAsync(
            PlatformTransactionQuery query,
            CancellationToken ct = default)
        {
            var paymentsQuery = _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.SubOrders)
                        .ThenInclude(s => s.Store)
                .Select(p => new PlatformTransactionDto
                {
                    Id = p.Id,
                    TransactionCode = p.PaymentCode,
                    Amount = p.Amount,
                    CreatedAt = p.CreatedAt,
                    TypeLabel = "Payment",
                    StatusLabel = p.Status.ToString(),
                    Direction = "IN",
                    StoreName = p.Order.SubOrders.Select(so => so.Store.StoreName).FirstOrDefault(),
                    ReferenceCode = p.Order.OrderCode.ToString(),
                    PaymentId = p.Id,
                    WalletTransactionId = null,
                    PayoutBatchId = null,
                    Note = p.TransactionRef
                });

            var walletQuery = _context.WalletTransactions
                .Include(w => w.PayoutBatch)
                .Include(w => w.PayoutTransaction)
                    .ThenInclude(pt => pt.Store)
                .Select(w => new PlatformTransactionDto
                {
                    Id = w.Id,
                    TransactionCode = w.TransactionCode,
                    Amount = w.Amount,
                    CreatedAt = w.CreatedAt,
                    TypeLabel = w.Type.ToString(),
                    StatusLabel = w.Status.ToString(),
                    Direction = w.Direction.ToString(),
                    StoreName = w.PayoutTransaction != null ? w.PayoutTransaction.Store.StoreName : null,
                    ReferenceCode = w.ReferenceCode ?? (w.PayoutBatch != null ? w.PayoutBatch.BatchCode : null),
                    PaymentId = null,
                    WalletTransactionId = w.Id,
                    PayoutBatchId = w.PayoutBatchId,
                    Note = w.Note
                });

            var combinedQuery = paymentsQuery.Union(walletQuery);

            combinedQuery = ApplyFilters(combinedQuery, query);

            var totalCount = await combinedQuery.CountAsync(ct);

            // Sorting
            var sortBy = (query.SortBy ?? "").ToLower();
            var isAsc = (query.SortDirection ?? "").ToLower() == "asc";

            combinedQuery = sortBy switch
            {
                "date" => isAsc ? combinedQuery.OrderBy(x => x.CreatedAt) : combinedQuery.OrderByDescending(x => x.CreatedAt),
                "amount" => isAsc ? combinedQuery.OrderBy(x => x.Amount) : combinedQuery.OrderByDescending(x => x.Amount),
                "type" => isAsc ? combinedQuery.OrderBy(x => x.TypeLabel) : combinedQuery.OrderByDescending(x => x.TypeLabel),
                "store" => isAsc ? combinedQuery.OrderBy(x => x.StoreName) : combinedQuery.OrderByDescending(x => x.StoreName),
                _ => combinedQuery.OrderByDescending(x => x.CreatedAt)
            };

            var items = await combinedQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            return new PagedResult<PlatformTransactionDto>(items, totalCount, query.PageNumber, query.PageSize);
        }

        public async Task<AdminPlatformTransactionSummaryDto> GetSummaryAsync(
            PlatformTransactionQuery query,
            CancellationToken ct = default)
        {
            // Base queries for summary
            var paymentsBase = _context.Payments.AsNoTracking();
            var walletBase = _context.WalletTransactions.AsNoTracking();

            var totalInflow = await paymentsBase
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0;

            var totalOutflow = await walletBase
                .Where(w => w.Status == WalletTransactionStatus.Completed && w.Direction == TransactionDirection.OUT)
                .SumAsync(w => (decimal?)w.Amount, ct) ?? 0;

            var totalTransactions = await paymentsBase.CountAsync(ct) + await walletBase.CountAsync(ct);

            return new AdminPlatformTransactionSummaryDto
            {
                TotalInflow = totalInflow,
                TotalOutflow = totalOutflow,
                TotalTransactions = totalTransactions,
                WalletBalance = 0 // Will be populated by service using IPayoutClient
            };
        }

        private IQueryable<PlatformTransactionDto> ApplyFilters(IQueryable<PlatformTransactionDto> query, PlatformTransactionQuery filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                query = query.Where(x => 
                    x.TransactionCode.ToLower().Contains(s) || 
                    (x.ReferenceCode != null && x.ReferenceCode.ToLower().Contains(s)) ||
                    (x.Note != null && x.Note.ToLower().Contains(s))
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(x => x.TypeLabel == filter.Type);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(x => x.StatusLabel == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.Direction))
            {
                query = query.Where(x => x.Direction == filter.Direction);
            }

            if (!string.IsNullOrWhiteSpace(filter.StoreName))
            {
                var s = filter.StoreName.Trim().ToLower();
                query = query.Where(x => x.StoreName != null && x.StoreName.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(filter.BatchCode))
            {
                query = query.Where(x => x.ReferenceCode == filter.BatchCode);
            }

            // Date Range
            var now = DateTime.UtcNow;
            var startDate = filter.StartDate;
            var endDate = filter.EndDate;

            if (!string.IsNullOrEmpty(filter.DateRangeType) && filter.DateRangeType != "Custom")
            {
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

            if (startDate.HasValue) query = query.Where(x => x.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
            {
                var endLocal = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreatedAt <= endLocal);
            }

            if (filter.MinAmount.HasValue) query = query.Where(x => x.Amount >= filter.MinAmount.Value);
            if (filter.MaxAmount.HasValue) query = query.Where(x => x.Amount <= filter.MaxAmount.Value);

            return query;
        }
    }
}
