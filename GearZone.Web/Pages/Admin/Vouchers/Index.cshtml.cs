using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Admin.Vouchers
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminVoucherService _voucherService;
        private readonly IAdminCategoryService _categoryService;

        public IndexModel(IAdminVoucherService voucherService, IAdminCategoryService categoryService)
        {
            _voucherService = voucherService;
            _categoryService = categoryService;
        }

        public PagedResult<AdminVoucherDto> PagedVouchers { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public AdminVoucherSummaryDto Summary { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public AdminVoucherQueryDto Query { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SortOption { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRange { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(SortOption))
            {
                var parts = SortOption.Split('-');
                if (parts.Length == 2)
                {
                    Query.SortBy = parts[0];
                    Query.SortDirection = parts[1];
                }
            }

            if (!string.IsNullOrEmpty(DateRange))
            {
                var dates = DateRange.Split(" to ");
                if (dates.Length == 2)
                {
                    if (DateTime.TryParse(dates[0], out var start)) Query.StartDate = start;
                    if (DateTime.TryParse(dates[1], out var end)) Query.EndDate = end;
                }
                else if (dates.Length == 1)
                {
                    if (DateTime.TryParse(dates[0], out var start)) Query.StartDate = start;
                }
            }

            var pagedResult = await _voucherService.GetPaginatedVouchersAsync(Query);
            PagedVouchers = pagedResult;
            
            Summary = await _voucherService.GetVoucherSummaryAsync();
            Categories = await _categoryService.GetAllCategoriesListAsync();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id)
        {
            var result = await _voucherService.ToggleVoucherStatusAsync(id);
            if (result)
            {
                TempData["ToastMessage"] = "Voucher status updated successfully!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Failed to update voucher status.";
                TempData["ToastType"] = "error";
            }
            return RedirectToPage();
        }
    }
}
