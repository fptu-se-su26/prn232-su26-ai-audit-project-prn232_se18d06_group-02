using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace GearZone.Web.Pages.Admin.AuditLogs;

[Authorize(Roles = "Super Admin")]
public sealed class IndexModel : PageModel
{
    private readonly IApiClient _api;

    public IndexModel(IApiClient api)
    {
        _api = api;
    }

    [BindProperty(SupportsGet = true)]
    public AdminAuditQueryDto Query { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string Range { get; set; } = "7d";

    public PagedResult<AdminAuditLogListItemDto> Logs { get; private set; } = new();
    public AdminAuditSummaryDto Summary { get; private set; } = new();
    public AdminAuditFilterOptionsDto FilterOptions { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        ApplyRange();
        Query.PageNumber = Math.Max(Query.PageNumber, 1);
        Query.PageSize = Math.Clamp(Query.PageSize, 1, 100);
        var apiQuery = ApiQueryStringBuilder.Build(Query);

        try
        {
            var logsTask = _api.GetAsync<PagedResult<AdminAuditLogListItemDto>>($"/api/admin/audit-logs{apiQuery}", ct);
            var summaryTask = _api.GetAsync<AdminAuditSummaryDto>($"/api/admin/audit-logs/summary{apiQuery}", ct);
            var optionsTask = _api.GetAsync<AdminAuditFilterOptionsDto>("/api/admin/audit-logs/filter-options", ct);
            await Task.WhenAll(logsTask, summaryTask, optionsTask);
            Logs = await logsTask ?? Logs;
            Summary = await summaryTask ?? Summary;
            FilterOptions = await optionsTask ?? FilterOptions;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.StatusCode == System.Net.HttpStatusCode.BadRequest
                ? "The selected audit filters are invalid. Custom ranges cannot exceed 366 days."
                : "Audit logs are temporarily unavailable. Apply the audit migration and verify that GearZone.Api is running.";
        }
    }

    public async Task<IActionResult> OnGetExportAsync(CancellationToken ct)
    {
        ApplyRange();
        var apiQuery = ApiQueryStringBuilder.Build(Query).TrimStart('?');
        var path = "/api/admin/audit-logs/export?format=csv" +
                   (apiQuery.Length == 0 ? string.Empty : $"&{apiQuery}");
        try
        {
            var file = await _api.GetFileAsync(path, ct);
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = "The audit log export could not be generated.";
            return Redirect(BuildPageUrl(Query.PageNumber));
        }
    }

    public string BuildPageUrl(int pageNumber)
    {
        var values = QueryValues(pageNumber);
        return QueryHelpers.AddQueryString("/admin/audit-logs", values);
    }

    public string BuildExportUrl()
    {
        var values = QueryValues(Query.PageNumber);
        values["handler"] = "Export";
        return QueryHelpers.AddQueryString("/admin/audit-logs", values);
    }

    private Dictionary<string, string?> QueryValues(int pageNumber) => new()
    {
        ["Range"] = Range,
        ["Query.From"] = Query.From?.ToString("yyyy-MM-dd"),
        ["Query.To"] = Query.To?.ToString("yyyy-MM-dd"),
        ["Query.Search"] = Query.Search,
        ["Query.ActorUserId"] = Query.ActorUserId,
        ["Query.Module"] = Query.Module,
        ["Query.Action"] = Query.Action,
        ["Query.Outcome"] = Query.Outcome?.ToString(),
        ["Query.RiskLevel"] = Query.RiskLevel?.ToString(),
        ["Query.EntityType"] = Query.EntityType,
        ["Query.EntityId"] = Query.EntityId,
        ["Query.PageNumber"] = pageNumber.ToString(),
        ["Query.PageSize"] = Query.PageSize.ToString()
    };

    private void ApplyRange()
    {
        var vietnamNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            DateTime.UtcNow,
            OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        var today = DateOnly.FromDateTime(vietnamNow);
        switch (Range?.ToLowerInvariant())
        {
            case "today":
                Query.From = Query.To = today;
                break;
            case "30d":
                Query.From = today.AddDays(-29);
                Query.To = today;
                break;
            case "custom":
                break;
            default:
                Range = "7d";
                Query.From = today.AddDays(-6);
                Query.To = today;
                break;
        }
    }
}
