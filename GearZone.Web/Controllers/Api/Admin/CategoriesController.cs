using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/categories")]
[ApiController]
public class CategoriesController : BaseApiController
{
    private readonly IAdminCategoryService _categoryService;

    public CategoriesController(IAdminCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET /api/admin/categories
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var categories = await _categoryService.GetAllCategoriesListAsync();
        return OkResponse(categories);
    }

    // GET /api/admin/categories/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return FailResponse("Category not found.", 404);
        return OkResponse(category);
    }

    // GET /api/admin/categories/{id}/attributes
    [HttpGet("{id:int}/attributes")]
    public async Task<IActionResult> Attributes(int id)
    {
        var attrs = await _categoryService.GetAttributesByCategoryIdAsync(id);
        return OkResponse(attrs);
    }

    // POST /api/admin/categories
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Category entity)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();
        var success = await _categoryService.CreateCategoryAsync(entity);
        if (!success) return FailResponse("Failed to create category.");
        return OkResponse("Category created.");
    }

    // PUT /api/admin/categories/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EditCategoryDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();
        dto.Id = id;
        await _categoryService.UpdateCategoryAsync(dto);
        return OkResponse("Category updated.");
    }

    // DELETE /api/admin/categories/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _categoryService.SoftDeleteCategoryAsync(id);
        return ok ? OkResponse("Category deleted.") : FailResponse("Failed to delete category.");
    }
}
