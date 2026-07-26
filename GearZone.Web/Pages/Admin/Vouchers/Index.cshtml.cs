using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Admin.Vouchers
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
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

        public async Task OnGetAsync(CancellationToken ct)
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

            Query.PageNumber = Query.PageNumber < 1 ? 1 : Query.PageNumber;
            Query.PageSize = Query.PageSize < 1 ? 10 : Query.PageSize;

            var data = await _api.GetAsync<AdminVoucherListResponseDto>(
                $"/api/admin/vouchers{ApiQueryStringBuilder.Build(Query)}", ct);
            if (data is not null)
            {
                PagedVouchers = data.Vouchers;
                Summary = data.Summary;
                Categories = data.Categories;
            }
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id, CancellationToken ct)
        {
            var result = await _api.PatchAsync($"/api/admin/vouchers/{id}/toggle-status", ct);
            if (result.Success)
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
