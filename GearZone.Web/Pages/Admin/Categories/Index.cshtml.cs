using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Categories
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminCategoryService _adminCategoryService;

        public IndexModel(IAdminCategoryService adminCategoryService)
        {
            _adminCategoryService = adminCategoryService;
        }

        [BindProperty(SupportsGet = true)]
        public CategoryQueryDto Query { get; set; } = new CategoryQueryDto();

        public List<CategoryDto> HierarchicalCategories { get; set; } = new();
        public int TotalCount { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _adminCategoryService.SoftDeleteCategoryAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete category.";
            }
            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            HierarchicalCategories = await _adminCategoryService.GetHierarchicalCategoriesAsync(Query);
            TotalCount = HierarchicalCategories.Count + HierarchicalCategories.Sum(c => c.Children.Count);
        }
    }
}
