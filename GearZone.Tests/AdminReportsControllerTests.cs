using GearZone.Api.Controllers.Admin;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GearZone.Tests;

public sealed class AdminReportsControllerTests
{
    [Fact]
    public void ControllerRequiresSuperAdminAndGenerationIsRateLimited()
    {
        var authorize = Assert.Single(typeof(ReportsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal("Super Admin", authorize.Roles);

        var post = typeof(ReportsController).GetMethod(nameof(ReportsController.GenerateInsight));
        var limiter = Assert.Single(post!.GetCustomAttributes(typeof(EnableRateLimitingAttribute), true)
            .Cast<EnableRateLimitingAttribute>());
        Assert.Equal("admin-ai-insights", limiter.PolicyName);
    }

    [Fact]
    public async Task InvalidRangeReturns400AndAiProviderFailureReturns503()
    {
        var controller = new ReportsController(new ResolverBackedReports(), new FakeExports(), new FailedInsights());

        var invalid = await controller.Overview(new AdminReportQueryDto
        {
            Range = "custom", From = new DateTime(2026, 7, 2), To = new DateTime(2026, 7, 1)
        }, default);
        var aiFailure = await controller.GenerateInsight("overview", false, new AdminReportQueryDto(), default);

        Assert.Equal(400, Assert.IsType<ObjectResult>(invalid).StatusCode);
        Assert.Equal(503, Assert.IsType<ObjectResult>(aiFailure).StatusCode);
    }

    private sealed class ResolverBackedReports : IAdminReportService
    {
        public Task<AdminOverviewReportDto> GetOverviewAsync(AdminReportQueryDto query, CancellationToken ct = default)
        {
            AdminReportPeriodResolver.Resolve(query);
            return Task.FromResult(new AdminOverviewReportDto());
        }

        public Task<AdminOrderReportDto> GetOrdersAsync(AdminReportQueryDto query, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<AdminSellerReportDto> GetSellersAsync(AdminReportQueryDto query, bool exportAll = false, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<object> GetInsightSnapshotAsync(string reportType, AdminReportQueryDto query, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class FakeExports : IAdminReportExportService
    {
        public Task<AdminReportFileDto> ExportAsync(string reportType, string format, AdminReportQueryDto query, CancellationToken ct = default) =>
            Task.FromResult(new AdminReportFileDto());
    }

    private sealed class FailedInsights : IAdminAiInsightService
    {
        public Task<AdminAiInsightDto?> GetCachedAsync(string reportType, AdminReportQueryDto query, CancellationToken ct = default) => Task.FromResult<AdminAiInsightDto?>(null);
        public Task<AdminAiInsightDto> GenerateAsync(string reportType, AdminReportQueryDto query, bool forceRefresh, CancellationToken ct = default) =>
            throw new AiInsightUnavailableException("Provider unavailable.");
    }
}
