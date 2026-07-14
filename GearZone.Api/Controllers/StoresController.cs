using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GearZone.Api.Controllers;

[AllowAnonymous]
public class StoresController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public StoresController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    // GET /api/stores/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> Profile(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return FailResponse("Store slug is required.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var store = await _catalogService.GetStoreProfileAsync(slug, userId);
        if (store == null) return FailResponse("Store not found.", 404);

        return OkResponse(store);
    }

    // GET /api/stores/{slug}/products?categorySlug=&sort=&minPrice=&maxPrice=&page=
    [HttpGet("{slug}/products")]
    public async Task<IActionResult> Products(string slug, [FromQuery] ProductFilterDto filter)
    {
        if (string.IsNullOrWhiteSpace(slug)) return FailResponse("Store slug is required.");

        var store = await _catalogService.GetStoreProfileAsync(slug);
        if (store == null) return FailResponse("Store not found.", 404);

        filter.StoreId = store.Id;
        if (filter.PageNumber < 1) filter.PageNumber = 1;
        if (filter.PageSize <= 0) filter.PageSize = 20;

        var products = await _catalogService.GetProductsAsync(filter);
        return OkResponse(products);
    }

    // POST /api/stores/{slug}/follow
    [Authorize]
    [HttpPost("{slug}/follow")]
    public async Task<IActionResult> Follow(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return FailResponse("Store slug is required.");

        var store = await _catalogService.GetStoreProfileAsync(slug);
        if (store == null) return FailResponse("Store not found.", 404);

        var isFollowing = await _catalogService.ToggleFollowAsync(CurrentUserId!, store.Id);
        var followerCount = await _catalogService.GetFollowerCountAsync(store.Id);

        return OkResponse(new { isFollowing, followerCount });
    }
}
