using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GearZone.Api.Auditing;
using GearZone.Application.Features.Admin;
using GearZone.Domain.Enums;

namespace GearZone.Api.Controllers.Admin;

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
    public async Task<IActionResult> List([FromQuery] CategoryQueryDto query)
    {
        var categories = await _categoryService.GetHierarchicalCategoriesAsync(query);
        return OkResponse(categories);
    }

    // GET /api/admin/categories/all
    [HttpGet("all")]
    public async Task<IActionResult> All()
        => OkResponse(await _categoryService.GetAllCategoriesListAsync());

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
    [AdminAuditAction(AdminAuditActions.CategoryCreated, AdminAuditModules.Categories, AdminAuditRiskLevel.Medium, EntityType = "Category")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var entity = new Category
        {
            Name = dto.Name,
            Slug = dto.Slug,
            ParentId = dto.ParentId,
            IsActive = dto.IsActive
        };

        var success = await _categoryService.CreateCategoryAsync(entity);
        if (!success) return FailResponse("Failed to create category.");
        return OkResponse(new CategoryCreatedDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
            ParentId = entity.ParentId,
            IsActive = entity.IsActive,
            IsDeleted = entity.IsDeleted
        }, "Category created.");
    }

    // PUT /api/admin/categories/{id}
    [HttpPut("{id:int}")]
    [AdminAuditAction(AdminAuditActions.CategoryUpdated, AdminAuditModules.Categories, AdminAuditRiskLevel.Medium, EntityType = "Category")]
    public async Task<IActionResult> Update(int id, [FromBody] EditCategoryDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();
        dto.Id = id;
        var ok = await _categoryService.UpdateCategoryAsync(dto);
        if (!ok) return FailResponse("Failed to update category.");
        return OkResponse("Category updated.");
    }

    // PUT /api/admin/categories/{id}/attributes
    [HttpPut("{id:int}/attributes")]
    [AdminAuditAction(AdminAuditActions.CategoryAttributesUpdated, AdminAuditModules.Categories, AdminAuditRiskLevel.Medium, EntityType = "Category")]
    public async Task<IActionResult> SaveAttributes(int id, [FromBody] SaveCategoryAttributesRequest request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();
        request.CategoryId = id;
        var ok = await _categoryService.SaveCategoryAttributesAsync(request);
        return ok ? OkResponse("Category attributes saved.") : FailResponse("Failed to save category attributes.");
    }

    // DELETE /api/admin/categories/{id}
    [HttpDelete("{id:int}")]
    [AdminAuditAction(AdminAuditActions.CategoryDeleted, AdminAuditModules.Categories, AdminAuditRiskLevel.Medium, EntityType = "Category")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _categoryService.SoftDeleteCategoryAsync(id);
        return ok ? OkResponse("Category deleted.") : FailResponse("Failed to delete category.");
    }
}
