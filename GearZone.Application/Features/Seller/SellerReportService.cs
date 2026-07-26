using System.Globalization;
using System.Text;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Seller
{
    public class SellerReportService : ISellerReportService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly ISubOrderRepository _subOrderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IStoreFollowRepository _storeFollowRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IVoucherUsageRepository _voucherUsageRepository;
        private readonly IProductReviewRepository _reviewRepository;

        // Statuses that do NOT count towards realised revenue.
        private static readonly OrderStatus[] NonRevenueStatuses =
            { OrderStatus.Cancelled, OrderStatus.Rejected, OrderStatus.Refunded };

        private const int LowStockThreshold = 5;

        public SellerReportService(
            IStoreRepository storeRepository,
            ISubOrderRepository subOrderRepository,
            IOrderItemRepository orderItemRepository,
            IProductVariantRepository variantRepository,
            IStoreFollowRepository storeFollowRepository,
            IVoucherRepository voucherRepository,
            IVoucherUsageRepository voucherUsageRepository,
            IProductReviewRepository reviewRepository)
        {
            _storeRepository = storeRepository;
            _subOrderRepository = subOrderRepository;
            _orderItemRepository = orderItemRepository;
            _variantRepository = variantRepository;
            _storeFollowRepository = storeFollowRepository;
            _voucherRepository = voucherRepository;
            _voucherUsageRepository = voucherUsageRepository;
            _reviewRepository = reviewRepository;
        }

        // ---------------------------------------------------------------- Sales
        public async Task<SalesReportDto> GetSalesReportAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default)
        {
            var store = await _storeRepository.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null) return new SalesReportDto { HasStore = false };

            var p = ResolvePeriod(query);

            // One pass over both current + previous window for order-level metrics.
            var orderRows = await _subOrderRepository.Query()
                .Where(x => x.StoreId == store.Id
                    && x.CreatedAt >= p.PrevStart && x.CreatedAt <= p.End
                    && !NonRevenueStatuses.Contains(x.Status))
                .Select(x => new { x.CreatedAt, x.CommissionableAmount, x.NetAmount })
                .ToListAsync(ct);

            var itemRows = await _orderItemRepository.Query()
                .Where(oi => oi.SubOrder.StoreId == store.Id
                    && oi.SubOrder.CreatedAt >= p.PrevStart && oi.SubOrder.CreatedAt <= p.End
                    && !NonRevenueStatuses.Contains(oi.SubOrder.Status))
                .Select(oi => new { oi.SubOrder.CreatedAt, oi.Quantity })
                .ToListAsync(ct);

            var curOrders = orderRows.Where(r => r.CreatedAt >= p.Start).ToList();
            var prevOrders = orderRows.Where(r => r.CreatedAt < p.Start).ToList();
            var curItems = itemRows.Where(r => r.CreatedAt >= p.Start).ToList();
            var prevItems = itemRows.Where(r => r.CreatedAt < p.Start).ToList();

            decimal curRevenue = curOrders.Sum(r => r.CommissionableAmount);
            decimal prevRevenue = prevOrders.Sum(r => r.CommissionableAmount);
            decimal curNet = curOrders.Sum(r => r.NetAmount);
            decimal prevNet = prevOrders.Sum(r => r.NetAmount);
            int curCount = curOrders.Count;
            int prevCount = prevOrders.Count;
            int curUnits = curItems.Sum(r => r.Quantity);
            int prevUnits = prevItems.Sum(r => r.Quantity);
            decimal curAov = curCount == 0 ? 0 : curRevenue / curCount;
            decimal prevAov = prevCount == 0 ? 0 : prevRevenue / prevCount;

            var buckets = BuildBuckets(p.Start, p.End, p.Granularity);
            var series = buckets.Select(b => new SalesPointDto { Label = b.Label, Date = b.Start }).ToList();
            foreach (var r in curOrders)
            {
                var idx = BucketIndex(r.CreatedAt, p.Start, p.Granularity, buckets.Count);
                if (idx < 0) continue;
                series[idx].Revenue += r.CommissionableAmount;
                series[idx].Orders += 1;
            }
            foreach (var r in curItems)
            {
                var idx = BucketIndex(r.CreatedAt, p.Start, p.Granularity, buckets.Count);
                if (idx < 0) continue;
                series[idx].Units += r.Quantity;
            }
            // Overlay the previous period bucketed against the same axis (index-aligned).
            foreach (var r in prevOrders)
            {
                var idx = BucketIndex(r.CreatedAt, p.PrevStart, p.Granularity, buckets.Count);
                if (idx < 0) continue;
                series[idx].PreviousRevenue += r.CommissionableAmount;
            }

            return new SalesReportDto
            {
                HasStore = true,
                StoreName = store.StoreName,
                Period = ToPeriodDto(p),
                Revenue = MetricDto.From(curRevenue, prevRevenue),
                NetRevenue = MetricDto.From(curNet, prevNet),
                Orders = MetricDto.From(curCount, prevCount),
                Units = MetricDto.From(curUnits, prevUnits),
                AverageOrderValue = MetricDto.From(curAov, prevAov),
                Series = series
            };
        }

        public async Task<string> GetSalesCsvAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default)
        {
            var report = await GetSalesReportAsync(ownerUserId, query, ct);
            var sb = new StringBuilder();
            sb.AppendLine("Period,Revenue,Orders,Units");
            foreach (var pt in report.Series)
                sb.AppendLine($"{Csv(pt.Label)},{pt.Revenue},{pt.Orders},{pt.Units}");
            sb.AppendLine();
            sb.AppendLine($"Total Revenue,{report.Revenue.Current}");
            sb.AppendLine($"Net Revenue,{report.NetRevenue.Current}");
            sb.AppendLine($"Total Orders,{report.Orders.Current}");
            sb.AppendLine($"Units Sold,{report.Units.Current}");
            sb.AppendLine($"Average Order Value,{report.AverageOrderValue.Current}");
            return sb.ToString();
        }

        // ------------------------------------------------------ Product performance
        public async Task<ProductPerformanceReportDto> GetProductPerformanceAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default)
        {
            var store = await _storeRepository.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null) return new ProductPerformanceReportDto { HasStore = false };

            var p = ResolvePeriod(query);

            var rows = await _orderItemRepository.Query()
                .Where(oi => oi.SubOrder.StoreId == store.Id
                    && oi.SubOrder.CreatedAt >= p.Start && oi.SubOrder.CreatedAt <= p.End
                    && !NonRevenueStatuses.Contains(oi.SubOrder.Status))
                .Select(oi => new
                {
                    oi.Variant.ProductId,
                    ProductName = oi.Variant.Product.Name,
                    oi.Quantity,
                    oi.LineTotal,
                    oi.SubOrderId
                })
                .ToListAsync(ct);

            var topProducts = rows
                .GroupBy(r => new { r.ProductId, r.ProductName })
                .Select(g => new ProductStatDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    UnitsSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.LineTotal),
                    OrderCount = g.Select(x => x.SubOrderId).Distinct().Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(20)
                .ToList();

            var lowStock = await _variantRepository.Query()
                .Where(v => v.Product.StoreId == store.Id
                    && !v.IsDeleted && v.IsActive
                    && v.StockQuantity <= LowStockThreshold)
                .OrderBy(v => v.StockQuantity)
                .Select(v => new LowStockDto
                {
                    VariantId = v.Id,
                    ProductName = v.Product.Name,
                    VariantName = v.VariantName,
                    Sku = v.Sku,
                    StockQuantity = v.StockQuantity
                })
                .Take(50)
                .ToListAsync(ct);

            var slow = await BuildSlowMovingAsync(store.Id, query.StaleDays, ct);

            return new ProductPerformanceReportDto
            {
                HasStore = true,
                Period = ToPeriodDto(p),
                TopProducts = topProducts,
                LowStock = lowStock,
                LowStockThreshold = LowStockThreshold,
                SlowMovingItems = slow.Items,
                SlowMovingSkuCount = slow.TotalCount,
                SlowMovingCapital = slow.TotalCapital,
                StaleDays = slow.StaleDays
            };
        }

        // Flags variants that are still in stock but selling slowly or not at all, so the seller
        // can clear ageing capital. "Slow" sells but would take > 90 days to clear at the current
        // pace; "Dead" has stock but no sale within the window; "NeverSold" has never sold at all.
        private async Task<(List<SlowMovingItemDto> Items, int TotalCount, decimal TotalCapital, int StaleDays)>
            BuildSlowMovingAsync(Guid storeId, int? requestedStaleDays, CancellationToken ct)
        {
            var staleDays = requestedStaleDays is > 0 ? requestedStaleDays.Value : 60;
            var today = DateTime.UtcNow.Date;
            var staleCutoff = today.AddDays(-staleDays);

            var variants = await _variantRepository.Query()
                .Where(v => v.Product.StoreId == storeId && !v.IsDeleted && v.IsActive && v.StockQuantity > 0)
                .Select(v => new
                {
                    v.Id,
                    ProductName = v.Product.Name,
                    v.VariantName,
                    v.Sku,
                    v.Price,
                    v.StockQuantity,
                    v.CreatedAt
                })
                .ToListAsync(ct);

            // All realised sale lines for the store (lightweight columns, aggregated in memory).
            var saleRows = await _orderItemRepository.Query()
                .Where(oi => oi.SubOrder.StoreId == storeId && !NonRevenueStatuses.Contains(oi.SubOrder.Status))
                .Select(oi => new { oi.VariantId, oi.SubOrder.CreatedAt, oi.Quantity })
                .ToListAsync(ct);

            var lastSaleByVariant = saleRows
                .GroupBy(r => r.VariantId)
                .ToDictionary(g => g.Key, g => g.Max(x => x.CreatedAt));
            var windowUnitsByVariant = saleRows
                .Where(r => r.CreatedAt >= staleCutoff)
                .GroupBy(r => r.VariantId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            var items = new List<SlowMovingItemDto>();
            foreach (var v in variants)
            {
                windowUnitsByVariant.TryGetValue(v.Id, out var soldWindow);
                bool hasLastSale = lastSaleByVariant.TryGetValue(v.Id, out var lastSale);
                int age = Math.Max(0, (int)(today - v.CreatedAt.Date).TotalDays);

                string? category;
                double? daysToSellOut = null;
                if (soldWindow > 0)
                {
                    var dailyVelocity = (double)soldWindow / staleDays;
                    daysToSellOut = dailyVelocity > 0 ? Math.Round(v.StockQuantity / dailyVelocity, 0) : null;
                    category = daysToSellOut is > 90 ? "Slow" : null; // sells fine → not stagnant
                }
                else
                {
                    // No sale within the window: only flag once the item has had a fair chance to sell.
                    category = age >= staleDays ? (hasLastSale ? "Dead" : "NeverSold") : null;
                }
                if (category == null) continue;

                items.Add(new SlowMovingItemDto
                {
                    VariantId = v.Id,
                    ProductName = v.ProductName,
                    VariantName = v.VariantName,
                    Sku = v.Sku,
                    StockQuantity = v.StockQuantity,
                    Price = v.Price,
                    StockValue = v.StockQuantity * v.Price,
                    DaysSinceLastSale = hasLastSale ? Math.Max(0, (int)(today - lastSale.Date).TotalDays) : null,
                    UnitsSoldWindow = soldWindow,
                    DaysToSellOut = daysToSellOut,
                    AgeDays = age,
                    Category = category
                });
            }

            var ordered = items.OrderByDescending(x => x.StockValue).ToList();
            // Return the full stagnant set (capped for safety); the UI paginates client-side.
            return (ordered.Take(500).ToList(), ordered.Count, ordered.Sum(x => x.StockValue), staleDays);
        }

        // ------------------------------------------------------------ Operations
        public async Task<OperationsReportDto> GetOperationsReportAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default)
        {
            var store = await _storeRepository.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null) return new OperationsReportDto { HasStore = false };

            var p = ResolvePeriod(query);

            var rows = await _subOrderRepository.Query()
                .Where(x => x.StoreId == store.Id && x.CreatedAt >= p.PrevStart && x.CreatedAt <= p.End)
                .Select(x => new { x.CreatedAt, x.Status, x.DeliveredAt })
                .ToListAsync(ct);

            var cur = rows.Where(r => r.CreatedAt >= p.Start).ToList();
            var prev = rows.Where(r => r.CreatedAt < p.Start).ToList();

            int total = cur.Count;
            int cancelled = cur.Count(r => r.Status == OrderStatus.Cancelled || r.Status == OrderStatus.Rejected);
            int refunded = cur.Count(r => r.Status == OrderStatus.Refunded);
            int completed = cur.Count(r => r.Status == OrderStatus.Completed || r.Status == OrderStatus.Delivered);

            var fulfilled = cur.Where(r => r.DeliveredAt.HasValue).ToList();
            double? avgHours = fulfilled.Count == 0
                ? null
                : fulfilled.Average(r => (r.DeliveredAt!.Value - r.CreatedAt).TotalHours);

            var statusBreakdown = cur
                .GroupBy(r => r.Status)
                .Select(g => new StatusBreakdownDto { Status = g.Key.ToString(), Count = g.Count() })
                .OrderByDescending(s => s.Count)
                .ToList();

            return new OperationsReportDto
            {
                HasStore = true,
                Period = ToPeriodDto(p),
                TotalOrders = MetricDto.From(total, prev.Count),
                CancellationRate = Pct(cancelled, total),
                RefundRate = Pct(refunded, total),
                CompletionRate = Pct(completed, total),
                AvgFulfillmentHours = avgHours,
                StatusBreakdown = statusBreakdown
            };
        }

        // ------------------------------------------------------------- Customers
        public async Task<CustomerReportDto> GetCustomerReportAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default)
        {
            var store = await _storeRepository.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null) return new CustomerReportDto { HasStore = false };

            var p = ResolvePeriod(query);

            // Buyers in the current + previous period (paid-through statuses only).
            var buyerRows = await _subOrderRepository.Query()
                .Where(x => x.StoreId == store.Id
                    && x.CreatedAt >= p.PrevStart && x.CreatedAt <= p.End
                    && !NonRevenueStatuses.Contains(x.Status))
                .Select(x => new { x.Order.UserId, x.CreatedAt })
                .ToListAsync(ct);

            var curBuyers = buyerRows.Where(r => r.CreatedAt >= p.Start).Select(r => r.UserId).Distinct().ToHashSet();
            var prevBuyers = buyerRows.Where(r => r.CreatedAt < p.Start).Select(r => r.UserId).Distinct().Count();

            // Buyers who ordered from this store before the period start = "returning".
            var earlierBuyers = await _subOrderRepository.Query()
                .Where(x => x.StoreId == store.Id && x.CreatedAt < p.Start
                    && !NonRevenueStatuses.Contains(x.Status))
                .Select(x => x.Order.UserId)
                .Distinct()
                .ToListAsync(ct);
            var earlierSet = earlierBuyers.ToHashSet();

            int returning = curBuyers.Count(b => earlierSet.Contains(b));
            int newBuyers = curBuyers.Count - returning;

            // Followers.
            var followRows = await _storeFollowRepository.Query()
                .Where(f => f.StoreId == store.Id && f.FollowedAt >= p.PrevStart && f.FollowedAt <= p.End)
                .Select(f => f.FollowedAt)
                .ToListAsync(ct);
            int curFollowers = followRows.Count(d => d >= p.Start);
            int prevFollowers = followRows.Count(d => d < p.Start);
            int totalFollowers = await _storeFollowRepository.Query().CountAsync(f => f.StoreId == store.Id, ct);

            var buckets = BuildBuckets(p.Start, p.End, p.Granularity);
            var followerSeries = buckets.Select(b => new FollowerPointDto { Label = b.Label, Date = b.Start }).ToList();
            foreach (var d in followRows.Where(d => d >= p.Start))
            {
                var idx = BucketIndex(d, p.Start, p.Granularity, buckets.Count);
                if (idx >= 0) followerSeries[idx].NewFollowers += 1;
            }
            // Previous period overlaid on the same axis (index-aligned).
            foreach (var d in followRows.Where(d => d < p.Start))
            {
                var idx = BucketIndex(d, p.PrevStart, p.Granularity, buckets.Count);
                if (idx >= 0) followerSeries[idx].PreviousNewFollowers += 1;
            }

            return new CustomerReportDto
            {
                HasStore = true,
                Period = ToPeriodDto(p),
                UniqueBuyers = MetricDto.From(curBuyers.Count, prevBuyers),
                NewBuyers = newBuyers,
                ReturningBuyers = returning,
                RepeatRate = Pct(returning, curBuyers.Count),
                NewFollowers = MetricDto.From(curFollowers, prevFollowers),
                TotalFollowers = totalFollowers,
                FollowerSeries = followerSeries
            };
        }

        // ------------------------------------------------------------- Marketing
        public async Task<MarketingReportDto> GetMarketingReportAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default)
        {
            var store = await _storeRepository.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null) return new MarketingReportDto { HasStore = false };

            var p = ResolvePeriod(query);

            // The voucher domain (SellerVoucherService / VoucherService) stores StartAt, EndAt
            // and UsedAt in server-local time (DateTime.Now), whereas the report period is UTC.
            // Shift the whole window to local so every voucher comparison here lines up; otherwise
            // usages and validity near the UTC/local boundary (e.g. a voucher starting "today")
            // get miscounted or dropped.
            var localOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            var winPrevStart = p.PrevStart.Add(localOffset);
            var winStart = p.Start.Add(localOffset);
            var winEnd = p.End.Add(localOffset);

            var usageRows = await _voucherUsageRepository.Query()
                .Where(u => u.Voucher.StoreId == store.Id
                    && u.UsedAt >= winPrevStart && u.UsedAt <= winEnd)
                .Select(u => new
                {
                    u.VoucherId,
                    u.Voucher.Code,
                    u.Voucher.Name,
                    u.DiscountAmount,
                    u.UsedAt
                })
                .ToListAsync(ct);

            var cur = usageRows.Where(r => r.UsedAt >= winStart).ToList();
            var prev = usageRows.Where(r => r.UsedAt < winStart).ToList();

            // Aggregate redemptions per voucher for the current period.
            var vouchersById = cur
                .GroupBy(r => new { r.VoucherId, r.Code, r.Name })
                .ToDictionary(g => g.Key.VoucherId, g => new VoucherStatDto
                {
                    VoucherId = g.Key.VoucherId,
                    Code = g.Key.Code,
                    Name = g.Key.Name,
                    UsageCount = g.Count(),
                    TotalDiscount = g.Sum(x => x.DiscountAmount)
                });

            // Also list this store's vouchers whose validity window overlaps the period,
            // so brand-new / unused vouchers still show up (with 0 uses) instead of vanishing.
            var periodVouchers = await _voucherRepository.Query()
                .Where(v => v.StoreId == store.Id && v.StartAt <= winEnd && v.EndAt >= winStart)
                .Select(v => new { v.Id, v.Code, v.Name })
                .ToListAsync(ct);

            foreach (var v in periodVouchers)
            {
                if (!vouchersById.ContainsKey(v.Id))
                    vouchersById[v.Id] = new VoucherStatDto { VoucherId = v.Id, Code = v.Code, Name = v.Name };
            }

            var vouchers = vouchersById.Values
                .OrderByDescending(v => v.UsageCount)
                .ThenBy(v => v.Code)
                .ToList();

            return new MarketingReportDto
            {
                HasStore = true,
                Period = ToPeriodDto(p),
                VoucherUsages = MetricDto.From(cur.Count, prev.Count),
                TotalDiscount = MetricDto.From(cur.Sum(r => r.DiscountAmount), prev.Sum(r => r.DiscountAmount)),
                Vouchers = vouchers
            };
        }

        // --------------------------------------------------------------- Reviews
        public async Task<ReviewsReportDto> GetReviewsReportAsync(string ownerUserId, SellerReportQueryDto query, CancellationToken ct = default)
        {
            var store = await _storeRepository.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null) return new ReviewsReportDto { HasStore = false };

            var p = ResolvePeriod(query);

            var rows = await _reviewRepository.Query()
                .Where(r => r.StoreId == store.Id && !r.IsDeleted
                    && r.CreatedAt >= p.PrevStart && r.CreatedAt <= p.End)
                .Select(r => new { r.Rating, r.CreatedAt, HasReply = r.SellerReplyContent != null })
                .ToListAsync(ct);

            var cur = rows.Where(r => r.CreatedAt >= p.Start).ToList();
            var prev = rows.Where(r => r.CreatedAt < p.Start).ToList();

            var distribution = Enumerable.Range(1, 5)
                .Select(star => new RatingBucketDto { Rating = star, Count = cur.Count(r => r.Rating == star) })
                .ToList();

            var buckets = BuildBuckets(p.Start, p.End, p.Granularity);
            var trend = buckets.Select(b => new ReviewTrendPointDto { Label = b.Label, Date = b.Start }).ToList();
            var grouped = new List<int>[buckets.Count];
            for (int i = 0; i < grouped.Length; i++) grouped[i] = new List<int>();
            foreach (var r in cur)
            {
                var idx = BucketIndex(r.CreatedAt, p.Start, p.Granularity, buckets.Count);
                if (idx >= 0) grouped[idx].Add(r.Rating);
            }
            // Previous-period review counts bucketed against the same axis.
            var prevGrouped = new int[buckets.Count];
            foreach (var r in prev)
            {
                var idx = BucketIndex(r.CreatedAt, p.PrevStart, p.Granularity, buckets.Count);
                if (idx >= 0) prevGrouped[idx]++;
            }
            for (int i = 0; i < trend.Count; i++)
            {
                trend[i].Count = grouped[i].Count;
                trend[i].AvgRating = grouped[i].Count == 0 ? 0 : Math.Round(grouped[i].Average(), 2);
                trend[i].PreviousCount = prevGrouped[i];
            }

            return new ReviewsReportDto
            {
                HasStore = true,
                Period = ToPeriodDto(p),
                AvgRating = cur.Count == 0 ? 0 : Math.Round(cur.Average(r => (double)r.Rating), 2),
                TotalReviews = cur.Count,
                RepliedCount = cur.Count(r => r.HasReply),
                NewReviews = MetricDto.From(cur.Count, prev.Count),
                Distribution = distribution,
                Trend = trend
            };
        }

        // ------------------------------------------------------------- Helpers
        private sealed record Period(DateTime Start, DateTime End, DateTime PrevStart, string Granularity, string Label);

        private static Period ResolvePeriod(SellerReportQueryDto query)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            DateTime start, end;
            string label;

            switch ((query.Range ?? "30d").ToLowerInvariant())
            {
                case "today":
                    start = today; end = now; label = "Today"; break;
                case "7d":
                    start = today.AddDays(-6); end = now; label = "Last 7 days"; break;
                case "thismonth":
                    start = new DateTime(today.Year, today.Month, 1); end = now; label = "This month"; break;
                case "lastmonth":
                    var firstThis = new DateTime(today.Year, today.Month, 1);
                    start = firstThis.AddMonths(-1);
                    end = firstThis.AddTicks(-1);
                    label = "Last month";
                    break;
                case "custom":
                    start = (query.From ?? today.AddDays(-29)).Date;
                    end = query.To.HasValue ? query.To.Value.Date.AddDays(1).AddTicks(-1) : now;
                    label = $"{start:dd/MM/yyyy} – {end:dd/MM/yyyy}";
                    break;
                default: // 30d
                    start = today.AddDays(-29); end = now; label = "Last 30 days"; break;
            }

            if (end < start) end = start;

            int days = (end.Date - start.Date).Days + 1;
            var prevStart = start.AddDays(-days);

            var granularity = ResolveGranularity(query.Granularity, days);
            return new Period(start, end, prevStart, granularity, label);
        }

        private static string ResolveGranularity(string? requested, int days)
        {
            if (!string.IsNullOrWhiteSpace(requested))
            {
                var g = requested.ToLowerInvariant();
                if (g is "day" or "week" or "month") return g;
            }
            if (days <= 31) return "day";
            if (days <= 120) return "week";
            return "month";
        }

        private static ReportPeriodDto ToPeriodDto(Period p) => new()
        {
            Start = p.Start,
            End = p.End,
            Label = p.Label,
            Granularity = p.Granularity
        };

        private sealed record Bucket(DateTime Start, string Label);

        private static List<Bucket> BuildBuckets(DateTime start, DateTime end, string granularity)
        {
            var buckets = new List<Bucket>();
            switch (granularity)
            {
                case "month":
                    var cursor = new DateTime(start.Year, start.Month, 1);
                    var last = new DateTime(end.Year, end.Month, 1);
                    while (cursor <= last)
                    {
                        buckets.Add(new Bucket(cursor, cursor.ToString("MMM yy", CultureInfo.InvariantCulture)));
                        cursor = cursor.AddMonths(1);
                    }
                    break;
                case "week":
                    var w = start.Date;
                    while (w <= end.Date)
                    {
                        buckets.Add(new Bucket(w, w.ToString("dd/MM", CultureInfo.InvariantCulture)));
                        w = w.AddDays(7);
                    }
                    break;
                default: // day
                    var d = start.Date;
                    while (d <= end.Date)
                    {
                        buckets.Add(new Bucket(d, d.ToString("dd/MM", CultureInfo.InvariantCulture)));
                        d = d.AddDays(1);
                    }
                    break;
            }
            if (buckets.Count == 0) buckets.Add(new Bucket(start.Date, start.ToString("dd/MM", CultureInfo.InvariantCulture)));
            return buckets;
        }

        private static int BucketIndex(DateTime date, DateTime start, string granularity, int count)
        {
            int idx = granularity switch
            {
                "month" => (date.Year - start.Year) * 12 + date.Month - start.Month,
                "week" => (date.Date - start.Date).Days / 7,
                _ => (date.Date - start.Date).Days
            };
            if (idx < 0) return -1;
            if (idx >= count) return count - 1;
            return idx;
        }

        private static double Pct(int part, int whole) =>
            whole == 0 ? 0 : Math.Round((double)part / whole * 100d, 2);

        private static string Csv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
