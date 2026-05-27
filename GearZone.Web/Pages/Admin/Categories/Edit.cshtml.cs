using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Pages.Admin.Categories.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Categories
{
    [Authorize(Roles = "Super Admin")]
    public class EditModel : PageModel
    {
        private readonly IAdminCategoryService _adminCategoryService;
        private readonly IMapper _mapper;

        public EditModel(IAdminCategoryService adminCategoryService, IMapper mapper)
        {
            _adminCategoryService = adminCategoryService;
            _mapper = mapper;
        }

        [BindProperty]
        public EditCategoryViewModel Input { get; set; } = new EditCategoryViewModel();

        public List<CategoryDto> AllCategories { get; set; } = new();
        public List<CategoryAttributeDto> Attributes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _adminCategoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToPage("/Admin/Categories/Index");
            }

            Input = _mapper.Map<EditCategoryViewModel>(category);
            AllCategories = await _adminCategoryService.GetAllCategoriesListAsync();
            Attributes = await _adminCategoryService.GetAttributesByCategoryIdAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                AllCategories = await _adminCategoryService.GetAllCategoriesListAsync();
                Attributes = await _adminCategoryService.GetAttributesByCategoryIdAsync(Input.Id);
                return Page();
            }

            var dto = _mapper.Map<EditCategoryDto>(Input);
            var result = await _adminCategoryService.UpdateCategoryAsync(dto);

            if (result)
            {
                // Save attributes
                var attrsJson = Request.Form["AttributesJson"].ToString();
                if (!string.IsNullOrWhiteSpace(attrsJson))
                {
                    var attrs = System.Text.Json.JsonSerializer.Deserialize<List<CategoryAttributeDto>>(attrsJson);
                    await _adminCategoryService.SaveCategoryAttributesAsync(new SaveCategoryAttributesRequest
                    {
                        CategoryId = Input.Id,
                        Attributes = attrs ?? new()
                    });
                }
                else
                {
                    // Save empty = clear all attributes
                    await _adminCategoryService.SaveCategoryAttributesAsync(new SaveCategoryAttributesRequest
                    {
                        CategoryId = Input.Id,
                        Attributes = new()
                    });
                }

                TempData["SuccessMessage"] = "Category updated successfully.";
                return RedirectToPage("/Admin/Categories/Index");
            }

            TempData["ErrorMessage"] = "Failed to update category. Please try again.";
            AllCategories = await _adminCategoryService.GetAllCategoriesListAsync();
            Attributes = await _adminCategoryService.GetAttributesByCategoryIdAsync(Input.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _adminCategoryService.SoftDeleteCategoryAsync(id);
            if (result)
                TempData["SuccessMessage"] = "Category deleted successfully.";
            else
                TempData["ErrorMessage"] = "Failed to delete category.";

            return RedirectToPage("/Admin/Categories/Index");
        }
    }
}
