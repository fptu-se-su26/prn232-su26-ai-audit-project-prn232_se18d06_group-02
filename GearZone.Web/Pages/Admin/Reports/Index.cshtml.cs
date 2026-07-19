using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace GearZone.Web.Pages.Admin.Reports;

[Authorize(Roles = "Super Admin")]
public sealed class IndexModel : PageModel
{
    private static readonly HashSet<string> Tabs = new(StringComparer.OrdinalIgnoreCase)
    {
        "overview", "orders", "sellers"
    };

    private readonly IApiClient _api;

    public IndexModel(IApiClient api)
    {
        _api = api;
    }

    [BindProperty(SupportsGet = true)]
    public string Tab { get; set; } = "overview";

    [BindProperty(SupportsGet = true)]
    public AdminReportQueryDto Query { get; set; } = new();

    public AdminOverviewReportDto? Overview { get; private set; }
    public AdminOrderReportDto? Orders { get; private set; }
    public AdminSellerReportDto? Sellers { get; private set; }
    public AdminAiInsightDto? Insight { get; private set; }
    public string? ErrorMessage { get; private set; }

    public string BuildPageUrl(string tab, int? pageNumber = null)
    {
        var values = PageQueryValues(tab, pageNumber);
        return "/admin/reports" + QueryString.Create(values).ToUriComponent();
    }

    public string BuildExportUrl(string format)
    {
        var values = PageQueryValues(Tab, Query.PageNumber);
        values.Insert(0, new("handler", "Export"));
        values.Insert(1, new("format", format));
        return "/admin/reports" + QueryString.Create(values).ToUriComponent();
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        NormalizeTab();
        try
        {
            var queryString = ApiQueryStringBuilder.Build(Query);
            var insightTask = _api.GetAsync<AdminAiInsightDto>(
                $"/api/admin/reports/{Tab}/insights{queryString}", ct);

            switch (Tab)
            {
                case "orders":
                    Orders = await _api.GetAsync<AdminOrderReportDto>(
                        $"/api/admin/reports/orders{queryString}", ct);
                    break;
                case "sellers":
                    Sellers = await _api.GetAsync<AdminSellerReportDto>(
                        $"/api/admin/reports/sellers{queryString}", ct);
                    break;
                default:
                    Overview = await _api.GetAsync<AdminOverviewReportDto>(
                        $"/api/admin/reports/overview{queryString}", ct);
                    break;
            }

            Insight = await insightTask;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.StatusCode == System.Net.HttpStatusCode.BadRequest
                ? "The selected report filters are invalid. Check the date range and try again."
                : "Reports are temporarily unavailable. Please try again.";
        }
    }

    public async Task<IActionResult> OnGetExportAsync(string format, CancellationToken ct)
    {
        NormalizeTab();
        format = format.ToLowerInvariant();
        if (format is not ("csv" or "xlsx" or "pdf")) return BadRequest();

        var queryString = ApiQueryStringBuilder.Build(Query).TrimStart('?');
        var path = $"/api/admin/reports/{Tab}/export?format={format}" +
                   (queryString.Length == 0 ? string.Empty : $"&{queryString}");

        try
        {
            var file = await _api.GetFileAsync(path, ct);
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = "The report could not be exported. Please check the filters and try again.";
            return RedirectToPage(RouteValues());
        }
    }

    public async Task<IActionResult> OnPostGenerateInsightAsync(bool forceRefresh, CancellationToken ct)
    {
        NormalizeTab();
        var queryString = ApiQueryStringBuilder.Build(Query).TrimStart('?');
        var path = $"/api/admin/reports/{Tab}/insights?forceRefresh={forceRefresh.ToString().ToLowerInvariant()}" +
                   (queryString.Length == 0 ? string.Empty : $"&{queryString}");

        var result = await _api.PostAndReadAsync<object, AdminAiInsightDto>(path, new { }, ct);
        var insight = result.Data;
        TempData[!result.Success || insight is null ? "ErrorMessage" : "SuccessMessage"] =
            !result.Success || insight is null
            ? result.FirstError ?? "AI insights are unavailable. Check whether the configured provider is enabled and reachable."
            : insight.HasEnoughData
                ? "AI business insights are ready."
                : "There is not enough report data to generate evidence-based insights.";

        return RedirectToPage(RouteValues());
    }

    private void NormalizeTab()
    {
        Tab = Tabs.Contains(Tab) ? Tab.ToLowerInvariant() : "overview";
    }

    private RouteValueDictionary RouteValues() => new()
    {
        ["tab"] = Tab,
        ["Query.Range"] = Query.Range,
        ["Query.From"] = Query.From?.ToString("yyyy-MM-dd"),
        ["Query.To"] = Query.To?.ToString("yyyy-MM-dd"),
        ["Query.Granularity"] = Query.Granularity,
        ["Query.Search"] = Query.Search,
        ["Query.StoreStatus"] = Query.StoreStatus,
        ["Query.SortBy"] = Query.SortBy,
        ["Query.SortDirection"] = Query.SortDirection,
        ["Query.PageNumber"] = Query.PageNumber,
        ["Query.PageSize"] = Query.PageSize
    };

    private List<KeyValuePair<string, string?>> PageQueryValues(string tab, int? pageNumber)
    {
        var values = new List<KeyValuePair<string, string?>>
        {
            new("tab", tab),
            new("Query.Range", Query.Range),
            new("Query.Granularity", Query.Granularity),
            new("Query.Search", Query.Search),
            new("Query.StoreStatus", Query.StoreStatus),
            new("Query.SortBy", Query.SortBy),
            new("Query.SortDirection", Query.SortDirection),
            new("Query.PageNumber", (pageNumber ?? Query.PageNumber).ToString()),
            new("Query.PageSize", Query.PageSize.ToString())
        };
        if (Query.From.HasValue) values.Add(new("Query.From", Query.From.Value.ToString("yyyy-MM-dd")));
        if (Query.To.HasValue) values.Add(new("Query.To", Query.To.Value.ToString("yyyy-MM-dd")));
        return values.Where(value => !string.IsNullOrWhiteSpace(value.Value)).ToList();
    }
}
