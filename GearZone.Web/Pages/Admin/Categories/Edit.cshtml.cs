using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Categories
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
        public EditCategoryDto Input { get; set; } = new();

        public List<CategoryDto> AllCategories { get; set; } = new();
        public List<CategoryAttributeDto> Attributes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
        {
            var categoryTask = _api.GetAsync<CategoryDto>($"/api/admin/categories/{id}", ct);
            var categoriesTask = LoadCategoriesAsync(ct);
            var attributesTask = LoadAttributesAsync(id, ct);
            await Task.WhenAll(categoryTask, categoriesTask, attributesTask);

            var category = await categoryTask;
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToPage("/Admin/Categories/Index");
            }

            Input = new EditCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                ParentId = category.ParentId,
                IsActive = category.IsActive
            };
            AllCategories = await categoriesTask;
            Attributes = await attributesTask;
            return Page();
        }

        public async Task<IActionResult> OnPostEditAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await ReloadLookupsAsync(ct);
                return Page();
            }

            var result = await _api.PutAsync($"/api/admin/categories/{Input.Id}", Input, ct);

            if (result.Success)
            {
                // Save attributes
                var attrsJson = Request.Form["AttributesJson"].ToString();
                if (!string.IsNullOrWhiteSpace(attrsJson))
                {
                    var attrs = System.Text.Json.JsonSerializer.Deserialize<List<CategoryAttributeDto>>(attrsJson);
                    var attributesResult = await SaveAttributesAsync(attrs ?? new(), ct);
                    if (!attributesResult.Success) return await AttributeFailureAsync(attributesResult, ct);
                }
                else
                {
                    // Save empty = clear all attributes
                    var attributesResult = await SaveAttributesAsync(new(), ct);
                    if (!attributesResult.Success) return await AttributeFailureAsync(attributesResult, ct);
                }

                TempData["SuccessMessage"] = "Category updated successfully.";
                return RedirectToPage("/Admin/Categories/Index");
            }

            var updateError = result.FirstError ?? "Failed to update category. Please try again.";
            TempData["ErrorMessage"] = updateError;
            ModelState.AddModelError(string.Empty, updateError);
            await ReloadLookupsAsync(ct);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
        {
            var result = await _api.DeleteAsync($"/api/admin/categories/{id}", ct);
            if (result.Success)
                TempData["SuccessMessage"] = "Category deleted successfully.";
            else
                TempData["ErrorMessage"] = "Failed to delete category.";

            return RedirectToPage("/Admin/Categories/Index");
        }

        private async Task<List<CategoryDto>> LoadCategoriesAsync(CancellationToken ct) =>
            await _api.GetAsync<List<CategoryDto>>("/api/admin/categories/all", ct) ?? new();

        private async Task<List<CategoryAttributeDto>> LoadAttributesAsync(int id, CancellationToken ct) =>
            await _api.GetAsync<List<CategoryAttributeDto>>($"/api/admin/categories/{id}/attributes", ct) ?? new();

        private async Task ReloadLookupsAsync(CancellationToken ct)
        {
            var categoriesTask = LoadCategoriesAsync(ct);
            var attributesTask = LoadAttributesAsync(Input.Id, ct);
            await Task.WhenAll(categoriesTask, attributesTask);
            AllCategories = await categoriesTask;
            Attributes = await attributesTask;
        }

        private Task<ApiResult> SaveAttributesAsync(List<CategoryAttributeDto> attributes, CancellationToken ct) =>
            _api.PutAsync($"/api/admin/categories/{Input.Id}/attributes", new SaveCategoryAttributesRequest
            {
                CategoryId = Input.Id,
                Attributes = attributes
            }, ct);

        private async Task<IActionResult> AttributeFailureAsync(ApiResult result, CancellationToken ct)
        {
            var error = result.FirstError ?? "Category was updated, but attributes could not be saved.";
            TempData["ErrorMessage"] = error;
            ModelState.AddModelError(string.Empty, error);
            await ReloadLookupsAsync(ct);
            return Page();
        }
    }
}
