using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GearZone.Api.Controllers;

[AllowAnonymous]
public class CatalogController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    // GET /api/catalog/home
    [HttpGet("home")]
    public async Task<IActionResult> Home()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var data = await _catalogService.GetHomePageAsync(userId);
        return OkResponse(data);
    }

    // GET /api/catalog/categories
    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var categories = await _catalogService.GetCategoriesAsync();
        return OkResponse(categories);
    }

    // GET /api/catalog/filters?slug=
    [HttpGet("filters")]
    public async Task<IActionResult> Filters([FromQuery] string? slug)
    {
        var sidebar = await _catalogService.GetFiltersForCategoryAsync(slug);
        return OkResponse(sidebar);
    }
}
