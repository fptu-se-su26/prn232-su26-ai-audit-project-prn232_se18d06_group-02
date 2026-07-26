using System.Security.Claims;
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
    public class EditModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of calling the voucher service in-process.
        private readonly IApiClient _api;

        public EditModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public SellerUpdateVoucherDto Input { get; set; } = new();

        [BindProperty]
        public Guid Id { get; set; }

        public List<CategoryDto> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var voucher = await _api.GetAsync<SellerVoucherDto>($"/api/seller/vouchers/{id}", ct);
            if (voucher == null)
            {
                TempData["ErrorMessage"] = "Voucher not found.";
                return RedirectToPage("./Index");
            }

            Id = id;
            Categories = await LoadCategoriesAsync(ct);

            Input.Name = voucher.Name;
            Input.Code = voucher.Code;
            Input.Description = voucher.Description;
            Input.DiscountType = voucher.DiscountType == DiscountType.FixedAmount ? "Fixed" : "Percent";
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
            Categories = await LoadCategoriesAsync(ct);

            if (Input.DiscountType == "Fixed")
            {
                Input.MaxDiscount = null;
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var result = await _api.PutAsync($"/api/seller/vouchers/{Id}", Input, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Voucher updated successfully.";
                return RedirectToPage("./Index");
            }

            var error = result.FirstError;
            if (!string.IsNullOrWhiteSpace(error))
            {
                ModelState.AddModelError(string.Empty, error);
                TempData["ErrorMessage"] = error;
            }

            return Page();
        }

        private async Task<List<CategoryDto>> LoadCategoriesAsync(CancellationToken ct) =>
            await _api.GetAsync<List<CategoryDto>>("/api/seller/vouchers/categories", ct) ?? new List<CategoryDto>();
    }
}
