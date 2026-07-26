using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Categories
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
        public CreateCategoryDto Input { get; set; } = new();

        public List<CategoryDto> AllCategories { get; set; } = new();

        public async Task OnGetAsync(CancellationToken ct)
        {
            AllCategories = await LoadCategoriesAsync(ct);
        }

        public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                AllCategories = await LoadCategoriesAsync(ct);
                return Page();
            }

            var result = await _api.PostAndReadAsync<CreateCategoryDto, CategoryCreatedDto>(
                "/api/admin/categories", Input, ct);

            if (result.Success && result.Data is not null)
            {
                // After category created, save attributes JSON if provided
                var attrsJson = Request.Form["AttributesJson"].ToString();
                if (!string.IsNullOrWhiteSpace(attrsJson))
                {
                    var attrs = System.Text.Json.JsonSerializer.Deserialize<List<CategoryAttributeDto>>(attrsJson);
                    if (attrs is { Count: > 0 })
                    {
                        var attributesResult = await _api.PutAsync(
                            $"/api/admin/categories/{result.Data.Id}/attributes",
                            new SaveCategoryAttributesRequest
                            {
                                CategoryId = result.Data.Id,
                                Attributes = attrs
                            }, ct);
                        if (!attributesResult.Success)
                        {
                            var error = attributesResult.FirstError ?? "Category was created, but attributes could not be saved.";
                            ModelState.AddModelError(string.Empty, error);
                            TempData["ErrorMessage"] = error;
                            AllCategories = await LoadCategoriesAsync(ct);
                            return Page();
                        }
                    }
                }

                TempData["SuccessMessage"] = "Category created successfully.";
                return RedirectToPage("/Admin/Categories/Index");
            }

            var createError = result.FirstError ?? "Failed to create category. Please try again.";
            TempData["ErrorMessage"] = createError;
            ModelState.AddModelError(string.Empty, createError);
            AllCategories = await LoadCategoriesAsync(ct);
            return Page();
        }

        private async Task<List<CategoryDto>> LoadCategoriesAsync(CancellationToken ct) =>
            await _api.GetAsync<List<CategoryDto>>("/api/admin/categories/all", ct) ?? new();
    }
}
