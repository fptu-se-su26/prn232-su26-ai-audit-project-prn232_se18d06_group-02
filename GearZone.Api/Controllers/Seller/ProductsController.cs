using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
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
    private readonly IProductImportService _importService;

    public ProductsController(
        ISellerProductService productService,
        ISellerStoreService storeService,
        IProductImportService importService)
    {
        _productService = productService;
        _storeService = storeService;
        _importService = importService;
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

        var stats = new SellerProductStatsDto
        {
            TotalProducts = all.Count,
            ActiveProducts = all.Count(p => p.Status == "Active"),
            OutofStockProducts = all.Count(p => p.TotalStock == 0),
            DraftProducts = all.Count(p => p.Status == "Draft"),
            PendingProducts = all.Count(p => p.Status == "Pending")
        };

        var query = all.AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Diacritics-insensitive so "chuot" matches "chuột".
            var t = NormalizeForSearch(searchTerm);
            query = query.Where(p =>
                NormalizeForSearch(p.Name).Contains(t, StringComparison.Ordinal) ||
                NormalizeForSearch(p.CategoryName).Contains(t, StringComparison.Ordinal) ||
                NormalizeForSearch(p.BrandName).Contains(t, StringComparison.Ordinal));
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

        return OkResponse(new SellerProductListResponseDto
        {
            Stats = stats,
            TotalCount = totalCount,
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    // GET /api/seller/products/{id}/details — richer payload for the details screen.
    [HttpGet("{id:guid}/details")]
    public async Task<IActionResult> Details(Guid id)
    {
        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);

        var product = await _productService.GetProductByIdAsync(id, store.Id);
        if (product == null) return FailResponse("Product not found.", 404);

        return OkResponse(product);
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

        return OkResponse(new SellerProductMetadataDto
        {
            Categories = categories
                .Select(c => new SellerCategoryOptionDto { Id = c.Id, Name = c.Name, ParentId = c.ParentId })
                .ToList(),
            Brands = brands
                .Select(b => new SellerBrandOptionDto { Id = b.Id, Name = b.Name })
                .ToList()
        });
    }

    // Strips Vietnamese diacritics + lowercases so search is accent-insensitive.
    private static string NormalizeForSearch(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var normalized = input.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(c));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
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

    // ---------------------------------------------------------- Bulk import (Excel)

    // GET /api/seller/products/import/template  -> .xlsx template
    [HttpGet("import/template")]
    public async Task<IActionResult> ImportTemplate(CancellationToken ct)
    {
        var bytes = await _importService.GenerateTemplateAsync(ct);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "product-import-template.xlsx");
    }

    // POST /api/seller/products/import/preview  (multipart/form-data: file) -> validated preview
    [HttpPost("import/preview")]
    public async Task<IActionResult> ImportPreview(IFormFile file, CancellationToken ct)
    {
        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);
        if (file == null || file.Length == 0) return FailResponse("Please choose a file to import.");

        try
        {
            var bytes = await ReadAllBytesAsync(file, ct);
            var preview = await _importService.PreviewAsync(bytes, store.Id, ct);
            return OkResponse(preview);
        }
        catch (Exception ex)
        {
            return FailResponse("Could not read the file — make sure it is a valid .xlsx. " + ex.Message);
        }
    }

    // POST /api/seller/products/import  (multipart/form-data: file) -> creates the valid products
    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);
        if (file == null || file.Length == 0) return FailResponse("Please choose a file to import.");

        try
        {
            var bytes = await ReadAllBytesAsync(file, ct);
            var result = await _importService.ImportAsync(bytes, store.Id, CurrentUserId!, ct);
            return OkResponse(result);
        }
        catch (Exception ex)
        {
            return FailResponse("Import failed: " + ex.Message);
        }
    }

    private static async Task<byte[]> ReadAllBytesAsync(IFormFile file, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        return ms.ToArray();
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
