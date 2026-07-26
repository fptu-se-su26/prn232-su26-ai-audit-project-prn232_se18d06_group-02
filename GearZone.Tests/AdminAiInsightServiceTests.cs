using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace GearZone.Tests;

public sealed class AdminAiInsightServiceTests
{
    [Fact]
    public async Task Generate_UsesSnapshotCacheAndRejectsUnsupportedEvidenceKeys()
    {
        var reports = new FakeReports(new
        {
            period = new { start = "2026-07-01", end = "2026-07-03" },
            metrics = new Dictionary<string, decimal?> { ["paidGmv"] = 100m, ["orders"] = 2m },
            trend = new[] { new { label = "01 Jul", gmv = 100m } }
        });
        var provider = new FakeProvider();
        var resolver = new FakeResolver(provider);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AdminAiInsightService(reports, resolver, cache, NullLogger<AdminAiInsightService>.Instance);
        var query = new AdminReportQueryDto();

        var first = await service.GenerateAsync("overview", query, false);
        var cached = await service.GenerateAsync("overview", query, false);
        var refreshed = await service.GenerateAsync("overview", query, true);

        Assert.False(first.IsCached);
        Assert.True(cached.IsCached);
        Assert.False(refreshed.IsCached);
        Assert.Equal(2, provider.Calls);
        Assert.Single(first.Highlights);
        Assert.Equal("paidGmv", first.Highlights[0].MetricKeys[0]);
        Assert.DoesNotContain("email", provider.LastRequest!.SnapshotJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("phone", provider.LastRequest.SnapshotJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Generate_WithNoNonZeroMetricsDoesNotCallProvider()
    {
        var reports = new FakeReports(new
        {
            metrics = new Dictionary<string, decimal?> { ["orders"] = 0m, ["paidGmv"] = 0m }
        });
        var provider = new FakeProvider();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AdminAiInsightService(
            reports, new FakeResolver(provider), cache, NullLogger<AdminAiInsightService>.Instance);

        var result = await service.GenerateAsync("overview", new AdminReportQueryDto(), false);

        Assert.False(result.HasEnoughData);
        Assert.Equal(0, provider.Calls);
    }

    private sealed class FakeProvider : IAiInsightProvider
    {
        public string Name => "Fake";
        public string Model => "fake-model";
        public int Calls { get; private set; }
        public AiInsightProviderRequest? LastRequest { get; private set; }

        public Task<AdminAiInsightDto> GenerateAsync(AiInsightProviderRequest request, CancellationToken ct = default)
        {
            Calls++;
            LastRequest = request;
            return Task.FromResult(new AdminAiInsightDto
            {
                Summary = "GMV increased with evidence from the supplied snapshot.",
                Highlights =
                [
                    new() { Title = "GMV", Explanation = "Paid GMV is positive.", Severity = "info", MetricKeys = ["paidGmv"] },
                    new() { Title = "Invented", Explanation = "Unsupported.", Severity = "critical", MetricKeys = ["customerEmail"] }
                ],
                Recommendations =
                [
                    new() { Title = "Monitor", Action = "Track paid GMV daily.", Priority = "low", MetricKeys = ["paidGmv"] }
                ]
            });
        }
    }

    private sealed class FakeResolver(IAiInsightProvider provider) : IAiInsightProviderResolver
    {
        public IAiInsightProvider Resolve() => provider;
        public string ProviderName => provider.Name;
        public string Model => provider.Model;
        public bool IsEnabled => true;
    }

    private sealed class FakeReports(object snapshot) : IAdminReportService
    {
        public Task<object> GetInsightSnapshotAsync(string reportType, AdminReportQueryDto query, CancellationToken ct = default) =>
            Task.FromResult(snapshot);

        public Task<AdminOverviewReportDto> GetOverviewAsync(AdminReportQueryDto query, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<AdminOrderReportDto> GetOrdersAsync(AdminReportQueryDto query, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<AdminSellerReportDto> GetSellersAsync(AdminReportQueryDto query, bool exportAll = false, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
