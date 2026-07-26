using System.Globalization;
using System.Security.Claims;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Vouchers
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of calling the voucher service in-process.
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        public PagedResult<SellerVoucherDto> PagedVouchers { get; set; } = new();
        public SellerVoucherSummaryDto Summary { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public SellerVoucherQueryDto Query { get; set; } = new()
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "createdAt",
            SortDirection = "desc"
        };

        [BindProperty(SupportsGet = true)]
        public string? SortOption { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRange { get; set; }

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            Query.PageNumber = Query.PageNumber < 1 ? 1 : Query.PageNumber;
            Query.PageSize = Query.PageSize < 1 ? 10 : Query.PageSize;
            Query.SortBy ??= "createdAt";
            Query.SortDirection ??= "desc";

            if (!string.IsNullOrWhiteSpace(SortOption))
            {
                var parts = SortOption.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    Query.SortBy = parts[0];
                    Query.SortDirection = parts[1];
                }
            }

            if (!string.IsNullOrWhiteSpace(DateRange))
            {
                var dates = DateRange.Split(" to ", StringSplitOptions.RemoveEmptyEntries);
                if (dates.Length == 2)
                {
                    if (DateTime.TryParse(dates[0], out var start)) Query.StartDate = start;
                    if (DateTime.TryParse(dates[1], out var end)) Query.EndDate = end;
                }
                else if (dates.Length == 1)
                {
                    if (DateTime.TryParse(dates[0], out var start))
                    {
                        Query.StartDate = start;
                        Query.EndDate = start;
                    }
                }
            }

            var data = await _api.GetAsync<SellerVoucherListDto>($"/api/seller/vouchers{BuildQueryString()}", ct);
            if (data is not null)
            {
                PagedVouchers = data.Vouchers;
                Summary = data.Summary;
                Categories = data.Categories;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id, CancellationToken ct)
        {
            var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var result = await _api.PatchAsync($"/api/seller/vouchers/{id}/toggle-status", ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Voucher status updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(result.FirstError)
                    ? "Failed to update voucher status."
                    : result.FirstError;
            }

            return RedirectToPage();
        }

        private string BuildQueryString()
        {
            var parts = new List<string>
            {
                $"pageNumber={Query.PageNumber}",
                $"pageSize={Query.PageSize}"
            };

            if (!string.IsNullOrWhiteSpace(Query.SortBy)) parts.Add($"sortBy={Uri.EscapeDataString(Query.SortBy)}");
            if (!string.IsNullOrWhiteSpace(Query.SortDirection)) parts.Add($"sortDirection={Uri.EscapeDataString(Query.SortDirection)}");
            if (!string.IsNullOrWhiteSpace(Query.Search)) parts.Add($"search={Uri.EscapeDataString(Query.Search)}");
            if (Query.Status.HasValue) parts.Add($"status={Query.Status.Value}");
            if (Query.Scope.HasValue) parts.Add($"scope={Query.Scope.Value}");
            if (Query.VoucherType.HasValue) parts.Add($"voucherType={Query.VoucherType.Value}");
            if (Query.DiscountType.HasValue) parts.Add($"discountType={Query.DiscountType.Value}");
            if (Query.CategoryId.HasValue) parts.Add($"categoryId={Query.CategoryId.Value}");
            if (Query.StartDate.HasValue) parts.Add($"startDate={Query.StartDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            if (Query.EndDate.HasValue) parts.Add($"endDate={Query.EndDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            return "?" + string.Join("&", parts);
        }

        public static string GetStatusClass(VoucherStatus status)
        {
            return status switch
            {
                VoucherStatus.Active => "bg-emerald-100 text-emerald-700",
                VoucherStatus.Upcoming => "bg-amber-100 text-amber-700",
                VoucherStatus.Expired => "bg-slate-100 text-slate-600",
                VoucherStatus.Disabled => "bg-rose-100 text-rose-700",
                VoucherStatus.Finished => "bg-indigo-100 text-indigo-700",
                _ => "bg-slate-100 text-slate-600"
            };
        }
    }
}
