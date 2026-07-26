using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Application.Features.Reviews.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GearZone.Api.Controllers;

// NOTE: no controller-level [AllowAnonymous] — it would override the [Authorize]
// on Preview. Public actions opt in individually instead.
public class ProductsController : BaseApiController
{
    private readonly ICatalogService _catalogService;
    private readonly IProductReviewService _reviewService;
    private readonly IAdminProductService _adminProductService;
    private readonly ISellerStoreService _storeService;

    public ProductsController(
        ICatalogService catalogService,
        IProductReviewService reviewService,
        IAdminProductService adminProductService,
        ISellerStoreService storeService)
    {
        _catalogService = catalogService;
        _reviewService = reviewService;
        _adminProductService = adminProductService;
        _storeService = storeService;
    }

    // GET /api/products?search=&categorySlug=&brand=&minPrice=&maxPrice=&sortBy=&pageNumber=&inStockOnly=
    // Any other query key is treated as a dynamic attribute filter, e.g. ?VRAM=12GB&VRAM=16GB
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Browse([FromQuery] ProductFilterDto filter)
    {
        // Ensure defaults
        if (filter.PageNumber < 1) filter.PageNumber = 1;
        if (filter.PageSize <= 0) filter.PageSize = 12;

        filter.Attributes = ParseDynamicAttributes();

        var result = await _catalogService.GetProductsAsync(filter);
        return OkResponse(result);
    }

    // Query keys that map onto ProductFilterDto itself; everything else is an attribute filter.
    private static readonly HashSet<string> FilterQueryKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "storeId", "search", "categorySlug", "categorySlugs", "brand", "brandSlugs", "minPrice", "maxPrice",
        "inStockOnly", "inStock", "sortBy", "sort", "viewMode", "pageNumber", "page", "pageSize",
        "attributes", "handler"
    };

    private Dictionary<string, List<string>> ParseDynamicAttributes()
    {
        var attributes = new Dictionary<string, List<string>>();

        foreach (var key in Request.Query.Keys)
        {
            if (FilterQueryKeys.Contains(key)) continue;

            var values = new List<string>();
            foreach (var value in Request.Query[key])
            {
                if (!string.IsNullOrWhiteSpace(value)) values.AddRange(value.Split(','));
            }

            if (values.Count > 0) attributes[key] = values;
        }

        return attributes;
    }

    // GET /api/products/suggestions?query=
    [AllowAnonymous]
    [HttpGet("suggestions")]
    public async Task<IActionResult> Suggestions([FromQuery] string query)
    {
        var suggestions = await _catalogService.GetProductSuggestionsAsync(query);
        return OkResponse(suggestions);
    }

    // GET /api/products/compare?categoryId=&ids=guid1,guid2
    [AllowAnonymous]
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
    [AllowAnonymous]
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

        return OkResponse(new ProductDetailPageDto
        {
            Product = product,
            Reviews = reviews,
            RelatedProducts = relatedProducts
        });
    }

    // GET /api/products/{slug}/reviews?rating=&withCommentOnly=&page=
    [AllowAnonymous]
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

    // GET /api/products/{id}/preview — preview a not-yet-public (draft/pending) product.
    // Super Admin can preview any product; a Store Owner only their own store's products.
    [Authorize(Roles = "Super Admin,Store Owner")]
    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> Preview(Guid id)
    {
        var product = await _adminProductService.GetProductDetailAsync(id);
        if (product == null) return FailResponse("Product not found.", 404);

        if (!User.IsInRole("Super Admin"))
        {
            var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
            // 404 rather than 403 so we don't leak that the product exists.
            if (store == null || product.Store?.Id != store.Id)
                return FailResponse("Product not found.", 404);
        }

        return OkResponse(product);
    }
}
