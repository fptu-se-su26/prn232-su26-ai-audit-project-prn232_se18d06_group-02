using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
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
    public class CreateModel : PageModel
    {
        private readonly IApiClient _api;

        public CreateModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public CreateVoucherDto Input { get; set; } = new();

        public List<CategoryDto> Categories { get; set; } = new();

        public async Task OnGetAsync(Guid? copyFromId = null, CancellationToken ct = default)
        {
            Categories = await LoadCategoriesAsync(ct);
            
            if (copyFromId.HasValue)
            {
                var sourceVoucher = await _api.GetAsync<AdminVoucherDto>(
                    $"/api/admin/vouchers/{copyFromId.Value}", ct);
                if (sourceVoucher != null)
                {
                    Input = new CreateVoucherDto
                    {
                        Name = sourceVoucher.Name + " (Copy)",
                        Description = sourceVoucher.Description,
                        Type = sourceVoucher.Type.ToString(),
                        DiscountType = sourceVoucher.DiscountType.ToString(),
                        DiscountValue = sourceVoucher.DiscountValue,
                        MaxDiscount = sourceVoucher.MaxDiscount,
                        MinOrderAmount = sourceVoucher.MinOrderAmount ?? 0,
                        UsageLimit = sourceVoucher.UsageLimit,
                        CategoryId = sourceVoucher.CategoryId,
                        IsVisible = true,
                        // Code is intentionally left blank for the user to enter
                        StartAt = DateTime.Now,
                        EndAt = DateTime.Now.AddDays(30)
                    };
                    return;
                }
            }

            // Set default values for new voucher
            Input.StartAt = DateTime.Now;
            Input.EndAt = DateTime.Now.AddDays(30);
            Input.UsageLimit = 1000;
            Input.DiscountType = "Percent";
            Input.DiscountValue = 10;
            Input.MinOrderAmount = 50000;
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

            var result = await _api.PostAsync("/api/admin/vouchers", Input, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Voucher created successfully!";
                return RedirectToPage("./Index");
            }

            var error = result.FirstError ?? "Failed to create voucher. Please check the information and try again.";
            TempData["ErrorMessage"] = error;
            ModelState.AddModelError("", error);
            Categories = await LoadCategoriesAsync(ct);
            return Page();
        }

        private async Task<List<CategoryDto>> LoadCategoriesAsync(CancellationToken ct) =>
            await _api.GetAsync<List<CategoryDto>>("/api/admin/categories/all", ct) ?? new();
    }
}
