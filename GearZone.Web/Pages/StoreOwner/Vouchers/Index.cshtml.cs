using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.StoreOwner.Vouchers
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private readonly ISellerVoucherService _sellerVoucherService;
        private readonly IAdminCategoryService _categoryService;

        public IndexModel(ISellerVoucherService sellerVoucherService, IAdminCategoryService categoryService)
        {
            _sellerVoucherService = sellerVoucherService;
            _categoryService = categoryService;
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

        public async Task<IActionResult> OnGetAsync()
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

            PagedVouchers = await _sellerVoucherService.GetPaginatedVouchersAsync(ownerUserId, Query);
            Summary = await _sellerVoucherService.GetVoucherSummaryAsync(ownerUserId);
            Categories = await _categoryService.GetAllCategoriesListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id)
        {
            var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var (success, error) = await _sellerVoucherService.ToggleVoucherStatusAsync(ownerUserId, id);
            if (success)
            {
                TempData["SuccessMessage"] = "Voucher status updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(error) ? "Failed to update voucher status." : error;
            }

            return RedirectToPage();
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
