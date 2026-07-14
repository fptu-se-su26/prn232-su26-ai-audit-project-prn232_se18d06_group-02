using System.Security.Claims;
using System.Text;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Reports
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private readonly ISellerReportService _reportService;

        public IndexModel(ISellerReportService reportService)
        {
            _reportService = reportService;
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

        private SellerReportQueryDto BuildQuery() => new()
        {
            Range = Range,
            Granularity = Granularity,
            From = From,
            To = To,
        };

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToPage("/Public/Auth/Login");

            Tab = NormalizeTab(Tab);
            var q = BuildQuery();

            switch (Tab)
            {
                case "products":
                    Products = await _reportService.GetProductPerformanceAsync(userId, q, ct);
                    HasStore = Products.HasStore;
                    break;
                case "operations":
                    Operations = await _reportService.GetOperationsReportAsync(userId, q, ct);
                    HasStore = Operations.HasStore;
                    break;
                case "customers":
                    Customers = await _reportService.GetCustomerReportAsync(userId, q, ct);
                    HasStore = Customers.HasStore;
                    break;
                case "marketing":
                    Marketing = await _reportService.GetMarketingReportAsync(userId, q, ct);
                    HasStore = Marketing.HasStore;
                    break;
                case "reviews":
                    Reviews = await _reportService.GetReviewsReportAsync(userId, q, ct);
                    HasStore = Reviews.HasStore;
                    break;
                default:
                    Sales = await _reportService.GetSalesReportAsync(userId, q, ct);
                    HasStore = Sales.HasStore;
                    break;
            }

            return Page();
        }

        public async Task<IActionResult> OnGetExportAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToPage("/Public/Auth/Login");

            var csv = await _reportService.GetSalesCsvAsync(userId, BuildQuery(), ct);
            // Prepend BOM so Excel opens UTF-8 correctly.
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
            return File(bytes, "text/csv", $"sales-report-{DateTime.UtcNow:yyyyMMdd}.csv");
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
