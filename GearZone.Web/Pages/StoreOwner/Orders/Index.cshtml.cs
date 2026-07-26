using System.Globalization;
using System.Security.Claims;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Orders
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the chat/order services in-process.
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        public PagedResult<SellerChatOrderListItemDto> Orders { get; set; } = new();
        public SellerOrderStatsDto Stats { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public OrderStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "createdAt";

        [BindProperty(SupportsGet = true)]
        public string SortDirection { get; set; } = "desc";

        [BindProperty(SupportsGet = true)]
        public decimal? MinSubtotal { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxSubtotal { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRangeShortcut { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRange { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            PageNumber = PageNumber < 1 ? 1 : PageNumber;
            SortBy = string.IsNullOrWhiteSpace(SortBy) ? "createdAt" : SortBy;
            SortDirection = string.Equals(SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";

            var data = await _api.GetAsync<SellerOrderListDto>($"/api/seller/orders{BuildQueryString()}", ct);
            if (data is not null)
            {
                Orders = data.Orders;
                Stats = data.Stats;
            }

            return Page();
        }

        public Task<IActionResult> OnPostApproveAsync(Guid subOrderId, CancellationToken ct) =>
            RunActionAsync(subOrderId, "approve", "Order approved and moved to processing.",
                "Cannot approve this order. Only pending orders can be approved.", ct);

        public Task<IActionResult> OnPostRejectAsync(Guid subOrderId, CancellationToken ct) =>
            RunActionAsync(subOrderId, "reject", "Order rejected successfully.",
                "Cannot reject this order. Only pending orders can be rejected.", ct);

        public Task<IActionResult> OnPostMarkProcessingAsync(Guid subOrderId, CancellationToken ct) =>
            RunActionAsync(subOrderId, "mark-processing", "Order marked as processing.",
                "Cannot mark this order as processing. Valid states: Approved or Paid.", ct);

        public Task<IActionResult> OnPostMarkDeliveredAsync(Guid subOrderId, CancellationToken ct) =>
            RunActionAsync(subOrderId, "mark-delivered", "Order marked as delivered.",
                "Cannot mark this order as delivered. Valid state: Processing.", ct);

        private async Task<IActionResult> RunActionAsync(Guid subOrderId, string action, string successMessage, string errorMessage, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            var result = await _api.PostAsync($"/api/seller/orders/{subOrderId}/{action}", ct);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? successMessage : errorMessage;

            return RedirectToPage(new
            {
                SearchTerm,
                PageNumber,
                Status,
                SortBy,
                SortDirection,
                MinSubtotal,
                MaxSubtotal,
                DateRangeShortcut,
                DateRange
            });
        }

        private string BuildQueryString()
        {
            var (startDate, endDate) = ResolveDateRange();
            var parts = new List<string>
            {
                $"pageNumber={PageNumber}",
                "pageSize=10",
                $"sortBy={Uri.EscapeDataString(SortBy)}",
                $"sortDirection={Uri.EscapeDataString(SortDirection)}"
            };

            if (!string.IsNullOrWhiteSpace(SearchTerm)) parts.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
            if (Status.HasValue) parts.Add($"status={Status.Value}");
            if (MinSubtotal.HasValue) parts.Add($"minSubtotal={MinSubtotal.Value.ToString(CultureInfo.InvariantCulture)}");
            if (MaxSubtotal.HasValue) parts.Add($"maxSubtotal={MaxSubtotal.Value.ToString(CultureInfo.InvariantCulture)}");
            if (startDate.HasValue) parts.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue) parts.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            return "?" + string.Join("&", parts);
        }

        private (DateTime? StartDate, DateTime? EndDate) ResolveDateRange()
        {
            if (string.IsNullOrWhiteSpace(DateRangeShortcut))
            {
                return (null, null);
            }

            var today = DateTime.UtcNow.Date;
            return DateRangeShortcut.ToLowerInvariant() switch
            {
                "today" => (today, today),
                "week" => (today.AddDays(-7), today),
                "month" => (today.AddDays(-30), today),
                "custom" => ParseCustomDateRange(),
                _ => (null, null)
            };
        }

        private (DateTime? StartDate, DateTime? EndDate) ParseCustomDateRange()
        {
            if (string.IsNullOrWhiteSpace(DateRange))
            {
                return (null, null);
            }

            var dates = DateRange.Split(" to ", StringSplitOptions.RemoveEmptyEntries);
            if (dates.Length == 2)
            {
                var start = DateTime.TryParse(dates[0], out var parsedStart) ? parsedStart : (DateTime?)null;
                var end = DateTime.TryParse(dates[1], out var parsedEnd) ? parsedEnd : (DateTime?)null;
                return (start, end);
            }

            if (dates.Length == 1 && DateTime.TryParse(dates[0], out var singleDate))
            {
                return (singleDate, singleDate);
            }

            return (null, null);
        }
    }
}
