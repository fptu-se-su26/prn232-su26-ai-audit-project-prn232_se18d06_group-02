using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
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

        private readonly IAdminStoreService _adminStoreService;
        private readonly IAdminProductService _adminProductService;
        private readonly IAdminOrderService _adminOrderService;

        public DetailModel(IAdminStoreService adminStoreService, IAdminProductService adminProductService, IAdminOrderService adminOrderService)
        {
            _adminStoreService = adminStoreService;
            _adminProductService = adminProductService;
            _adminOrderService = adminOrderService;
        }

        public StoreApplicationDto? StoreApplication { get; set; }
        public PagedResult<AdminProductDto> Products { get; set; } = new(new List<AdminProductDto>(), 0, 1, 5);
        public PagedResult<AdminOrderDto> Orders { get; set; } = new(new List<AdminOrderDto>(), 0, 1, 5);

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            StoreApplication = await _adminStoreService.GetStoreApplicationByIdAsync(id);

            if (StoreApplication == null)
                return NotFound();

            var query = new AdminProductQueryDto 
            { 
                StoreId = id, 
                PageNumber = 1, 
                PageSize = 5,
                SortBy = "createdAt",
                SortDirection = "desc"
            };
            Products = await _adminProductService.GetProductsAsync(query);

            var orderQuery = new AdminOrderQueryDto
            {
                StoreId = id,
                PageNumber = 1,
                PageSize = 5,
                SortBy = "createdAt",
                SortDirection = "desc"
            };
            Orders = await _adminOrderService.GetOrdersAsync(orderQuery);

            return Page();
        }

        public async Task<IActionResult> OnPostChangeStatusAsync(Guid id, StoreStatus status, string reason = "")
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

            var success = await _adminStoreService.UpdateStoreStatusAsync(id, status, normalizedReason);

            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to change store status.";
                return RedirectToPage(new { id });
            }

            TempData["SuccessMessage"] = $"Store status has been changed to {status}.";
            return RedirectToPage(new { id });
        }
    }
}
