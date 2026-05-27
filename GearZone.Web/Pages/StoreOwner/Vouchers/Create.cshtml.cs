using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.StoreOwner.Vouchers
{
    [Authorize(Roles = "Store Owner")]
    public class CreateModel : PageModel
    {
        private readonly ISellerVoucherService _sellerVoucherService;
        private readonly IAdminCategoryService _categoryService;

        public CreateModel(ISellerVoucherService sellerVoucherService, IAdminCategoryService categoryService)
        {
            _sellerVoucherService = sellerVoucherService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public SellerCreateVoucherDto Input { get; set; } = new();

        public List<CategoryDto> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid? copyFromId = null)
        {
            Categories = await _categoryService.GetAllCategoriesListAsync();

            if (copyFromId.HasValue)
            {
                var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(ownerUserId))
                {
                    return RedirectToPage("/Public/Auth/Login");
                }

                var sourceVoucher = await _sellerVoucherService.GetVoucherByIdAsync(ownerUserId, copyFromId.Value);
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

        public async Task<IActionResult> OnPostAsync()
        {
            Categories = await _categoryService.GetAllCategoriesListAsync();

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

            var (success, error) = await _sellerVoucherService.CreateVoucherAsync(ownerUserId, Input);
            if (success)
            {
                TempData["SuccessMessage"] = "Voucher created successfully.";
                return RedirectToPage("./Index");
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                ModelState.AddModelError(string.Empty, error);
                TempData["ErrorMessage"] = error;
            }

            return Page();
        }
    }
}
