using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using GearZone.Api.Auditing;
using GearZone.Application.Features.Admin;
using GearZone.Domain.Enums;
using GearZone.Infrastructure.Auditing;

namespace GearZone.Api.Controllers.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/reports")]
public sealed class ReportsController : BaseApiController
{
    private readonly IAdminReportService _reports;
    private readonly IAdminReportExportService _exports;
    private readonly IAdminAiInsightService _insights;

    public ReportsController(
        IAdminReportService reports,
        IAdminReportExportService exports,
        IAdminAiInsightService insights)
    {
        _reports = reports;
        _exports = exports;
        _insights = insights;
    }

    [HttpGet("overview")]
    public Task<IActionResult> Overview([FromQuery] AdminReportQueryDto query, CancellationToken ct) =>
        ExecuteAsync(async () => OkResponse(await _reports.GetOverviewAsync(query, ct)));

    [HttpGet("orders")]
    public Task<IActionResult> Orders([FromQuery] AdminReportQueryDto query, CancellationToken ct) =>
        ExecuteAsync(async () => OkResponse(await _reports.GetOrdersAsync(query, ct)));

    [HttpGet("sellers")]
    public Task<IActionResult> Sellers([FromQuery] AdminReportQueryDto query, CancellationToken ct) =>
        ExecuteAsync(async () => OkResponse(await _reports.GetSellersAsync(query, false, ct)));

    [HttpGet("{reportType}/export")]
    [AdminAuditAction(AdminAuditActions.ReportExported, AdminAuditModules.Reports, AdminAuditRiskLevel.Medium, EntityType = "AdminReport", RouteIdName = "reportType")]
    public Task<IActionResult> Export(
        string reportType,
        [FromQuery] string format,
        [FromQuery] AdminReportQueryDto query,
        CancellationToken ct) =>
        ExecuteAsync(async () =>
        {
            var file = await _exports.ExportAsync(reportType, format, query, ct);
            return File(file.Content, file.ContentType, file.FileName);
        });

    [HttpGet("{reportType}/insights")]
    public Task<IActionResult> CachedInsight(
        string reportType,
        [FromQuery] AdminReportQueryDto query,
        CancellationToken ct) =>
        ExecuteAsync(async () =>
        {
            var insight = await _insights.GetCachedAsync(reportType, query, ct);
            return OkResponse<AdminAiInsightDto?>(insight);
        });

    [HttpPost("{reportType}/insights")]
    [EnableRateLimiting("admin-ai-insights")]
    [AdminAuditAction(AdminAuditActions.AiInsightGenerated, AdminAuditModules.Reports, AdminAuditRiskLevel.Low, EntityType = "AdminAiInsight", RouteIdName = "reportType")]
    public Task<IActionResult> GenerateInsight(
        string reportType,
        [FromQuery] bool forceRefresh,
        [FromQuery] AdminReportQueryDto query,
        CancellationToken ct) =>
        ExecuteAsync(async () =>
        {
            var insight = await _insights.GenerateAsync(reportType, query, forceRefresh, ct);
            var auditEvent = HttpContext.RequestServices
                .GetService<AdminAuditContext>()?
                .Current?.Event;
            if (auditEvent is not null)
            {
                auditEvent.Metadata["provider"] = insight.Provider;
                auditEvent.Metadata["model"] = insight.Model;
                auditEvent.Metadata["cacheHit"] = insight.IsCached.ToString();
                auditEvent.Metadata["hasEnoughData"] = insight.HasEnoughData.ToString();
            }

            return OkResponse(insight);
        });

    private async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (ArgumentException ex)
        {
            return FailResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (AiInsightUnavailableException ex)
        {
            return FailResponse(ex.Message, StatusCodes.Status503ServiceUnavailable);
        }
    }
}
