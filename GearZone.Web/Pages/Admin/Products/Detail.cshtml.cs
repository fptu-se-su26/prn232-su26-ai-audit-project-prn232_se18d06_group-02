using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;

namespace GearZone.Web.Pages.Admin.Products
{
    [Authorize(Roles = "Super Admin")]
    public class DetailModel : PageModel
    {
        private readonly IAdminProductService _productService;

        public DetailModel(IAdminProductService productService)
        {
            _productService = productService;
        }

        public AdminProductDetailDto Product { get; set; } = new AdminProductDetailDto();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return RedirectToPage("./Index");
            }

            var product = await _productService.GetProductDetailAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            Product = product;
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id)
        {
            var success = await _productService.BulkUpdateStatusAsync(new List<Guid> { id }, ProductStatus.Active);
            if (success)
                TempData["SuccessMessage"] = "Product approved successfully.";
            else
                TempData["ErrorMessage"] = "Failed to approve product.";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id, string? reason = null)
        {
            var success = await _productService.BulkUpdateStatusAsync(new List<Guid> { id }, ProductStatus.Rejected, reason);
            if (success)
                TempData["SuccessMessage"] = "Product rejected.";
            else
                TempData["ErrorMessage"] = "Failed to reject product.";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostSuspendAsync(Guid id, string? reason = null)
        {
            var success = await _productService.BulkUpdateStatusAsync(new List<Guid> { id }, ProductStatus.Inactive, reason);
            if (success)
                TempData["SuccessMessage"] = "Product suspended.";
            else
                TempData["ErrorMessage"] = "Failed to suspend product.";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id, string reason)
        {
            var success = await _productService.DeleteProductAsync(id, reason);
            if (success)
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