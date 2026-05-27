using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Brands
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminBrandService _adminBrandService;

        public IndexModel(IAdminBrandService adminBrandService)
        {
            _adminBrandService = adminBrandService;
        }

        [BindProperty(SupportsGet = true)]
        public AdminBrandQueryDto Query { get; set; } = new AdminBrandQueryDto();

        [BindProperty]
        public CreateBrandDto CreateInput { get; set; } = new CreateBrandDto();

        [BindProperty]
        public EditBrandDto EditInput { get; set; } = new EditBrandDto();

        public PagedResult<AdminBrandDto> Brands { get; set; } = new PagedResult<AdminBrandDto>();
        public AdminBrandStatsDto BrandStats { get; set; } = new AdminBrandStatsDto();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Validation failed. Please check the inputs.";
                return RedirectToPage();
            }

            var result = await _adminBrandService.CreateBrandAsync(CreateInput);
            if (result)
            {
                TempData["SuccessMessage"] = "Brand created successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to create brand. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Validation failed. Please check the inputs.";
                return RedirectToPage();
            }

            var result = await _adminBrandService.UpdateBrandAsync(EditInput);
            if (result)
            {
                TempData["SuccessMessage"] = "Brand updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update brand. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var result = await _adminBrandService.ApproveBrandAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Brand approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve brand. It might not exist or is already approved.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var result = await _adminBrandService.RejectBrandAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Brand rejected successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject brand. It might not exist or is already rejected.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _adminBrandService.DeleteBrandAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Brand deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete brand. It might not exist or is already deleted.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetBrandDetailsAsync(int brandId)
        {
            var brand = await _adminBrandService.GetBrandByIdAsync(brandId);
            if (brand == null) return NotFound();

            return new JsonResult(brand);
        }

        private async Task LoadDataAsync()
        {
            if (Query.PageNumber < 1) Query.PageNumber = 1;
            if (Query.PageSize < 1) Query.PageSize = 10;

            Brands = await _adminBrandService.GetBrandsAsync(Query);
            BrandStats = await _adminBrandService.GetBrandStatsAsync();
        }
    }
}
