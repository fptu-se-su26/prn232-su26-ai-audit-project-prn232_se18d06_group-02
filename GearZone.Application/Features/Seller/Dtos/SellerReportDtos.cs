using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Seller.Dtos
{
    /// <summary>
    /// Query for all seller report endpoints. Range shortcuts: today, 7d, 30d,
    /// thisMonth, lastMonth, custom (uses From/To). Granularity: day, week, month
    /// (auto-selected from the range length when omitted).
    /// </summary>
    public class SellerReportQueryDto
    {
        public string? Range { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Granularity { get; set; }
        /// <summary>Slow-moving/dead-stock window in days (products report). Defaults to 60.</summary>
        public int? StaleDays { get; set; }
    }

    /// <summary>A single metric with its previous-period value and percentage change.</summary>
    public class MetricDto
    {
        public decimal Current { get; set; }
        public decimal Previous { get; set; }
        /// <summary>Percentage change vs previous period. Null when previous is 0 (no baseline).</summary>
        public double? ChangePct { get; set; }

        public static MetricDto From(decimal current, decimal previous)
        {
            double? pct = previous == 0m
                ? (current == 0m ? 0d : null)
                : (double)((current - previous) / previous) * 100d;
            return new MetricDto { Current = current, Previous = previous, ChangePct = pct };
        }
    }

    public class ReportPeriodDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Granularity { get; set; } = "day";
    }

    // ---------- Sales ----------
    public class SalesPointDto
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        /// <summary>Revenue in the equivalent bucket of the previous period (for trend comparison).</summary>
        public decimal PreviousRevenue { get; set; }
        public int Orders { get; set; }
        public int Units { get; set; }
    }

    public class SalesReportDto
    {
        public bool HasStore { get; set; } = true;
        public string StoreName { get; set; } = string.Empty;
        public ReportPeriodDto Period { get; set; } = new();
        public MetricDto Revenue { get; set; } = new();
        public MetricDto NetRevenue { get; set; } = new();
        public MetricDto Orders { get; set; } = new();
        public MetricDto Units { get; set; } = new();
        public MetricDto AverageOrderValue { get; set; } = new();
        public List<SalesPointDto> Series { get; set; } = new();
    }

    // ---------- Product performance ----------
    public class ProductStatDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class LowStockDto
    {
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }

    /// <summary>A stagnant variant: still in stock but selling slowly / not at all.</summary>
    public class SlowMovingItemDto
    {
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
        /// <summary>Capital tied up in this variant = StockQuantity * Price.</summary>
        public decimal StockValue { get; set; }
        /// <summary>Days since the last realised sale. Null when it has never sold.</summary>
        public int? DaysSinceLastSale { get; set; }
        /// <summary>Units sold within the stale window.</summary>
        public int UnitsSoldWindow { get; set; }
        /// <summary>Estimated days to clear current stock at the window's pace. Null = no sales (never at current pace).</summary>
        public double? DaysToSellOut { get; set; }
        /// <summary>Days since the variant was listed.</summary>
        public int AgeDays { get; set; }
        /// <summary>Dead | Slow | NeverSold.</summary>
        public string Category { get; set; } = string.Empty;
    }

    public class ProductPerformanceReportDto
    {
        public bool HasStore { get; set; } = true;
        public ReportPeriodDto Period { get; set; } = new();
        public List<ProductStatDto> TopProducts { get; set; } = new();
        public List<LowStockDto> LowStock { get; set; } = new();
        public int LowStockThreshold { get; set; }

        // Slow-moving / dead stock.
        public List<SlowMovingItemDto> SlowMovingItems { get; set; } = new();
        public int SlowMovingSkuCount { get; set; }
        public decimal SlowMovingCapital { get; set; }
        public int StaleDays { get; set; }
    }

    // ---------- Operations ----------
    public class StatusBreakdownDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class OperationsReportDto
    {
        public bool HasStore { get; set; } = true;
        public ReportPeriodDto Period { get; set; } = new();
        public MetricDto TotalOrders { get; set; } = new();
        public double CancellationRate { get; set; }
        public double RefundRate { get; set; }
        public double CompletionRate { get; set; }
        public double? AvgFulfillmentHours { get; set; }
        public List<StatusBreakdownDto> StatusBreakdown { get; set; } = new();
    }

    // ---------- Customers ----------
    public class FollowerPointDto
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int NewFollowers { get; set; }
    }

    public class CustomerReportDto
    {
        public bool HasStore { get; set; } = true;
        public ReportPeriodDto Period { get; set; } = new();
        public MetricDto UniqueBuyers { get; set; } = new();
        public int NewBuyers { get; set; }
        public int ReturningBuyers { get; set; }
        public double RepeatRate { get; set; }
        public MetricDto NewFollowers { get; set; } = new();
        public int TotalFollowers { get; set; }
        public List<FollowerPointDto> FollowerSeries { get; set; } = new();
    }

    // ---------- Marketing ----------
    public class VoucherStatDto
    {
        public Guid VoucherId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal TotalDiscount { get; set; }
    }

    public class MarketingReportDto
    {
        public bool HasStore { get; set; } = true;
        public ReportPeriodDto Period { get; set; } = new();
        public MetricDto VoucherUsages { get; set; } = new();
        public MetricDto TotalDiscount { get; set; } = new();
        public List<VoucherStatDto> Vouchers { get; set; } = new();
    }

    // ---------- Reviews ----------
    public class RatingBucketDto
    {
        public int Rating { get; set; }
        public int Count { get; set; }
    }

    public class ReviewTrendPointDto
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double AvgRating { get; set; }
        public int Count { get; set; }
    }

    public class ReviewsReportDto
    {
        public bool HasStore { get; set; } = true;
        public ReportPeriodDto Period { get; set; } = new();
        public double AvgRating { get; set; }
        public int TotalReviews { get; set; }
        public int RepliedCount { get; set; }
        public MetricDto NewReviews { get; set; } = new();
        public List<RatingBucketDto> Distribution { get; set; } = new();
        public List<ReviewTrendPointDto> Trend { get; set; } = new();
    }
}
