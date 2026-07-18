using GearZone.Application.Common.Models;

namespace GearZone.Application.Features.Admin.Dtos;

public sealed class AdminReportQueryDto
{
    public string Range { get; set; } = "30d";
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Granularity { get; set; }

    // Seller-table filters. Other report tabs safely ignore these values.
    public string? Search { get; set; }
    public string StoreStatus { get; set; } = "Approved";
    public string SortBy { get; set; } = "revenue";
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class AdminReportPeriodDto
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public DateTime PreviousStart { get; set; }
    public DateTime PreviousEnd { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Granularity { get; set; } = "day";
    public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
}

public sealed class ComparisonMetricDto
{
    public decimal Current { get; set; }
    public decimal Previous { get; set; }
    public decimal? ChangePct { get; set; }

    public static ComparisonMetricDto From(decimal current, decimal previous) => new()
    {
        Current = current,
        Previous = previous,
        ChangePct = previous == 0m
            ? current == 0m ? 0m : null
            : Math.Round((current - previous) / previous * 100m, 2)
    };
}

public sealed class AdminReportSeriesPointDto
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Gmv { get; set; }
    public decimal Commission { get; set; }
    public int Orders { get; set; }
    public int SubOrders { get; set; }
}

public sealed class AdminCategoryRevenueDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

public sealed class AdminOverviewReportDto
{
    public AdminReportPeriodDto Period { get; set; } = new();
    public ComparisonMetricDto PaidGmv { get; set; } = new();
    public ComparisonMetricDto PlatformCommission { get; set; } = new();
    public ComparisonMetricDto SellerNetAmount { get; set; } = new();
    public ComparisonMetricDto Orders { get; set; } = new();
    public ComparisonMetricDto UnitsSold { get; set; } = new();
    public ComparisonMetricDto AverageOrderValue { get; set; } = new();
    public ComparisonMetricDto UniqueBuyers { get; set; } = new();
    public ComparisonMetricDto ActiveSellers { get; set; } = new();
    public List<AdminReportSeriesPointDto> Trend { get; set; } = new();
    public List<AdminCategoryRevenueDto> RevenueByCategory { get; set; } = new();
}

public sealed class AdminStatusBreakdownDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public sealed class AdminPaymentMethodBreakdownDto
{
    public string Method { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public sealed class AdminHighValueOrderDto
{
    public Guid OrderId { get; set; }
    public long OrderCode { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal PaidGmv { get; set; }
    public int StoreCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public sealed class AdminOrderReportDto
{
    public AdminReportPeriodDto Period { get; set; } = new();
    public ComparisonMetricDto Orders { get; set; } = new();
    public ComparisonMetricDto SubOrders { get; set; } = new();
    public ComparisonMetricDto PaidSubOrders { get; set; } = new();
    public decimal CompletionRate { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal RejectionRate { get; set; }
    public decimal RefundRate { get; set; }
    public decimal? AverageFulfillmentHours { get; set; }
    public List<AdminReportSeriesPointDto> Trend { get; set; } = new();
    public List<AdminStatusBreakdownDto> StatusBreakdown { get; set; } = new();
    public List<AdminPaymentMethodBreakdownDto> PaymentMethods { get; set; } = new();
    public List<AdminHighValueOrderDto> HighValueOrders { get; set; } = new();
}

public sealed class AdminSellerPerformanceDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal PaidGmv { get; set; }
    public decimal PreviousGmv { get; set; }
    public decimal? GrowthPct { get; set; }
    public decimal Commission { get; set; }
    public decimal SellerNetAmount { get; set; }
    public int Orders { get; set; }
    public int Units { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal RefundRate { get; set; }
    public decimal AverageRating { get; set; }
}

public sealed class AdminSellerReportDto
{
    public AdminReportPeriodDto Period { get; set; } = new();
    public ComparisonMetricDto ActiveSellers { get; set; } = new();
    public ComparisonMetricDto NewApprovedSellers { get; set; } = new();
    public ComparisonMetricDto PaidGmv { get; set; } = new();
    public ComparisonMetricDto PlatformCommission { get; set; } = new();
    public ComparisonMetricDto SellerNetAmount { get; set; } = new();
    public PagedResult<AdminSellerPerformanceDto> Sellers { get; set; } = new();
}

public sealed class AdminAiInsightItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
    public List<string> MetricKeys { get; set; } = new();
}

public sealed class AdminAiRecommendationDto
{
    public string Title { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Priority { get; set; } = "medium";
    public List<string> MetricKeys { get; set; } = new();
}

public sealed class AdminAiInsightDto
{
    public string Summary { get; set; } = string.Empty;
    public List<AdminAiInsightItemDto> Highlights { get; set; } = new();
    public List<AdminAiInsightItemDto> Risks { get; set; } = new();
    public List<AdminAiRecommendationDto> Recommendations { get; set; } = new();
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
    public bool IsCached { get; set; }
    public bool HasEnoughData { get; set; } = true;
}

public sealed class AdminReportFileDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = "admin-report.bin";
}

