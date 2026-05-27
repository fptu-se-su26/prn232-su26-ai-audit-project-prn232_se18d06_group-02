using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Application.Features.Reviews.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GearZone.Web.Controllers.Api;

[AllowAnonymous]
public class ProductsController : BaseApiController
{
    private readonly ICatalogService _catalogService;
    private readonly IProductReviewService _reviewService;
    private readonly IAdminProductService _adminProductService;

    public ProductsController(
        ICatalogService catalogService,
        IProductReviewService reviewService,
        IAdminProductService adminProductService)
    {
        _catalogService = catalogService;
        _reviewService = reviewService;
        _adminProductService = adminProductService;
    }

    // GET /api/products?search=&categorySlug=&brand=&minPrice=&maxPrice=&sort=&page=&inStock=
    [HttpGet]
    public async Task<IActionResult> Browse([FromQuery] ProductFilterDto filter)
    {
        // Ensure defaults
        if (filter.PageNumber < 1) filter.PageNumber = 1;
        if (filter.PageSize <= 0) filter.PageSize = 12;

        var result = await _catalogService.GetProductsAsync(filter);
        return OkResponse(result);
    }

    // GET /api/products/suggestions?query=
    [HttpGet("suggestions")]
    public async Task<IActionResult> Suggestions([FromQuery] string query)
    {
        var suggestions = await _catalogService.GetProductSuggestionsAsync(query);
        return OkResponse(suggestions);
    }

    // GET /api/products/compare?categoryId=&ids=guid1,guid2
    [HttpGet("compare")]
    public async Task<IActionResult> Compare([FromQuery] int categoryId, [FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids))
            return FailResponse("Product IDs are required.");

        var productIds = ids.Split(',')
            .Select(id => Guid.TryParse(id.Trim(), out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();

        if (!productIds.Any())
            return FailResponse("No valid product IDs provided.");

        var data = await _catalogService.GetProductComparisonAsync(categoryId, productIds);
        if (data == null) return FailResponse("Comparison data not found.", 404);

        return OkResponse(data);
    }

    // GET /api/products/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> Detail(string slug, [FromQuery] int? reviewRating, [FromQuery] bool withCommentOnly, [FromQuery] int reviewPage = 1)
    {
        var product = await _catalogService.GetProductDetailBySlugAsync(slug);
        if (product == null) return FailResponse("Product not found.", 404);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
            product.EligibleReview = await _reviewService.GetEligibleReviewForProductAsync(userId, product.Id);

        var reviews = await _reviewService.GetProductReviewsAsync(product.Id, new ProductReviewQueryDto
        {
            Rating = reviewRating,
            WithCommentOnly = withCommentOnly,
            PageNumber = reviewPage < 1 ? 1 : reviewPage,
            PageSize = 5
        });

        var relatedProducts = await _catalogService.GetRelatedProductsAsync(product.CategoryId, product.Id, 4);

        return OkResponse(new { product, reviews, relatedProducts });
    }

    // GET /api/products/{slug}/reviews?rating=&withCommentOnly=&page=
    [HttpGet("{slug}/reviews")]
    public async Task<IActionResult> Reviews(string slug, [FromQuery] ProductReviewQueryDto query)
    {
        var product = await _catalogService.GetProductDetailBySlugAsync(slug);
        if (product == null) return FailResponse("Product not found.", 404);

        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize <= 0) query.PageSize = 5;

        var reviews = await _reviewService.GetProductReviewsAsync(product.Id, query);
        return OkResponse(reviews);
    }

    // GET /api/products/{id}/preview  (Admin only - pending products)
    [Authorize(Roles = "Super Admin")]
    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> Preview(Guid id)
    {
        var product = await _adminProductService.GetProductDetailAsync(id);
        if (product == null) return FailResponse("Product not found.", 404);

        return OkResponse(product);
    }
}
