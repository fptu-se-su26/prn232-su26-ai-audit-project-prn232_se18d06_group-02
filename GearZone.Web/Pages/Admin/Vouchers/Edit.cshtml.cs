using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Admin.Vouchers
{
    [Authorize(Roles = "Super Admin")]
    public class EditModel : PageModel
    {
        private readonly IApiClient _api;

        public EditModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public UpdateVoucherDto Input { get; set; } = new();

        [BindProperty]
        public Guid Id { get; set; }

        public List<CategoryDto> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            var voucher = await _api.GetAsync<AdminVoucherDto>($"/api/admin/vouchers/{id}", ct);
            if (voucher == null)
            {
                TempData["ErrorMessage"] = "Voucher not found.";
                return RedirectToPage("./Index");
            }

            Id = id;
            Categories = await LoadCategoriesAsync(ct);
            
            // Map AdminVoucherDto properties to UpdateVoucherDto
            Input.Name = voucher.Name;
            Input.Code = voucher.Code;
            Input.Description = voucher.Description;
            Input.Type = voucher.Type.ToString() == "ShippingDiscount" ? "Shipping" : "Order";
            Input.DiscountType = voucher.DiscountType.ToString() == "Percent" ? "Percent" : "Fixed";
            Input.DiscountValue = voucher.DiscountValue;
            Input.MaxDiscount = voucher.MaxDiscount;
            Input.MinOrderAmount = voucher.MinOrderAmount ?? 0;
            Input.UsageLimit = voucher.UsageLimit;
            Input.MaxUsagePerUser = voucher.MaxUsagePerUser;
            Input.StartAt = voucher.StartAt;
            Input.EndAt = voucher.EndAt;
            Input.CategoryId = voucher.CategoryId;
            Input.IsVisible = voucher.IsActive;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            // Custom Validation
            if (Input.DiscountType == "Percent" && Input.DiscountValue > 100)
            {
                ModelState.AddModelError("Input.DiscountValue", "Percentage must be less than or equal to 100%");
            }
            if (Input.DiscountType == "Fixed" && Input.MinOrderAmount <= Input.DiscountValue)
            {
                ModelState.AddModelError("Input.MinOrderAmount", "Minimum spend must be greater than discount amount");
            }
            if (Input.EndAt <= Input.StartAt)
            {
                ModelState.AddModelError("Input.EndAt", "Expiration date must be later than launch date");
            }

            if (Input.DiscountType == "Fixed")
            {
                Input.MaxDiscount = null;
            }

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                TempData["ErrorMessage"] = firstError ?? "Please check the form for errors.";
                Categories = await LoadCategoriesAsync(ct);
                return Page();
            }

            var result = await _api.PutAsync($"/api/admin/vouchers/{Id}", Input, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Voucher updated successfully!";
                return RedirectToPage("./Index");
            }

            var error = result.FirstError ?? "Failed to update voucher. Please try again.";
            TempData["ErrorMessage"] = error;
            ModelState.AddModelError(string.Empty, error);
            Categories = await LoadCategoriesAsync(ct);
            return Page();
        }

        private async Task<List<CategoryDto>> LoadCategoriesAsync(CancellationToken ct) =>
            await _api.GetAsync<List<CategoryDto>>("/api/admin/categories/all", ct) ?? new();
    }
}
