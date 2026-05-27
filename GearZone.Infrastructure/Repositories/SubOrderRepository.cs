using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Orders.Dtos;
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
    public class SubOrderRepository : Repository<SubOrder, Guid>, ISubOrderRepository
    {
        public SubOrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<SubOrder>> GetOrdersNotTransfer()
        {
            var orders = await _dbSet
            .Where(o =>
                (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered) &&
                o.PayoutStatus == PayoutStatus.Unpaid &&
                o.Order.Payments.Any(p => p.Status == PaymentStatus.Paid &&
                                          p.Method != PaymentMethod.COD) &&
                (o.DeliveredAt ?? o.UpdatedAt ?? o.CreatedAt) <= DateTime.UtcNow.AddDays(-7)
            )
            .ToListAsync();
            return orders;
        }

        public async Task<List<SubOrder>> GetEligibleForPayoutAsync(
            DateTime periodStart,
            DateTime periodEnd,
            CancellationToken ct = default)
        {
            return await BuildEligibleForPayoutQuery(periodStart, periodEnd)
                .ToListAsync(ct);
        }

        public async Task<List<SubOrder>> GetEligibleForPayoutByStoresAsync(
            DateTime periodStart,
            DateTime periodEnd,
            IReadOnlyCollection<Guid> storeIds,
            CancellationToken ct = default)
        {
            var uniqueStoreIds = storeIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!uniqueStoreIds.Any())
            {
                return new List<SubOrder>();
            }

            return await BuildEligibleForPayoutQuery(periodStart, periodEnd)
                .Where(o => uniqueStoreIds.Contains(o.StoreId))
                .ToListAsync(ct);
        }

        public async Task BulkUpdatePayoutStatusAsync(
            List<Guid> subOrderIds,
            PayoutStatus status,
            CancellationToken ct = default)
        {
            var subOrders = await _dbSet.Where(o => subOrderIds.Contains(o.Id)).ToListAsync(ct);
            foreach (var subOrder in subOrders)
            {
                subOrder.PayoutStatus = status;
            }
        }

        public async Task<PagedResult<SubOrder>> GetAdminOrdersAsync(AdminOrderQueryDto queryDto)
        {
            var query = _dbSet
                .Include(o => o.Store)
                .Include(o => o.Order)
                .ThenInclude(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
            {
                var search = queryDto.SearchTerm.Trim().ToLower();
                query = query.Where(o => 
                    o.Order.OrderCode.ToString().Contains(search) || 
                    o.Order.ReceiverName.ToLower().Contains(search) ||
                    o.Order.User.UserName.ToLower().Contains(search));
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
                query = query.Where(o => o.Subtotal >= queryDto.MinPrice.Value);
            }

            if (queryDto.MaxPrice.HasValue)
            {
                query = query.Where(o => o.Subtotal <= queryDto.MaxPrice.Value);
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
                        query = isDesc ? query.OrderByDescending(o => o.Order.OrderCode) : query.OrderBy(o => o.Order.OrderCode);
                        break;
                    case "grandtotal":
                        query = isDesc ? query.OrderByDescending(o => o.Subtotal) : query.OrderBy(o => o.Subtotal);
                        break;
                    case "commission":
                        query = isDesc ? query.OrderByDescending(o => o.CommissionAmount) : query.OrderBy(o => o.CommissionAmount);
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

            return new PagedResult<SubOrder>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
        }

        public async Task<AdminOrderStatsDto> GetAdminOrderStatsAsync()
        {
            var stats = new AdminOrderStatsDto();

            stats.TotalOrders = await _dbSet.CountAsync();
            stats.PaidOrders = await _dbSet.CountAsync(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid);
            stats.UnpaidOrders = await _dbSet.CountAsync(o => o.Status == OrderStatus.Pending);
            stats.TotalRevenue = await _dbSet.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid).SumAsync(o => o.Subtotal);

            return stats;
        }

        public async Task<decimal> GetTotalEligiblePayoutAmountAsync(CancellationToken ct = default)
        {
            return await _dbSet
                .Where(x =>
                    (x.Status == OrderStatus.Completed || x.Status == OrderStatus.Delivered) &&
                    x.PayoutStatus == PayoutStatus.Unpaid)
                .SumAsync(x => x.NetAmount, ct);
        }

        public async Task<List<SubOrder>> GetDeliveredOrdersForAutoCompleteAsync(int days, CancellationToken ct = default)
        {
            var threshold = DateTime.UtcNow.AddDays(-days);
            return await _dbSet
                .Where(o => o.Status == OrderStatus.Delivered &&
                            (o.UpdatedAt ?? o.CreatedAt) <= threshold)
                .ToListAsync(ct);
        }

        public async Task<List<ChartDataPoint>> GetRevenueOverviewAsync(DateTime start, DateTime end, string period, CancellationToken ct = default)
        {
            var query = _dbSet
                .Where(so => so.CreatedAt >= start && so.CreatedAt <= end && so.Status != OrderStatus.Cancelled);

            if (period == "year")
            {
                return await query
                    .GroupBy(so => new { so.CreatedAt.Year, so.CreatedAt.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new ChartDataPoint
                    {
                        Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Value = g.Sum(x => x.Subtotal),
                        SecondaryValue = g.Sum(x => x.NetAmount)
                    })
                    .ToListAsync(ct);
            }

            return await query
                .GroupBy(so => so.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key.ToString("dd MMM"),
                    Value = g.Sum(x => x.Subtotal),
                    SecondaryValue = g.Sum(x => x.NetAmount)
                })
                .ToListAsync(ct);
        }

        public async Task<List<CategoryRevenueDto>> GetCategoryBreakdownAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            var data = await _context.OrderItems
                .Include(oi => oi.Variant).ThenInclude(v => v.Product).ThenInclude(p => p.Category)
                .Where(oi => oi.SubOrder.CreatedAt >= start && oi.SubOrder.CreatedAt <= end && oi.SubOrder.Status != OrderStatus.Cancelled)
                .GroupBy(oi => oi.Variant.Product.Category.Name)
                .Select(g => new CategoryRevenueDto
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync(ct);

            var total = data.Sum(x => x.Revenue);
            if (total > 0)
            {
                foreach (var item in data)
                {
                    item.Percentage = (double)(item.Revenue / total * 100);
                }
            }

            return data;
        }

        public async Task<List<OrderStatusBreakdownDto>> GetOrderStatusBreakdownAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            var groups = await _dbSet
                .Where(so => so.CreatedAt >= start && so.CreatedAt <= end)
                .GroupBy(so => so.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(ct);

            var total = groups.Sum(x => x.Count);
            return groups.Select(g => new OrderStatusBreakdownDto
            {
                Status = g.Status.ToString(),
                Count = g.Count,
                Percentage = total > 0 ? (double)g.Count / total * 100 : 0,
                ColorClass = GetStatusColor(g.Status)
            }).ToList();
        }

        public async Task<List<DashboardStoreDto>> GetTopStoresAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(so => so.Store)
                .Where(so => so.CreatedAt >= start && so.CreatedAt <= end && so.Status != OrderStatus.Cancelled)
                .GroupBy(so => new { so.StoreId, so.Store.StoreName, so.Store.LogoUrl, so.Store.CommissionRate, so.Store.Status })
                .Select(g => new DashboardStoreDto
                {
                    StoreId = g.Key.StoreId,
                    StoreName = g.Key.StoreName,
                    LogoUrl = g.Key.LogoUrl,
                    Revenue = g.Sum(x => x.Subtotal),
                    Orders = g.Count(),
                    Commission = g.Sum(x => x.CommissionAmount),
                    Status = g.Key.Status.ToString(),
                    Rating = 4.8 // Placeholder rating
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync(ct);
        }

        public async Task<decimal> GetGrossRevenueAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(so => so.CreatedAt >= start && so.CreatedAt <= end && so.Status != OrderStatus.Cancelled)
                .SumAsync(so => (decimal?)so.Subtotal, ct) ?? 0;
        }

        public async Task<int> GetTotalOrdersCountAsync(DateTime start, DateTime end, CancellationToken ct = default)
        {
            return await _dbSet
                .CountAsync(so => so.CreatedAt >= start && so.CreatedAt <= end, ct);
        }

        private string GetStatusColor(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Completed => "bg-primary",
                OrderStatus.Processing => "bg-amber-400",
                OrderStatus.Cancelled => "bg-slate-200",
                _ => "bg-slate-400"
            };
        }

        private IQueryable<SubOrder> BuildEligibleForPayoutQuery(
            DateTime periodStart,
            DateTime periodEnd)
        {
            return _dbSet
                .Include(o => o.Items)
                .Include(o => o.Store)
                .Where(o => (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered) &&
                            o.PayoutStatus == PayoutStatus.Unpaid &&
                            o.Order.Payments.Any(p => p.Status == PaymentStatus.Paid &&
                                                      p.Method != PaymentMethod.COD) &&
                            (o.DeliveredAt ?? o.UpdatedAt ?? o.CreatedAt) >= periodStart &&
                            (o.DeliveredAt ?? o.UpdatedAt ?? o.CreatedAt) <= periodEnd);
        }

        public async Task<PagedResult<UserOrderDto>> GetUserOrdersAsync(string userId, UserOrderQueryDto queryDto, DateTime utcNow, CancellationToken ct = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.Order.UserId == userId);

            if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
            {
                var search = queryDto.SearchTerm.Trim().ToLower();
                query = query.Where(x =>
                    x.Order.OrderCode.ToString().Contains(search) ||
                    x.Items.Any(item => item.ProductNameSnapshot.ToLower().Contains(search)));
            }

            var reviewWindowStart = utcNow.AddDays(-7);
            var status = (queryDto.Status ?? "all").Trim().ToLowerInvariant();
            query = status switch
            {
                "processing" => query.Where(x =>
                    x.Status == OrderStatus.Pending ||
                    x.Status == OrderStatus.Approved ||
                    x.Status == OrderStatus.Paid ||
                    x.Status == OrderStatus.Processing),
                "delivered" => query.Where(x => x.Status == OrderStatus.Delivered),
                "cancelled" => query.Where(x =>
                    x.Status == OrderStatus.Cancelled ||
                    x.Status == OrderStatus.Refunded ||
                    x.Status == OrderStatus.Rejected),
                "to_review" => query.Where(x =>
                    x.Status == OrderStatus.Delivered &&
                    (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart &&
                    x.Items.Any(item => !_context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted))),
                _ => query
            };

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .Select(x => new UserOrderDto
                {
                    SubOrderId = x.Id,
                    OrderId = x.OrderId,
                    StoreId = x.StoreId,
                    StoreName = x.Store.StoreName,
                    StoreSlug = x.Store.Slug,
                    OrderCode = x.Order.OrderCode,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    DeliveredAt = x.DeliveredAt,
                    Subtotal = x.Subtotal,
                    HasAnyReviewableItem = x.Status == OrderStatus.Delivered
                        && (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart
                        && x.Items.Any(item => !_context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted)),
                    HasAnyEditableReview = x.Status == OrderStatus.Delivered
                        && (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart
                        && x.Items.Any(item => _context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted)),
                    Items = x.Items
                        .OrderBy(item => item.ProductNameSnapshot)
                        .Select(item => new UserOrderItemDto
                        {
                            OrderItemId = item.Id,
                            ProductId = item.Variant.ProductId,
                            ProductName = item.ProductNameSnapshot,
                            ProductSlug = item.Variant.Product.Slug,
                            ProductImageUrl = item.Variant.Product.Images
                                .Where(img => img.IsPrimary)
                                .Select(img => img.ImageUrl)
                                .FirstOrDefault()
                                ?? item.Variant.Product.Images.Select(img => img.ImageUrl).FirstOrDefault(),
                            VariantName = item.VariantNameSnapshot,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPriceSnapshot,
                            CanReview = x.Status == OrderStatus.Delivered
                                && (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart
                                && !_context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted),
                            CanEditReview = x.Status == OrderStatus.Delivered
                                && (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart
                                && _context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted),
                            ReviewId = _context.ProductReviews
                                .Where(review => review.OrderItemId == item.Id && !review.IsDeleted)
                                .Select(review => (Guid?)review.Id)
                                .FirstOrDefault(),
                            ReviewDeadline = (x.Status == OrderStatus.Delivered
                                ? (DateTime?)((x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt).AddDays(7))
                                : null)
                        })
                        .ToList()
                })
                .ToListAsync(ct);

            return new PagedResult<UserOrderDto>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
        }

        public async Task<UserOrderStatusSummaryDto> GetUserOrderStatusSummaryAsync(string userId, DateTime utcNow, CancellationToken ct = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.Order.UserId == userId);

            var reviewWindowStart = utcNow.AddDays(-7);

            return new UserOrderStatusSummaryDto
            {
                All = await query.CountAsync(ct),
                Processing = await query.CountAsync(x =>
                    x.Status == OrderStatus.Pending ||
                    x.Status == OrderStatus.Approved ||
                    x.Status == OrderStatus.Paid ||
                    x.Status == OrderStatus.Processing, ct),
                Delivered = await query.CountAsync(x => x.Status == OrderStatus.Delivered, ct),
                Cancelled = await query.CountAsync(x =>
                    x.Status == OrderStatus.Cancelled ||
                    x.Status == OrderStatus.Refunded ||
                    x.Status == OrderStatus.Rejected, ct),
                ToReview = await query.CountAsync(x =>
                    x.Status == OrderStatus.Delivered &&
                    (x.DeliveredAt ?? x.UpdatedAt ?? x.CreatedAt) >= reviewWindowStart &&
                    x.Items.Any(item => !_context.ProductReviews.Any(review => review.OrderItemId == item.Id && !review.IsDeleted)), ct)
            };
        }

        public async Task<PagedResult<SellerChatOrderListItemDto>> GetSellerChatOrdersAsync(string ownerUserId, SellerChatOrderQueryDto queryDto, CancellationToken ct = default)
        {
            queryDto ??= new SellerChatOrderQueryDto();
            queryDto.PageNumber = queryDto.PageNumber < 1 ? 1 : queryDto.PageNumber;
            queryDto.PageSize = queryDto.PageSize < 1 ? 10 : queryDto.PageSize;

            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.Store.OwnerUserId == ownerUserId);

            if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
            {
                var search = queryDto.SearchTerm.Trim().ToLower();
                query = query.Where(x =>
                    x.Order.OrderCode.ToString().Contains(search) ||
                    (x.Order.User.FullName != null && x.Order.User.FullName.ToLower().Contains(search)) ||
                    (x.Order.User.UserName != null && x.Order.User.UserName.ToLower().Contains(search)) ||
                    (x.Order.User.Email != null && x.Order.User.Email.ToLower().Contains(search)) ||
                    x.Items.Any(item => item.ProductNameSnapshot.ToLower().Contains(search)));
            }

            if (queryDto.Status.HasValue)
            {
                query = query.Where(x => x.Status == queryDto.Status.Value);
            }

            if (queryDto.MinSubtotal.HasValue)
            {
                query = query.Where(x => x.Subtotal >= queryDto.MinSubtotal.Value);
            }

            if (queryDto.MaxSubtotal.HasValue)
            {
                query = query.Where(x => x.Subtotal <= queryDto.MaxSubtotal.Value);
            }

            if (queryDto.StartDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= queryDto.StartDate.Value);
            }

            if (queryDto.EndDate.HasValue)
            {
                var endOfDay = queryDto.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreatedAt <= endOfDay);
            }

            var sortBy = (queryDto.SortBy ?? "createdAt").Trim().ToLowerInvariant();
            var sortDirection = (queryDto.SortDirection ?? "desc").Trim().ToLowerInvariant();
            var isAsc = sortDirection == "asc";

            query = sortBy switch
            {
                "ordercode" => isAsc ? query.OrderBy(x => x.Order.OrderCode) : query.OrderByDescending(x => x.Order.OrderCode),
                "buyer" => isAsc ? query.OrderBy(x => x.Order.User.FullName ?? x.Order.User.UserName ?? x.Order.User.Email) : query.OrderByDescending(x => x.Order.User.FullName ?? x.Order.User.UserName ?? x.Order.User.Email),
                "subtotal" => isAsc ? query.OrderBy(x => x.Subtotal) : query.OrderByDescending(x => x.Subtotal),
                "status" => isAsc ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
                _ => isAsc ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt)
            };

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .Select(x => new SellerChatOrderListItemDto
                {
                    SubOrderId = x.Id,
                    OrderCode = x.Order.OrderCode,
                    StoreId = x.StoreId,
                    StoreName = x.Store.StoreName,
                    BuyerUserId = x.Order.UserId,
                    BuyerDisplayName = x.Order.User.FullName ?? x.Order.User.UserName ?? x.Order.User.Email ?? "Buyer",
                    BuyerAvatarUrl = x.Order.User.AvatarUrl,
                    CreatedAt = x.CreatedAt,
                    DeliveredAt = x.DeliveredAt,
                    Status = x.Status,
                    Subtotal = x.Subtotal,
                    ItemCount = x.Items.Count,
                    ProductPreview = x.Items
                        .OrderBy(item => item.ProductNameSnapshot)
                        .Select(item => item.ProductNameSnapshot)
                        .FirstOrDefault() ?? "Order items"
                })
                .ToListAsync(ct);

            return new PagedResult<SellerChatOrderListItemDto>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
        }

        public async Task<SubOrder?> GetSellerChatSubOrderAsync(string ownerUserId, Guid subOrderId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(x => x.Order)
                .Include(x => x.Order.User)
                .Include(x => x.Store)
                .FirstOrDefaultAsync(x => x.Id == subOrderId && x.Store.OwnerUserId == ownerUserId, ct);
        }

        public async Task<SellerChatOrderDetailDto?> GetSellerChatOrderDetailAsync(string ownerUserId, Guid subOrderId, CancellationToken ct = default)
        {
            var subOrder = await _dbSet
                .AsNoTracking()
                .Include(x => x.Order)
                .ThenInclude(o => o.User)
                .Include(x => x.Store)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == subOrderId && x.Store.OwnerUserId == ownerUserId, ct);

            if (subOrder == null)
            {
                return null;
            }

            var statusHistory = await _context.OrderStatusHistories
                .AsNoTracking()
                .Where(x => x.OrderId == subOrder.OrderId)
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new SellerChatOrderStatusHistoryDto
                {
                    ChangedAt = x.ChangedAt,
                    OldStatus = x.OldStatus,
                    NewStatus = x.NewStatus,
                    ChangedByDisplayName = x.ChangedByUser.FullName ?? x.ChangedByUser.UserName ?? x.ChangedByUser.Email ?? "System",
                    Note = x.Note
                })
                .ToListAsync(ct);

            return new SellerChatOrderDetailDto
            {
                SubOrderId = subOrder.Id,
                OrderCode = subOrder.Order.OrderCode,
                StoreId = subOrder.StoreId,
                StoreName = subOrder.Store.StoreName,
                BuyerUserId = subOrder.Order.UserId,
                BuyerDisplayName = subOrder.Order.User.FullName ?? subOrder.Order.User.UserName ?? subOrder.Order.User.Email ?? "Buyer",
                BuyerAvatarUrl = subOrder.Order.User.AvatarUrl,
                BuyerEmail = subOrder.Order.User.Email,
                CreatedAt = subOrder.CreatedAt,
                DeliveredAt = subOrder.DeliveredAt,
                UpdatedAt = subOrder.UpdatedAt,
                Status = subOrder.Status,
                Subtotal = subOrder.Subtotal,
                ShippingFee = subOrder.Order.ShippingFee,
                GrandTotal = subOrder.Order.GrandTotal,
                CommissionRateSnapshot = subOrder.CommissionRateSnapshot,
                CommissionAmount = subOrder.CommissionAmount,
                NetAmount = subOrder.NetAmount,
                ReceiverName = subOrder.Order.ReceiverName,
                ReceiverPhone = subOrder.Order.ReceiverPhone,
                ShippingAddress = subOrder.Order.ShippingAddress,
                ShippingProvider = subOrder.Order.ShippingProvider,
                TrackingNumber = subOrder.Order.TrackingNumber,
                Items = subOrder.Items
                    .OrderBy(x => x.ProductNameSnapshot)
                    .ThenBy(x => x.SkuSnapshot)
                    .Select(x => new SellerChatOrderItemDetailDto
                    {
                        OrderItemId = x.Id,
                        VariantId = x.VariantId,
                        ProductName = x.ProductNameSnapshot,
                        VariantName = x.VariantNameSnapshot,
                        Sku = x.SkuSnapshot,
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPriceSnapshot,
                        LineTotal = x.LineTotal
                    })
                    .ToList(),
                StatusHistory = statusHistory
            };
        }

        public async Task<List<ChatContextOrderDto>> GetConversationOrderContextAsync(string buyerUserId, Guid storeId, int take, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.Order.UserId == buyerUserId && x.StoreId == storeId)
                .OrderByDescending(x => x.DeliveredAt ?? x.CreatedAt)
                .Take(take)
                .Select(x => new ChatContextOrderDto
                {
                    SubOrderId = x.Id,
                    OrderCode = x.Order.OrderCode,
                    CreatedAt = x.CreatedAt,
                    DeliveredAt = x.DeliveredAt,
                    Status = x.Status,
                    Subtotal = x.Subtotal,
                    ItemCount = x.Items.Count,
                    ProductPreview = x.Items
                        .OrderBy(item => item.ProductNameSnapshot)
                        .Select(item => item.ProductNameSnapshot)
                        .FirstOrDefault() ?? "Order items"
                })
                .ToListAsync(ct);
        }
    }
}
