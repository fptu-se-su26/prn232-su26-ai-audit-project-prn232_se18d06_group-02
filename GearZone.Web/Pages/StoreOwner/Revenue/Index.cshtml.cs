using System.Globalization;
using System.Security.Claims;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Revenue
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        // Consumes GearZone.Api over HTTP (see Phase 2 pilot) instead of querying
        // the payout repository in-process.
        private readonly IApiClient _api;

        private const int DefaultPageSize = 10;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        public bool HasStore { get; set; }
        public string StoreName { get; set; } = "Your Store";
        public SellerPayoutSummaryDto Summary { get; set; } = new();
        public PagedResult<SellerPayoutTransactionDto> Transactions { get; set; } =
            new(new List<SellerPayoutTransactionDto>(), 0, 1, DefaultPageSize);

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public PayoutTransactionStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinAmount { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxAmount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRangeShortcut { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRange { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "date";

        [BindProperty(SupportsGet = true)]
        public string SortDirection { get; set; } = "desc";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            PageNumber = PageNumber < 1 ? 1 : PageNumber;
            SortBy = string.IsNullOrWhiteSpace(SortBy) ? "date" : SortBy;
            SortDirection = string.Equals(SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";

            var data = await _api.GetAsync<SellerRevenueDto>($"/api/seller/revenue{BuildQueryString()}", ct);
            if (data is null || !data.HasStore)
            {
                return Page();
            }

            HasStore = true;
            StoreName = data.StoreName;
            Summary = data.Summary;
            Transactions = data.Transactions;

            return Page();
        }

        private string BuildQueryString()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
                parts.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
            if (Status.HasValue)
                parts.Add($"status={Status.Value}");
            if (MinAmount.HasValue)
                parts.Add($"minAmount={MinAmount.Value.ToString(CultureInfo.InvariantCulture)}");
            if (MaxAmount.HasValue)
                parts.Add($"maxAmount={MaxAmount.Value.ToString(CultureInfo.InvariantCulture)}");
            if (!string.IsNullOrWhiteSpace(DateRangeShortcut))
                parts.Add($"dateRangeShortcut={Uri.EscapeDataString(DateRangeShortcut)}");
            if (!string.IsNullOrWhiteSpace(DateRange))
                parts.Add($"dateRange={Uri.EscapeDataString(DateRange)}");

            parts.Add($"sortBy={Uri.EscapeDataString(SortBy)}");
            parts.Add($"sortDirection={Uri.EscapeDataString(SortDirection)}");
            parts.Add($"pageNumber={PageNumber}");

            return "?" + string.Join("&", parts);
        }
    }
}
