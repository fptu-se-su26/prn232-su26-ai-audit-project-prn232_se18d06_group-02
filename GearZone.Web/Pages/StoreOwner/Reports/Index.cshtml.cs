using System.Security.Claims;
using System.Text;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Reports
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {

        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public string Tab { get; set; } = "sales";

        [BindProperty(SupportsGet = true)]
        public string Range { get; set; } = "30d";

        [BindProperty(SupportsGet = true)]
        public string? Granularity { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        public bool HasStore { get; set; }

        public SalesReportDto? Sales { get; private set; }
        public ProductPerformanceReportDto? Products { get; private set; }
        public OperationsReportDto? Operations { get; private set; }
        public CustomerReportDto? Customers { get; private set; }
        public MarketingReportDto? Marketing { get; private set; }
        public ReviewsReportDto? Reviews { get; private set; }

        public static readonly (string Key, string Label, string Icon)[] Tabs =
        {
            ("sales", "Sales", "trending_up"),
            ("products", "Products", "inventory_2"),
            ("operations", "Operations", "local_shipping"),
            ("customers", "Customers", "group"),
            ("marketing", "Marketing", "sell"),
            ("reviews", "Reviews", "reviews"),
        };

        public static readonly (string Key, string Label)[] Ranges =
        {
            ("today", "Today"),
            ("7d", "7 days"),
            ("30d", "30 days"),
            ("thisMonth", "This month"),
            ("lastMonth", "Last month"),
        };

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToPage("/Public/Auth/Login");

            Tab = NormalizeTab(Tab);
            var qs = BuildQueryString();

            switch (Tab)
            {
                case "products":
                    Products = await _api.GetAsync<ProductPerformanceReportDto>($"/api/seller/reports/products{qs}", ct);
                    HasStore = Products?.HasStore ?? false;
                    break;
                case "operations":
                    Operations = await _api.GetAsync<OperationsReportDto>($"/api/seller/reports/operations{qs}", ct);
                    HasStore = Operations?.HasStore ?? false;
                    break;
                case "customers":
                    Customers = await _api.GetAsync<CustomerReportDto>($"/api/seller/reports/customers{qs}", ct);
                    HasStore = Customers?.HasStore ?? false;
                    break;
                case "marketing":
                    Marketing = await _api.GetAsync<MarketingReportDto>($"/api/seller/reports/marketing{qs}", ct);
                    HasStore = Marketing?.HasStore ?? false;
                    break;
                case "reviews":
                    Reviews = await _api.GetAsync<ReviewsReportDto>($"/api/seller/reports/reviews{qs}", ct);
                    HasStore = Reviews?.HasStore ?? false;
                    break;
                default:
                    Sales = await _api.GetAsync<SalesReportDto>($"/api/seller/reports/sales{qs}", ct);
                    HasStore = Sales?.HasStore ?? false;
                    break;
            }

            return Page();
        }

        public async Task<IActionResult> OnGetExportAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToPage("/Public/Auth/Login");

            var raw = await _api.GetBytesAsync($"/api/seller/reports/sales/export{BuildQueryString()}", ct);
            // Prepend BOM so Excel opens UTF-8 correctly.
            var bytes = Encoding.UTF8.GetPreamble().Concat(raw).ToArray();
            return File(bytes, "text/csv", $"sales-report-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        private string BuildQueryString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Range)) parts.Add($"range={Uri.EscapeDataString(Range)}");
            if (!string.IsNullOrWhiteSpace(Granularity)) parts.Add($"granularity={Uri.EscapeDataString(Granularity)}");
            if (From.HasValue) parts.Add($"from={From.Value:yyyy-MM-dd}");
            if (To.HasValue) parts.Add($"to={To.Value:yyyy-MM-dd}");
            return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
        }

        private static string NormalizeTab(string tab)
        {
            tab = (tab ?? "sales").ToLowerInvariant();
            return Tabs.Any(t => t.Key == tab) ? tab : "sales";
        }

        // ---- View helpers ----
        public string ChangeClass(double? pct) =>
            pct is null ? "text-slate-400" : pct >= 0 ? "text-emerald-600" : "text-red-600";

        public string ChangeText(double? pct) =>
            pct is null ? "—" : $"{(pct >= 0 ? "▲" : "▼")} {Math.Abs(pct.Value):0.0}%";
    }
}
