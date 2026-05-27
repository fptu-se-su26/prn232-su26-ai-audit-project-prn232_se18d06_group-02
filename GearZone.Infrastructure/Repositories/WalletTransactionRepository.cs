using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class WalletTransactionRepository : Repository<WalletTransaction, Guid>, IWalletTransactionRepository
    {
        public WalletTransactionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PagedResult<WalletTransactionDto>> GetPagedAsync(
            WalletTransactionQuery query,
            CancellationToken ct = default)
        {
            var q = _dbSet
                .Include(x => x.PayoutBatch)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                q = q.Where(x =>
                    x.TransactionCode.ToLower().Contains(search) ||
                    (x.ReferenceCode != null && x.ReferenceCode.ToLower().Contains(search)) ||
                    (x.Note != null && x.Note.ToLower().Contains(search))
                );
            }

            // Type filter
            if (query.Type.HasValue)
                q = q.Where(x => x.Type == query.Type.Value);

            // Status filter
            if (query.Status.HasValue)
                q = q.Where(x => x.Status == query.Status.Value);

            var totalCount = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(x => x.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new WalletTransactionDto
                {
                    Id = x.Id,
                    TransactionCode = x.TransactionCode,
                    CreatedAt = x.CreatedAt,
                    Type = x.Type,
                    Direction = x.Direction,
                    ReferenceCode = x.ReferenceCode,
                    Note = x.Note,
                    Amount = x.Amount,
                    BalanceBefore = x.BalanceBefore,
                    BalanceAfter = x.BalanceAfter,
                    Currency = x.Currency,
                    Status = x.Status,
                    Provider = x.Provider,
                    ProviderTransactionId = x.ProviderTransactionId,
                    CreatedByAdminId = x.CreatedByAdminId,
                    PayoutBatchCode = x.PayoutBatch != null ? x.PayoutBatch.BatchCode : null
                })
                .ToListAsync(ct);

            return new PagedResult<WalletTransactionDto>(items, totalCount, query.PageNumber, query.PageSize);
        }

        public async Task<List<WalletTransactionDto>> GetRecentAsync(int count = 30, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(x => x.Status == WalletTransactionStatus.Completed)
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .Select(x => new WalletTransactionDto
                {
                    Id = x.Id,
                    TransactionCode = x.TransactionCode,
                    CreatedAt = x.CreatedAt,
                    Type = x.Type,
                    Direction = x.Direction,
                    Amount = x.Amount,
                    BalanceBefore = x.BalanceBefore,
                    BalanceAfter = x.BalanceAfter,
                    Currency = x.Currency,
                    Status = x.Status
                })
                .ToListAsync(ct);
        }

        public async Task<List<WalletTransactionDto>> GetCompletedSinceAsync(DateTime fromUtc, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(x => x.Status == WalletTransactionStatus.Completed && x.CreatedAt >= fromUtc)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new WalletTransactionDto
                {
                    Id = x.Id,
                    TransactionCode = x.TransactionCode,
                    CreatedAt = x.CreatedAt,
                    Type = x.Type,
                    Direction = x.Direction,
                    Amount = x.Amount,
                    BalanceBefore = x.BalanceBefore,
                    BalanceAfter = x.BalanceAfter,
                    Currency = x.Currency,
                    Status = x.Status
                })
                .ToListAsync(ct);
        }

        public async Task<WalletTransaction?> GetLastCompletedTransactionAsync(CancellationToken ct = default)
        {
            return await _dbSet
                .Where(x => x.Status == WalletTransactionStatus.Completed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }
    }
}
