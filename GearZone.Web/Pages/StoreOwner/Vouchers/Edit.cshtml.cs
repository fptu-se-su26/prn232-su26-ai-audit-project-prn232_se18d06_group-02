using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.StoreOwner.Vouchers
{
    [Authorize(Roles = "Store Owner")]
    public class EditModel : PageModel
    {
        private readonly ISellerVoucherService _sellerVoucherService;
        private readonly IAdminCategoryService _categoryService;

        public EditModel(ISellerVoucherService sellerVoucherService, IAdminCategoryService categoryService)
        {
            _sellerVoucherService = sellerVoucherService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public SellerUpdateVoucherDto Input { get; set; } = new();

        [BindProperty]
        public Guid Id { get; set; }

        public List<CategoryDto> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var voucher = await _sellerVoucherService.GetVoucherByIdAsync(ownerUserId, id);
            if (voucher == null)
            {
                TempData["ErrorMessage"] = "Voucher not found.";
                return RedirectToPage("./Index");
            }

            Id = id;
            Categories = await _categoryService.GetAllCategoriesListAsync();

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

            var (success, error) = await _sellerVoucherService.UpdateVoucherAsync(ownerUserId, Id, Input);
            if (success)
            {
                TempData["SuccessMessage"] = "Voucher updated successfully.";
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
