using System.Text;
using ClosedXML.Excel;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Infrastructure.External;

namespace GearZone.Tests;

public sealed class AdminReportExportServiceTests
{
    [Fact]
    public async Task OverviewExportsHaveBomValidWorkbookAndPdfHeader()
    {
        var service = new AdminReportExportService(new FakeReports(BuildOverview()));
        var csv = await service.ExportAsync("overview", "csv", new AdminReportQueryDto());
        var xlsx = await service.ExportAsync("overview", "xlsx", new AdminReportQueryDto());
        var pdf = await service.ExportAsync("overview", "pdf", new AdminReportQueryDto());

        Assert.True(csv.Content.AsSpan(0, 3).SequenceEqual(Encoding.UTF8.GetPreamble()));
        var csvText = Encoding.UTF8.GetString(csv.Content);
        Assert.Contains("\"Keyboards, \"\"Pro\"\"\"", csvText);

        using var stream = new MemoryStream(xlsx.Content);
        using var workbook = new XLWorkbook(stream);
        Assert.NotNull(workbook.Worksheet("Summary"));
        Assert.NotNull(workbook.Worksheet("Trend"));
        Assert.NotNull(workbook.Worksheet("Details"));

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf.Content, 0, 4));
        Assert.Contains("20260701", pdf.FileName);
    }

    private static AdminOverviewReportDto BuildOverview() => new()
    {
        Period = new AdminReportPeriodDto
        {
            Start = new DateTime(2026, 7, 1), End = new DateTime(2026, 7, 3),
            PreviousStart = new DateTime(2026, 6, 28), PreviousEnd = new DateTime(2026, 6, 30),
            Label = "01 Jul 2026 - 03 Jul 2026", Granularity = "day"
        },
        PaidGmv = ComparisonMetricDto.From(100m, 80m),
        PlatformCommission = ComparisonMetricDto.From(10m, 8m),
        SellerNetAmount = ComparisonMetricDto.From(90m, 72m),
        Orders = ComparisonMetricDto.From(2m, 1m),
        UnitsSold = ComparisonMetricDto.From(3m, 1m),
        AverageOrderValue = ComparisonMetricDto.From(50m, 80m),
        UniqueBuyers = ComparisonMetricDto.From(2m, 1m),
        ActiveSellers = ComparisonMetricDto.From(1m, 1m),
        Trend = [new() { Date = new DateTime(2026, 7, 1), Label = "01 Jul", Gmv = 100m, Commission = 10m, Orders = 2, SubOrders = 2 }],
        RevenueByCategory = [new() { CategoryId = 1, CategoryName = "Keyboards, \"Pro\"", Revenue = 100m, Percentage = 100m }]
    };

    private sealed class FakeReports(AdminOverviewReportDto overview) : IAdminReportService
    {
        public Task<AdminOverviewReportDto> GetOverviewAsync(AdminReportQueryDto query, CancellationToken ct = default) => Task.FromResult(overview);
        public Task<AdminOrderReportDto> GetOrdersAsync(AdminReportQueryDto query, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<AdminSellerReportDto> GetSellersAsync(AdminReportQueryDto query, bool exportAll = false, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<object> GetInsightSnapshotAsync(string reportType, AdminReportQueryDto query, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
