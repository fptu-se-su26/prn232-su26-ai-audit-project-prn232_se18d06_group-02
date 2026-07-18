using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Categories
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public CategoryQueryDto Query { get; set; } = new CategoryQueryDto();

        public List<CategoryDto> HierarchicalCategories { get; set; } = new();
        public int TotalCount { get; set; }

        public async Task OnGetAsync(CancellationToken ct)
        {
            await LoadDataAsync(ct);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
        {
            var result = await _api.DeleteAsync($"/api/admin/categories/{id}", ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete category.";
            }
            return RedirectToPage();
        }

        private async Task LoadDataAsync(CancellationToken ct)
        {
            HierarchicalCategories = await _api.GetAsync<List<CategoryDto>>(
                $"/api/admin/categories{ApiQueryStringBuilder.Build(Query)}", ct) ?? new();
            TotalCount = HierarchicalCategories.Count + HierarchicalCategories.Sum(c => c.Children.Count);
        }
    }
}
