using System.Security.Claims;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Vouchers
{
    [Authorize(Roles = "Store Owner")]
    public class CreateModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of calling the voucher service in-process.
        private readonly IApiClient _api;

        public CreateModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public SellerCreateVoucherDto Input { get; set; } = new();

        public List<CategoryDto> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid? copyFromId, CancellationToken ct)
        {
            Categories = await LoadCategoriesAsync(ct);

            if (copyFromId.HasValue)
            {
                var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(ownerUserId))
                {
                    return RedirectToPage("/Public/Auth/Login");
                }

                var sourceVoucher = await _api.GetAsync<SellerVoucherDto>($"/api/seller/vouchers/{copyFromId.Value}", ct);
                if (sourceVoucher != null)
                {
                    Input = new SellerCreateVoucherDto
                    {
                        Name = sourceVoucher.Name + " (Copy)",
                        Description = sourceVoucher.Description,
                        Type = "Order",
                        DiscountType = sourceVoucher.DiscountType.ToString(),
                        DiscountValue = sourceVoucher.DiscountValue,
                        MaxDiscount = sourceVoucher.MaxDiscount,
                        MinOrderAmount = sourceVoucher.MinOrderAmount ?? 0,
                        UsageLimit = sourceVoucher.UsageLimit,
                        MaxUsagePerUser = sourceVoucher.MaxUsagePerUser,
                        CategoryId = sourceVoucher.CategoryId,
                        IsVisible = true,
                        StartAt = DateTime.Now,
                        EndAt = DateTime.Now.AddDays(30)
                    };

                    return Page();
                }
            }

            Input.StartAt = DateTime.Now;
            Input.EndAt = DateTime.Now.AddDays(30);
            Input.DiscountType = "Percent";
            Input.DiscountValue = 10;
            Input.MinOrderAmount = 50000;
            Input.UsageLimit = 100;
            Input.MaxUsagePerUser = 1;
            Input.IsVisible = true;

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

            var result = await _api.PostAsync("/api/seller/vouchers", Input, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Voucher created successfully.";
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
