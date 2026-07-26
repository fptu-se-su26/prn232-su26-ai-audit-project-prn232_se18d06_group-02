using System.Text.Json;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GearZone.Application.Features.Admin;

public sealed class AdminReportService : IAdminReportService
{
    private static readonly OrderStatus[] PaidLikeStatuses =
        [OrderStatus.Paid, OrderStatus.Processing, OrderStatus.Delivered, OrderStatus.Completed];

    private readonly ISubOrderRepository _subOrders;
    private readonly IOrderItemRepository _orderItems;
    private readonly IStoreRepository _stores;
    private readonly IPaymentRepository _payments;
    private readonly IProductReviewRepository _reviews;
    private readonly IMemoryCache _cache;

    public AdminReportService(
        ISubOrderRepository subOrders,
        IOrderItemRepository orderItems,
        IStoreRepository stores,
        IPaymentRepository payments,
        IProductReviewRepository reviews,
        IMemoryCache cache)
    {
        _subOrders = subOrders;
        _orderItems = orderItems;
        _stores = stores;
        _payments = payments;
        _reviews = reviews;
        _cache = cache;
    }

    public async Task<AdminOverviewReportDto> GetOverviewAsync(AdminReportQueryDto query, CancellationToken ct = default)
    {
        var period = AdminReportPeriodResolver.Resolve(query);
        var key = CacheKey("overview", query, period);
        if (_cache.TryGetValue(key, out AdminOverviewReportDto? cached) && cached is not null)
            return cached;

        var rows = await BaseSubOrderQuery(period)
            .Where(x => PaidLikeStatuses.Contains(x.Status))
            .Select(x => new SubOrderRow(
                x.Id, x.OrderId, x.StoreId, x.CreatedAt, x.Status, x.Subtotal,
                x.CommissionAmount, x.NetAmount, x.DeliveredAt, x.Order.UserId,
                x.Order.OrderCode, x.Order.User.FullName ?? x.Order.User.UserName ?? "Customer",
                x.Order.PaidAt))
            .ToListAsync(ct);

        var itemRows = await _orderItems.Query()
            .AsNoTracking()
            .Where(x => x.SubOrder.CreatedAt >= period.PreviousStartUtc &&
                        x.SubOrder.CreatedAt < period.EndExclusiveUtc &&
                        PaidLikeStatuses.Contains(x.SubOrder.Status))
            .Select(x => new ItemRow(
                x.SubOrderId, x.SubOrder.OrderId, x.SubOrder.StoreId, x.SubOrder.CreatedAt,
                x.Quantity, x.LineTotal, x.Variant.Product.CategoryId, x.Variant.Product.Category.Name))
            .ToListAsync(ct);

        var current = rows.Where(x => IsCurrent(x.CreatedAt, period)).ToList();
        var previous = rows.Where(x => IsPrevious(x.CreatedAt, period)).ToList();
        var currentItems = itemRows.Where(x => IsCurrent(x.CreatedAt, period)).ToList();
        var previousItems = itemRows.Where(x => IsPrevious(x.CreatedAt, period)).ToList();

        var dto = new AdminOverviewReportDto
        {
            Period = period.ToDto(),
            PaidGmv = ComparisonMetricDto.From(current.Sum(x => x.Subtotal), previous.Sum(x => x.Subtotal)),
            PlatformCommission = ComparisonMetricDto.From(current.Sum(x => x.Commission), previous.Sum(x => x.Commission)),
            SellerNetAmount = ComparisonMetricDto.From(current.Sum(x => x.NetAmount), previous.Sum(x => x.NetAmount)),
            Orders = ComparisonMetricDto.From(current.Select(x => x.OrderId).Distinct().Count(), previous.Select(x => x.OrderId).Distinct().Count()),
            UnitsSold = ComparisonMetricDto.From(currentItems.Sum(x => x.Quantity), previousItems.Sum(x => x.Quantity)),
            UniqueBuyers = ComparisonMetricDto.From(current.Select(x => x.UserId).Distinct().Count(), previous.Select(x => x.UserId).Distinct().Count()),
            ActiveSellers = ComparisonMetricDto.From(current.Select(x => x.StoreId).Distinct().Count(), previous.Select(x => x.StoreId).Distinct().Count())
        };
        dto.AverageOrderValue = ComparisonMetricDto.From(
            SafeDivide(dto.PaidGmv.Current, dto.Orders.Current),
            SafeDivide(dto.PaidGmv.Previous, dto.Orders.Previous));
        dto.Trend = BuildTrend(period, current, paidOnly: true);

        var allCategoryGroups = currentItems
            .GroupBy(x => new { x.CategoryId, x.CategoryName })
            .Select(g => new AdminCategoryRevenueDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                Revenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();
        var categoryTotal = allCategoryGroups.Sum(x => x.Revenue);
        var categoryGroups = allCategoryGroups.Take(10).ToList();
        foreach (var item in categoryGroups)
            item.Percentage = SafePercent(item.Revenue, categoryTotal);
        dto.RevenueByCategory = categoryGroups;

        _cache.Set(key, dto, TimeSpan.FromMinutes(5));
        return dto;
    }

    public async Task<AdminOrderReportDto> GetOrdersAsync(AdminReportQueryDto query, CancellationToken ct = default)
    {
        var period = AdminReportPeriodResolver.Resolve(query);
        var key = CacheKey("orders", query, period);
        if (_cache.TryGetValue(key, out AdminOrderReportDto? cached) && cached is not null)
            return cached;

        var rows = await BaseSubOrderQuery(period)
            .Select(x => new SubOrderRow(
                x.Id, x.OrderId, x.StoreId, x.CreatedAt, x.Status, x.Subtotal,
                x.CommissionAmount, x.NetAmount, x.DeliveredAt, x.Order.UserId,
                x.Order.OrderCode, x.Order.User.FullName ?? x.Order.User.UserName ?? "Customer",
                x.Order.PaidAt))
            .ToListAsync(ct);

        var current = rows.Where(x => IsCurrent(x.CreatedAt, period)).ToList();
        var previous = rows.Where(x => IsPrevious(x.CreatedAt, period)).ToList();
        var currentPaid = current.Where(x => PaidLikeStatuses.Contains(x.Status)).ToList();
        var previousPaid = previous.Where(x => PaidLikeStatuses.Contains(x.Status)).ToList();

        var paymentRows = await _payments.Query()
            .AsNoTracking()
            .Where(x => x.Order.CreatedAt >= period.StartUtc && x.Order.CreatedAt < period.EndExclusiveUtc)
            .Select(x => new { x.OrderId, x.Method, x.Amount, x.CreatedAt })
            .ToListAsync(ct);
        var latestPayments = paymentRows
            .GroupBy(x => x.OrderId)
            .Select(g => g.OrderByDescending(x => x.CreatedAt).First())
            .GroupBy(x => x.Method.ToString())
            .Select(g => new AdminPaymentMethodBreakdownDto
            {
                Method = g.Key,
                Count = g.Count(),
                Amount = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var dto = new AdminOrderReportDto
        {
            Period = period.ToDto(),
            Orders = ComparisonMetricDto.From(current.Select(x => x.OrderId).Distinct().Count(), previous.Select(x => x.OrderId).Distinct().Count()),
            SubOrders = ComparisonMetricDto.From(current.Count, previous.Count),
            PaidSubOrders = ComparisonMetricDto.From(currentPaid.Count, previousPaid.Count),
            CompletionRate = StatusRate(current, OrderStatus.Completed),
            CancellationRate = StatusRate(current, OrderStatus.Cancelled),
            RejectionRate = StatusRate(current, OrderStatus.Rejected),
            RefundRate = StatusRate(current, OrderStatus.Refunded),
            AverageFulfillmentHours = AverageFulfillmentHours(current),
            Trend = BuildTrend(period, current, paidOnly: false),
            PaymentMethods = latestPayments,
            HighValueOrders = currentPaid
                .GroupBy(x => new { x.OrderId, x.OrderCode, x.CustomerName, x.PaidAt })
                .Select(g => new AdminHighValueOrderDto
                {
                    OrderId = g.Key.OrderId,
                    OrderCode = g.Key.OrderCode,
                    CustomerName = g.Key.CustomerName,
                    PaidGmv = g.Sum(x => x.Subtotal),
                    StoreCount = g.Select(x => x.StoreId).Distinct().Count(),
                    CreatedAt = g.Min(x => x.CreatedAt),
                    PaidAt = g.Key.PaidAt
                })
                .OrderByDescending(x => x.PaidGmv)
                .Take(10)
                .ToList()
        };

        dto.StatusBreakdown = current
            .GroupBy(x => x.Status)
            .Select(g => new AdminStatusBreakdownDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                Percentage = SafePercent(g.Count(), current.Count)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        _cache.Set(key, dto, TimeSpan.FromMinutes(5));
        return dto;
    }

    public async Task<AdminSellerReportDto> GetSellersAsync(
        AdminReportQueryDto query,
        bool exportAll = false,
        CancellationToken ct = default)
    {
        var period = AdminReportPeriodResolver.Resolve(query);
        NormalizeSellerPaging(query);
        var key = CacheKey(exportAll ? "sellers-export" : "sellers", query, period);
        if (_cache.TryGetValue(key, out AdminSellerReportDto? cached) && cached is not null)
            return cached;

        var storesQuery = _stores.Query().AsNoTracking();
        if (!string.Equals(query.StoreStatus, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse<StoreStatus>(query.StoreStatus, true, out var status))
                throw new ArgumentException($"Unsupported store status '{query.StoreStatus}'.");
            storesQuery = storesQuery.Where(x => x.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            storesQuery = storesQuery.Where(x => x.StoreName.ToLower().Contains(search));
        }

        var storeRows = await storesQuery
            .Select(x => new StoreRow(x.Id, x.StoreName, x.Status, x.ApprovedAt))
            .ToListAsync(ct);
        var storeIds = storeRows.Select(x => x.Id).ToList();

        var subOrderRows = await BaseSubOrderQuery(period)
            .Where(x => storeIds.Contains(x.StoreId))
            .Select(x => new SubOrderRow(
                x.Id, x.OrderId, x.StoreId, x.CreatedAt, x.Status, x.Subtotal,
                x.CommissionAmount, x.NetAmount, x.DeliveredAt, x.Order.UserId,
                x.Order.OrderCode, string.Empty, x.Order.PaidAt))
            .ToListAsync(ct);

        var itemRows = await _orderItems.Query()
            .AsNoTracking()
            .Where(x => storeIds.Contains(x.SubOrder.StoreId) &&
                        x.SubOrder.CreatedAt >= period.PreviousStartUtc &&
                        x.SubOrder.CreatedAt < period.EndExclusiveUtc &&
                        PaidLikeStatuses.Contains(x.SubOrder.Status))
            .Select(x => new { x.SubOrder.StoreId, x.SubOrder.CreatedAt, x.Quantity })
            .ToListAsync(ct);

        var ratings = await _reviews.Query()
            .AsNoTracking()
            .Where(x => storeIds.Contains(x.StoreId) && !x.IsDeleted)
            .GroupBy(x => x.StoreId)
            .Select(g => new { StoreId = g.Key, Sum = g.Sum(x => x.Rating), Count = g.Count() })
            .ToDictionaryAsync(x => x.StoreId, x => SafeDivide(x.Sum, x.Count), ct);

        var allPerformance = new List<AdminSellerPerformanceDto>(storeRows.Count);
        foreach (var store in storeRows)
        {
            var currentAll = subOrderRows.Where(x => x.StoreId == store.Id && IsCurrent(x.CreatedAt, period)).ToList();
            var currentPaid = currentAll.Where(x => PaidLikeStatuses.Contains(x.Status)).ToList();
            var previousPaid = subOrderRows.Where(x => x.StoreId == store.Id && IsPrevious(x.CreatedAt, period) && PaidLikeStatuses.Contains(x.Status)).ToList();
            var currentGmv = currentPaid.Sum(x => x.Subtotal);
            var previousGmv = previousPaid.Sum(x => x.Subtotal);
            var orderCount = currentPaid.Select(x => x.OrderId).Distinct().Count();
            allPerformance.Add(new AdminSellerPerformanceDto
            {
                StoreId = store.Id,
                StoreName = store.Name,
                Status = store.Status.ToString(),
                PaidGmv = currentGmv,
                PreviousGmv = previousGmv,
                GrowthPct = ComparisonMetricDto.From(currentGmv, previousGmv).ChangePct,
                Commission = currentPaid.Sum(x => x.Commission),
                SellerNetAmount = currentPaid.Sum(x => x.NetAmount),
                Orders = orderCount,
                Units = itemRows.Where(x => x.StoreId == store.Id && IsCurrent(x.CreatedAt, period)).Sum(x => x.Quantity),
                AverageOrderValue = SafeDivide(currentGmv, orderCount),
                CancellationRate = StatusRate(currentAll, OrderStatus.Cancelled),
                RefundRate = StatusRate(currentAll, OrderStatus.Refunded),
                AverageRating = ratings.TryGetValue(store.Id, out var rating) ? Math.Round(rating, 1) : 0m
            });
        }

        allPerformance = SortSellers(allPerformance, query.SortBy, query.SortDirection);
        var totalCount = allPerformance.Count;
        var paged = exportAll
            ? allPerformance
            : allPerformance.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        var currentPaidAll = subOrderRows.Where(x => IsCurrent(x.CreatedAt, period) && PaidLikeStatuses.Contains(x.Status)).ToList();
        var previousPaidAll = subOrderRows.Where(x => IsPrevious(x.CreatedAt, period) && PaidLikeStatuses.Contains(x.Status)).ToList();
        var currentApproved = storeRows.Count(x => x.ApprovedAt >= period.StartUtc && x.ApprovedAt < period.EndExclusiveUtc);
        var previousApproved = storeRows.Count(x => x.ApprovedAt >= period.PreviousStartUtc && x.ApprovedAt < period.PreviousEndExclusiveUtc);
        var dto = new AdminSellerReportDto
        {
            Period = period.ToDto(),
            ActiveSellers = ComparisonMetricDto.From(currentPaidAll.Select(x => x.StoreId).Distinct().Count(), previousPaidAll.Select(x => x.StoreId).Distinct().Count()),
            NewApprovedSellers = ComparisonMetricDto.From(currentApproved, previousApproved),
            PaidGmv = ComparisonMetricDto.From(currentPaidAll.Sum(x => x.Subtotal), previousPaidAll.Sum(x => x.Subtotal)),
            PlatformCommission = ComparisonMetricDto.From(currentPaidAll.Sum(x => x.Commission), previousPaidAll.Sum(x => x.Commission)),
            SellerNetAmount = ComparisonMetricDto.From(currentPaidAll.Sum(x => x.NetAmount), previousPaidAll.Sum(x => x.NetAmount)),
            Sellers = new PagedResult<AdminSellerPerformanceDto>(paged, totalCount, exportAll ? 1 : query.PageNumber, exportAll ? Math.Max(totalCount, 1) : query.PageSize)
        };

        _cache.Set(key, dto, TimeSpan.FromMinutes(5));
        return dto;
    }

    public async Task<object> GetInsightSnapshotAsync(string reportType, AdminReportQueryDto query, CancellationToken ct = default)
    {
        return reportType.Trim().ToLowerInvariant() switch
        {
            "overview" => ToOverviewSnapshot(await GetOverviewAsync(query, ct)),
            "orders" => ToOrdersSnapshot(await GetOrdersAsync(query, ct)),
            "sellers" => ToSellerSnapshot(await GetSellersAsync(query, false, ct)),
            _ => throw new ArgumentException($"Unsupported report type '{reportType}'.")
        };
    }

    private IQueryable<Domain.Entities.SubOrder> BaseSubOrderQuery(ResolvedAdminReportPeriod period) =>
        _subOrders.Query()
            .AsNoTracking()
            .Where(x => x.CreatedAt >= period.PreviousStartUtc && x.CreatedAt < period.EndExclusiveUtc);

    private static List<AdminReportSeriesPointDto> BuildTrend(
        ResolvedAdminReportPeriod period,
        List<SubOrderRow> rows,
        bool paidOnly)
    {
        var source = paidOnly ? rows.Where(x => PaidLikeStatuses.Contains(x.Status)).ToList() : rows;
        return AdminReportPeriodResolver.BuildBuckets(period)
            .Select(bucket =>
            {
                var bucketRows = source.Where(x =>
                {
                    var local = AdminReportPeriodResolver.ToLocal(x.CreatedAt, period.TimeZone);
                    return local >= bucket.StartLocal && local < bucket.EndExclusiveLocal;
                }).ToList();
                var paidRows = bucketRows.Where(x => PaidLikeStatuses.Contains(x.Status)).ToList();
                return new AdminReportSeriesPointDto
                {
                    Date = bucket.StartLocal,
                    Label = bucket.Label,
                    Gmv = paidRows.Sum(x => x.Subtotal),
                    Commission = paidRows.Sum(x => x.Commission),
                    Orders = bucketRows.Select(x => x.OrderId).Distinct().Count(),
                    SubOrders = bucketRows.Count
                };
            })
            .ToList();
    }

    private static List<AdminSellerPerformanceDto> SortSellers(
        List<AdminSellerPerformanceDto> rows,
        string sortBy,
        string direction)
    {
        var desc = !string.Equals(direction, "asc", StringComparison.OrdinalIgnoreCase);
        sortBy = (sortBy ?? "revenue").Trim().ToLowerInvariant();
        if (sortBy is not ("revenue" or "orders" or "growth" or "cancelrate" or "refundrate" or "rating"))
            throw new ArgumentException($"Unsupported seller sort '{sortBy}'.");

        Func<AdminSellerPerformanceDto, decimal> selector = sortBy switch
        {
            "orders" => x => x.Orders,
            "growth" => x => x.GrowthPct ?? decimal.MinValue,
            "cancelrate" => x => x.CancellationRate,
            "refundrate" => x => x.RefundRate,
            "rating" => x => x.AverageRating,
            _ => x => x.PaidGmv
        };
        return (desc ? rows.OrderByDescending(selector) : rows.OrderBy(selector))
            .ThenBy(x => x.StoreName)
            .ToList();
    }

    private static object ToOverviewSnapshot(AdminOverviewReportDto x) => new
    {
        x.Period,
        metrics = new Dictionary<string, decimal?>
        {
            ["paidGmv"] = x.PaidGmv.Current,
            ["paidGmvChangePct"] = x.PaidGmv.ChangePct,
            ["platformCommission"] = x.PlatformCommission.Current,
            ["sellerNetAmount"] = x.SellerNetAmount.Current,
            ["orders"] = x.Orders.Current,
            ["ordersChangePct"] = x.Orders.ChangePct,
            ["unitsSold"] = x.UnitsSold.Current,
            ["averageOrderValue"] = x.AverageOrderValue.Current,
            ["uniqueBuyers"] = x.UniqueBuyers.Current,
            ["activeSellers"] = x.ActiveSellers.Current
        },
        trend = x.Trend,
        categories = x.RevenueByCategory
    };

    private static object ToOrdersSnapshot(AdminOrderReportDto x) => new
    {
        x.Period,
        metrics = new Dictionary<string, decimal?>
        {
            ["orders"] = x.Orders.Current,
            ["ordersChangePct"] = x.Orders.ChangePct,
            ["subOrders"] = x.SubOrders.Current,
            ["paidSubOrders"] = x.PaidSubOrders.Current,
            ["completionRate"] = x.CompletionRate,
            ["cancellationRate"] = x.CancellationRate,
            ["rejectionRate"] = x.RejectionRate,
            ["refundRate"] = x.RefundRate,
            ["averageFulfillmentHours"] = x.AverageFulfillmentHours
        },
        trend = x.Trend,
        statuses = x.StatusBreakdown,
        paymentMethods = x.PaymentMethods
    };

    private static object ToSellerSnapshot(AdminSellerReportDto x) => new
    {
        x.Period,
        metrics = new Dictionary<string, decimal?>
        {
            ["activeSellers"] = x.ActiveSellers.Current,
            ["newApprovedSellers"] = x.NewApprovedSellers.Current,
            ["paidGmv"] = x.PaidGmv.Current,
            ["paidGmvChangePct"] = x.PaidGmv.ChangePct,
            ["platformCommission"] = x.PlatformCommission.Current,
            ["sellerNetAmount"] = x.SellerNetAmount.Current
        },
        sellers = x.Sellers.Items.Take(20).Select(s => new
        {
            s.StoreName,
            s.Status,
            s.PaidGmv,
            s.GrowthPct,
            s.Orders,
            s.Units,
            s.CancellationRate,
            s.RefundRate,
            s.AverageRating
        })
    };

    private static bool IsCurrent(DateTime value, ResolvedAdminReportPeriod p) =>
        value >= p.StartUtc && value < p.EndExclusiveUtc;

    private static bool IsPrevious(DateTime value, ResolvedAdminReportPeriod p) =>
        value >= p.PreviousStartUtc && value < p.PreviousEndExclusiveUtc;

    private static decimal SafeDivide(decimal numerator, decimal denominator) =>
        denominator == 0m ? 0m : Math.Round(numerator / denominator, 2);

    private static decimal SafePercent(decimal numerator, decimal denominator) =>
        denominator == 0m ? 0m : Math.Round(numerator / denominator * 100m, 2);

    private static decimal StatusRate(List<SubOrderRow> rows, OrderStatus status) =>
        SafePercent(rows.Count(x => x.Status == status), rows.Count);

    private static decimal? AverageFulfillmentHours(List<SubOrderRow> rows)
    {
        var values = rows
            .Where(x => x.DeliveredAt.HasValue && x.DeliveredAt.Value >= x.CreatedAt)
            .Select(x => (decimal)(x.DeliveredAt!.Value - x.CreatedAt).TotalHours)
            .ToList();
        return values.Count == 0 ? null : Math.Round(values.Average(), 2);
    }

    private static void NormalizeSellerPaging(AdminReportQueryDto query)
    {
        query.PageNumber = Math.Max(1, query.PageNumber);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        query.SortDirection = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        query.StoreStatus = string.IsNullOrWhiteSpace(query.StoreStatus) ? "Approved" : query.StoreStatus.Trim();
    }

    private static string CacheKey(string type, AdminReportQueryDto q, ResolvedAdminReportPeriod p) =>
        $"admin-report:{type}:{p.StartUtc:O}:{p.EndExclusiveUtc:O}:{p.Granularity}:" +
        $"{q.Search?.Trim().ToLowerInvariant()}:{q.StoreStatus?.ToLowerInvariant()}:" +
        $"{q.SortBy?.ToLowerInvariant()}:{q.SortDirection?.ToLowerInvariant()}:{q.PageNumber}:{q.PageSize}";

    private sealed record SubOrderRow(
        Guid Id,
        Guid OrderId,
        Guid StoreId,
        DateTime CreatedAt,
        OrderStatus Status,
        decimal Subtotal,
        decimal Commission,
        decimal NetAmount,
        DateTime? DeliveredAt,
        string UserId,
        long OrderCode,
        string CustomerName,
        DateTime? PaidAt);

    private sealed record ItemRow(
        Guid SubOrderId,
        Guid OrderId,
        Guid StoreId,
        DateTime CreatedAt,
        int Quantity,
        decimal LineTotal,
        int CategoryId,
        string CategoryName);

    private sealed record StoreRow(Guid Id, string Name, StoreStatus Status, DateTime? ApprovedAt);
}
