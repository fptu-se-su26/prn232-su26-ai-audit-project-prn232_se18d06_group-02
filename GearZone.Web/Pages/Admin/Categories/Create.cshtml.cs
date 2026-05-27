using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Web.Pages.Admin.Categories.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Categories
{
    [Authorize(Roles = "Super Admin")]
    public class CreateModel : PageModel
    {
        private readonly IAdminCategoryService _adminCategoryService;
        private readonly IMapper _mapper;

        public CreateModel(IAdminCategoryService adminCategoryService, IMapper mapper)
        {
            _adminCategoryService = adminCategoryService;
            _mapper = mapper;
        }

        [BindProperty]
        public CreateCategoryViewModel Input { get; set; } = new CreateCategoryViewModel();

        public List<CategoryDto> AllCategories { get; set; } = new();

        public async Task OnGetAsync()
        {
            AllCategories = await _adminCategoryService.GetAllCategoriesListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                AllCategories = await _adminCategoryService.GetAllCategoriesListAsync();
                return Page();
            }

            var category = _mapper.Map<Category>(Input);
            var result = await _adminCategoryService.CreateCategoryAsync(category);

            if (result)
            {
                // After category created, save attributes JSON if provided
                var attrsJson = Request.Form["AttributesJson"].ToString();
                if (!string.IsNullOrWhiteSpace(attrsJson))
                {
                    // We need the new category id — re-fetch by slug
                    var allCats = await _adminCategoryService.GetAllCategoriesListAsync();
                    var newCat = allCats.FirstOrDefault(c => c.Slug == Input.Slug);
                    if (newCat != null)
                    {
                        var attrs = System.Text.Json.JsonSerializer.Deserialize<List<CategoryAttributeDto>>(attrsJson);
                        if (attrs != null && attrs.Count > 0)
                        {
                            await _adminCategoryService.SaveCategoryAttributesAsync(new SaveCategoryAttributesRequest
                            {
                                CategoryId = newCat.Id,
                                Attributes = attrs
                            });
                        }
                    }
                }

                TempData["SuccessMessage"] = "Category created successfully.";
                return RedirectToPage("/Admin/Categories/Index");
            }

            TempData["ErrorMessage"] = "Failed to create category. Please try again.";
            AllCategories = await _adminCategoryService.GetAllCategoriesListAsync();
            return Page();
        }
    }
}
