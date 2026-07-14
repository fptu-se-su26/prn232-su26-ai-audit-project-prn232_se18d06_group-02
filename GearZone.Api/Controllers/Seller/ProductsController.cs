using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers.Seller;

[Authorize(Roles = "Store Owner")]
[Route("api/seller/products")]
[ApiController]
public class ProductsController : BaseApiController
{
    private readonly ISellerProductService _productService;
    private readonly ISellerStoreService _storeService;

    public ProductsController(ISellerProductService productService, ISellerStoreService storeService)
    {
        _productService = productService;
        _storeService = storeService;
    }

    // GET /api/seller/products?search=&status=&categoryId=&brandId=&sort=&dir=&page=
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? searchTerm, [FromQuery] string? status,
        [FromQuery] int? categoryId, [FromQuery] int? brandId,
        [FromQuery] string sortBy = "createdAt", [FromQuery] string sortDir = "desc",
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);

        var all = await _productService.GetProductsByStoreAsync(store.Id);

        var stats = new
        {
            total = all.Count,
            active = all.Count(p => p.Status == "Active"),
            outOfStock = all.Count(p => p.TotalStock == 0),
            draft = all.Count(p => p.Status == "Draft"),
            pending = all.Count(p => p.Status == "Pending")
        };

        var query = all.AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var t = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLowerInvariant().Contains(t) ||
                p.CategoryName.ToLowerInvariant().Contains(t) ||
                p.BrandName.ToLowerInvariant().Contains(t));
        }
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (brandId.HasValue) query = query.Where(p => p.BrandId == brandId.Value);

        query = sortBy switch
        {
            "name" => sortDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "price" => sortDir == "asc" ? query.OrderBy(p => p.BasePrice) : query.OrderByDescending(p => p.BasePrice),
            "stock" => sortDir == "asc" ? query.OrderBy(p => p.TotalStock) : query.OrderByDescending(p => p.TotalStock),
            _ => sortDir == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return OkResponse(new { stats, totalCount, items, page, pageSize, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) });
    }

    // GET /api/seller/products/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);

        var product = await _productService.GetProductForEditAsync(id, store.Id);
        if (product == null) return FailResponse("Product not found.", 404);

        return OkResponse(product);
    }

    // POST /api/seller/products
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);

        var id = await _productService.CreateProductAsync(dto, store.Id, CurrentUserId!);
        return CreatedResponse(new { id }, $"/api/seller/products/{id}", "Product created.");
    }

    // PUT /api/seller/products/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);

        await _productService.UpdateProductAsync(id, dto, store.Id, CurrentUserId!);
        return OkResponse("Product updated.");
    }

    // PATCH /api/seller/products/{id}/toggle-status
    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);

        await _productService.ToggleProductStatusAsync(id, store.Id);
        return OkResponse("Product status updated.");
    }

    // GET /api/seller/products/metadata  (categories + brands)
    [HttpGet("metadata")]
    public async Task<IActionResult> Metadata()
    {
        var categories = await _productService.GetCategoriesAsync();
        var brands = await _productService.GetBrandsAsync();
        return OkResponse(new { categories, brands });
    }

    // GET /api/seller/products/attributes?categoryId=
    [HttpGet("attributes")]
    public async Task<IActionResult> Attributes([FromQuery] int categoryId)
    {
        var attrs = await _productService.GetCategoryAttributesAsync(categoryId);
        return OkResponse(attrs);
    }

    // GET /api/seller/products/specifications?categoryId=
    [HttpGet("specifications")]
    public async Task<IActionResult> Specifications([FromQuery] int categoryId)
    {
        var specs = await _productService.GetCategoryProductSpecsAsync(categoryId);
        return OkResponse(specs);
    }

    // POST /api/seller/products/specifications
    [HttpPost("specifications")]
    public async Task<IActionResult> CreateSpecification([FromBody] CreateSpecRequest request)
    {
        if (request.CategoryId <= 0) return FailResponse("Category is required.");
        if (string.IsNullOrWhiteSpace(request.Name)) return FailResponse("Name is required.");

        var id = await _productService.CreateCategoryProductSpecificationAsync(
            request.CategoryId, request.Name, request.Unit, request.ValueType);
        return CreatedResponse(new { id, name = request.Name.Trim() }, $"/api/seller/products/specifications");
    }

    // POST /api/seller/products/brands
    [HttpPost("brands")]
    public async Task<IActionResult> CreateBrand([FromBody] CreateNameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return FailResponse("Name is required.");
        var id = await _productService.CreateBrandByNameAsync(request.Name);
        return CreatedResponse(new { id, name = request.Name }, $"/api/seller/products/brands");
    }

    // POST /api/seller/products/categories
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateNameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return FailResponse("Name is required.");
        var id = await _productService.CreateCategoryByNameAsync(request.Name);
        return CreatedResponse(new { id, name = request.Name }, $"/api/seller/products/categories");
    }
}

public class CreateSpecRequest
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? ValueType { get; set; }
}

public class CreateNameRequest
{
    public string Name { get; set; } = string.Empty;
}
