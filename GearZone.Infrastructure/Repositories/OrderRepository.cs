using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order, Guid>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Order>> GetAdminOrdersAsync(AdminOrderQueryDto queryDto)
        {
            var query = _dbSet
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
            {
                var search = queryDto.SearchTerm.Trim().ToLower();
                query = query.Where(o => 
                    o.OrderCode.ToString().Contains(search) || 
                    o.ReceiverName.ToLower().Contains(search) ||
                    o.User.UserName.ToLower().Contains(search));
            }

            if (queryDto.IsPaid.HasValue)
            {
                if (queryDto.IsPaid.Value)
                {
                    query = query.Where(o => o.PaidAt != null);
                }
                else
                {
                    query = query.Where(o => o.PaidAt == null);
                }
            }

            if (queryDto.StoreId.HasValue && queryDto.StoreId.Value != Guid.Empty)
            {
                query = query.Where(o => o.SubOrders.Any(s => s.StoreId == queryDto.StoreId.Value));
            }

            if (queryDto.StartDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= queryDto.StartDate.Value);
            }

            if (queryDto.EndDate.HasValue)
            {
                var endLocal = queryDto.EndDate.Value.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.CreatedAt <= endLocal);
            }

            if (queryDto.MinPrice.HasValue)
            {
                query = query.Where(o => o.GrandTotal >= queryDto.MinPrice.Value);
            }

            if (queryDto.MaxPrice.HasValue)
            {
                query = query.Where(o => o.GrandTotal <= queryDto.MaxPrice.Value);
            }

            // Default sort by CreatedAt Desc
            if (string.IsNullOrWhiteSpace(queryDto.SortBy))
            {
                query = query.OrderByDescending(o => o.CreatedAt);
            }
            else
            {
                bool isDesc = queryDto.SortDirection?.ToLower() == "desc";
                switch (queryDto.SortBy.ToLower())
                {
                    case "ordercode":
                        query = isDesc ? query.OrderByDescending(o => o.OrderCode) : query.OrderBy(o => o.OrderCode);
                        break;
                    case "grandtotal":
                        query = isDesc ? query.OrderByDescending(o => o.GrandTotal) : query.OrderBy(o => o.GrandTotal);
                        break;
                    case "createdat":
                        query = isDesc ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt);
                        break;
                    default:
                        query = query.OrderByDescending(o => o.CreatedAt);
                        break;
                }
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
        }

        public async Task<Order?> GetAdminOrderDetailAsync(Guid id)
        {
            return await _dbSet
                .Include(o => o.User)
                .Include(o => o.Payments)
                .Include(o => o.StatusHistories)
                .Include(o => o.SubOrders)
                    .ThenInclude(s => s.Store)
                .Include(o => o.SubOrders)
                    .ThenInclude(s => s.Items)
                        .ThenInclude(i => i.Variant)
                            .ThenInclude(v => v.Product)
                                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<AdminOrderStatsDto> GetAdminOrderStatsAsync()
        {
            var stats = new AdminOrderStatsDto();

            stats.TotalOrders = await _dbSet.CountAsync();
            stats.PaidOrders = await _dbSet.CountAsync(o => o.PaidAt != null);
            stats.UnpaidOrders = await _dbSet.CountAsync(o => o.PaidAt == null);
            stats.TotalRevenue = await _dbSet.Where(o => o.PaidAt != null).SumAsync(o => o.GrandTotal);

            return stats;
        }
    }
}
