using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Admin.StoreManagement
{
    [Authorize(Roles = "Super Admin")]
    public class DetailModel : PageModel
    {
        private const int MaxReasonLength = 500;

        private readonly IApiClient _api;

        public DetailModel(IApiClient api)
        {
            _api = api;
        }

        public StoreApplicationDto? StoreApplication { get; set; }
        public PagedResult<AdminProductDto> Products { get; set; } = new(new List<AdminProductDto>(), 0, 1, 5);
        public PagedResult<AdminOrderDto> Orders { get; set; } = new(new List<AdminOrderDto>(), 0, 1, 5);

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            var productQuery = new AdminProductQueryDto
            {
                StoreId = id,
                PageNumber = 1,
                PageSize = 5,
                SortBy = "createdAt",
                SortDirection = "desc"
            };
            var orderQuery = new AdminOrderQueryDto
            {
                StoreId = id,
                PageNumber = 1,
                PageSize = 5,
                SortBy = "createdAt",
                SortDirection = "desc"
            };

            var storeTask = _api.GetAsync<StoreApplicationDto>($"/api/admin/stores/{id}", ct);
            var productsTask = _api.GetAsync<AdminProductListResponseDto>(
                $"/api/admin/products{ApiQueryStringBuilder.Build(productQuery)}", ct);
            var ordersTask = _api.GetAsync<AdminOrderListResponseDto>(
                $"/api/admin/orders{ApiQueryStringBuilder.Build(orderQuery)}", ct);
            await Task.WhenAll(storeTask, productsTask, ordersTask);

            StoreApplication = await storeTask;

            if (StoreApplication == null)
                return NotFound();

            Products = (await productsTask)?.Products ?? Products;
            Orders = (await ordersTask)?.Orders ?? Orders;

            return Page();
        }

        public async Task<IActionResult> OnPostChangeStatusAsync(
            Guid id, StoreStatus status, string reason = "", CancellationToken ct = default)
        {
            var normalizedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            if ((status == StoreStatus.Locked || status == StoreStatus.Rejected) && string.IsNullOrWhiteSpace(normalizedReason))
            {
                TempData["ErrorMessage"] = "Reason is required for this status change.";
                return RedirectToPage(new { id });
            }

            if (normalizedReason?.Length > MaxReasonLength)
            {
                TempData["ErrorMessage"] = $"Reason cannot exceed {MaxReasonLength} characters.";
                return RedirectToPage(new { id });
            }

            var result = await _api.PostAsync(
                $"/api/admin/stores/{id}/change-status",
                new { status, reason = normalizedReason }, ct);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = "Failed to change store status.";
                return RedirectToPage(new { id });
            }

            TempData["SuccessMessage"] = $"Store status has been changed to {status}.";
            return RedirectToPage(new { id });
        }
    }
}
