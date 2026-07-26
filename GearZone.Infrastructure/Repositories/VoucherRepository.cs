using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Repositories
{
    public class VoucherRepository : Repository<Voucher, Guid>, IVoucherRepository
    {
        public VoucherRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<PagedResult<Voucher>> GetPaginatedAdminVouchersAsync(AdminVoucherQueryDto query)
        {
            var baseQuery = Query()
                .Include(v => v.Category)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(query.Search))
            {
                var search = query.Search.ToLower();
                baseQuery = baseQuery.Where(v => v.Name.ToLower().Contains(search) || v.Code.ToLower().Contains(search));
            }

            if (query.Status.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.Status == query.Status.Value);
            }

            if (query.Scope.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.Scope == query.Scope.Value);
            }

            if (query.VoucherType.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.Type == query.VoucherType.Value);
            }

            if (query.DiscountType.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.DiscountType == query.DiscountType.Value);
            }

            if (query.CategoryId.HasValue)
            {
                // Should also check for children categories if it's a parent category
                // For simplicity now, just match direct category
                baseQuery = baseQuery.Where(v => v.CategoryId == query.CategoryId.Value);
            }

            if (query.StartDate.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.StartAt >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                var endOfDay = query.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                baseQuery = baseQuery.Where(v => v.EndAt <= endOfDay);
            }

            var totalCount = await baseQuery.CountAsync();

            // Apply Dynamic Sorting
            baseQuery = query.SortBy?.ToLower() switch
            {
                "code" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.Code) : baseQuery.OrderByDescending(v => v.Code),
                "name" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.Name) : baseQuery.OrderByDescending(v => v.Name),
                "discount" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.DiscountValue) : baseQuery.OrderByDescending(v => v.DiscountValue),
                "usage" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.UsedCount) : baseQuery.OrderByDescending(v => v.UsedCount),
                "limit" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.UsageLimit) : baseQuery.OrderByDescending(v => v.UsageLimit),
                "start" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.StartAt) : baseQuery.OrderByDescending(v => v.StartAt),
                "expiry" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.EndAt) : baseQuery.OrderByDescending(v => v.EndAt),
                "createdAt" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.CreatedAt) : baseQuery.OrderByDescending(v => v.CreatedAt),
                _ => baseQuery.OrderByDescending(v => v.CreatedAt)
            };

            var items = await baseQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Voucher>(items, totalCount, query.PageNumber, query.PageSize);
        }

        public async Task<AdminVoucherSummaryDto> GetAdminVoucherSummaryAsync()
        {
            var now = DateTime.Now;
            var vouchers = await Query().AsNoTracking().ToListAsync();

            var totalVouchers = vouchers.Count;
            var activeToday = vouchers.Count(v => v.Status == VoucherStatus.Active && v.IsActive && v.EndAt >= now);
            
            var totalLimit = vouchers.Sum(v => v.UsageLimit);
            var totalUsed = vouchers.Sum(v => v.UsedCount);
            var redemptionRate = totalLimit > 0 ? Math.Round((decimal)totalUsed / totalLimit * 100, 1) : 0;

            // Simplified calculation for total saved amount
            var totalSavedAmount = 0m; 

            return new AdminVoucherSummaryDto
            {
                TotalVouchers = totalVouchers,
                ActiveToday = activeToday,
                RedemptionRate = redemptionRate,
                TotalSavedAmount = totalSavedAmount
            };
        }

        public async Task<PagedResult<Voucher>> GetPaginatedSellerVouchersAsync(Guid storeId, SellerVoucherQueryDto query)
        {
            var baseQuery = Query()
                .Include(v => v.Category)
                .Include(v => v.Usages)
                .AsNoTracking()
                .Where(v => v.StoreId == storeId && v.Scope == VoucherScope.Seller);

            if (!string.IsNullOrEmpty(query.Search))
            {
                var search = query.Search.ToLower();
                baseQuery = baseQuery.Where(v => v.Name.ToLower().Contains(search) || v.Code.ToLower().Contains(search));
            }

            if (query.Status.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.Status == query.Status.Value);
            }

            if (query.VoucherType.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.Type == query.VoucherType.Value);
            }

            if (query.DiscountType.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.DiscountType == query.DiscountType.Value);
            }

            if (query.CategoryId.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.CategoryId == query.CategoryId.Value);
            }

            if (query.StartDate.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.StartAt >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                var endOfDay = query.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                baseQuery = baseQuery.Where(v => v.EndAt <= endOfDay);
            }

            var totalCount = await baseQuery.CountAsync();

            baseQuery = query.SortBy?.ToLower() switch
            {
                "code" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.Code) : baseQuery.OrderByDescending(v => v.Code),
                "name" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.Name) : baseQuery.OrderByDescending(v => v.Name),
                "discount" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.DiscountValue) : baseQuery.OrderByDescending(v => v.DiscountValue),
                "usage" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.UsedCount) : baseQuery.OrderByDescending(v => v.UsedCount),
                "limit" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.UsageLimit) : baseQuery.OrderByDescending(v => v.UsageLimit),
                "start" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.StartAt) : baseQuery.OrderByDescending(v => v.StartAt),
                "expiry" => query.SortDirection == "asc" ? baseQuery.OrderBy(v => v.EndAt) : baseQuery.OrderByDescending(v => v.EndAt),
                _ => baseQuery.OrderByDescending(v => v.CreatedAt)
            };

            var items = await baseQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Voucher>(items, totalCount, query.PageNumber, query.PageSize);
        }

        public async Task<SellerVoucherSummaryDto> GetSellerVoucherSummaryAsync(Guid storeId)
        {
            var now = DateTime.UtcNow;
            var vouchers = await Query()
                .AsNoTracking()
                .Where(v => v.StoreId == storeId && v.Scope == VoucherScope.Seller)
                .ToListAsync();

            var totalLimit = vouchers.Sum(v => v.UsageLimit);
            var redeemed = await _context.VoucherUsages
                .Where(x =>
                    x.Voucher.StoreId == storeId &&
                    x.Voucher.Scope == VoucherScope.Seller &&
                    x.Status == VoucherUsageStatus.Redeemed)
                .Select(x => new { x.DiscountAmount })
                .ToListAsync();
            var redemptionRate = totalLimit > 0
                ? Math.Round((decimal)redeemed.Count / totalLimit * 100, 1)
                : 0;

            return new SellerVoucherSummaryDto
            {
                TotalVouchers = vouchers.Count,
                ActiveToday = vouchers.Count(v =>
                    v.IsActive &&
                    v.StartAt <= now &&
                    v.EndAt > now &&
                    v.UsedCount < v.UsageLimit),
                RedemptionRate = redemptionRate,
                TotalSavedAmount = redeemed.Sum(x => x.DiscountAmount)
            };
        }

        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            var normalized = code.Trim().ToUpper();
            return await Query()
                .Include(v => v.Store)
                .Include(v => v.Category)
                .FirstOrDefaultAsync(v => v.Code == normalized);
        }

        public async Task<List<Voucher>> GetAvailableVouchersAsync(VoucherType type)
        {
            var now = DateTime.Now;
            return await Query()
                .AsNoTracking()
                .Include(v => v.Store)
                .Include(v => v.Category)
                .Where(v => v.Type == type
                    && v.IsActive
                    && v.StartAt <= now
                    && v.EndAt >= now
                    && v.UsedCount < v.UsageLimit)
                .OrderByDescending(v => v.DiscountValue)
                .ToListAsync();
        }

        public async Task<bool> TryReserveUsageAsync(
            Guid voucherId,
            DateTime utcNow,
            CancellationToken ct = default)
        {
            var affected = await _dbSet
                .Where(v =>
                    v.Id == voucherId &&
                    v.IsActive &&
                    v.StartAt <= utcNow &&
                    v.EndAt > utcNow &&
                    v.UsedCount < v.UsageLimit)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(v => v.UsedCount, v => v.UsedCount + 1), ct);

            return affected == 1;
        }

        public async Task ReleaseUsageCapacityAsync(
            Guid voucherId,
            CancellationToken ct = default)
        {
            await _dbSet
                .Where(v => v.Id == voucherId && v.UsedCount > 0)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(v => v.UsedCount, v => v.UsedCount - 1), ct);
        }
    }
}

