using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;

namespace GearZone.Web.Pages.Admin.Products
{
    [Authorize(Roles = "Super Admin")]
    public class DetailModel : PageModel
    {
        private readonly IApiClient _api;

        public DetailModel(IApiClient api)
        {
            _api = api;
        }

        public AdminProductDetailDto Product { get; set; } = new AdminProductDetailDto();

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty)
            {
                return RedirectToPage("./Index");
            }

            var product = await _api.GetAsync<AdminProductDetailDto>($"/api/admin/products/{id}", ct);
            if (product == null)
            {
                return NotFound();
            }

            Product = product;
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id, CancellationToken ct)
        {
            var result = await _api.PostAsync($"/api/admin/products/{id}/approve", ct);
            if (result.Success)
                TempData["SuccessMessage"] = "Product approved successfully.";
            else
                TempData["ErrorMessage"] = "Failed to approve product.";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id, string? reason = null, CancellationToken ct = default)
        {
            var result = await _api.PostAsync($"/api/admin/products/{id}/reject", new { reason }, ct);
            if (result.Success)
                TempData["SuccessMessage"] = "Product rejected.";
            else
                TempData["ErrorMessage"] = "Failed to reject product.";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostSuspendAsync(Guid id, string? reason = null, CancellationToken ct = default)
        {
            var result = await _api.PostAsync($"/api/admin/products/{id}/suspend", new { reason }, ct);
            if (result.Success)
                TempData["SuccessMessage"] = "Product suspended.";
            else
                TempData["ErrorMessage"] = "Failed to suspend product.";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id, string reason, CancellationToken ct)
        {
            var result = await _api.DeleteAsync($"/api/admin/products/{id}", new { reason }, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Product deleted.";
                return RedirectToPage("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete product.";
                return RedirectToPage(new { id });
            }
        }
    }
}
