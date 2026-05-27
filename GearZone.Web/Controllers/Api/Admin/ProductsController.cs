using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/products")]
[ApiController]
public class ProductsController : BaseApiController
{
    private readonly IAdminProductService _productService;
    private readonly IAdminCategoryService _categoryService;
    private readonly IAdminStoreService _storeService;
    private readonly IAdminBrandService _brandService;

    public ProductsController(
        IAdminProductService productService,
        IAdminCategoryService categoryService,
        IAdminStoreService storeService,
        IAdminBrandService brandService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _storeService = storeService;
        _brandService = brandService;
    }

    // GET /api/admin/products?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AdminProductQueryDto query)
    {
        var products = await _productService.GetProductsAsync(query);
        var stats = await _productService.GetProductStatsAsync();
        return OkResponse(new { products, stats });
    }

    // GET /api/admin/products/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var product = await _productService.GetProductDetailAsync(id);
        if (product == null) return FailResponse("Product not found.", 404);
        return OkResponse(product);
    }

    // GET /api/admin/products/metadata  (categories, stores, brands for filter dropdowns)
    [HttpGet("metadata")]
    public async Task<IActionResult> Metadata()
    {
        var categories = await _categoryService.GetAllCategoriesListAsync();
        var stores = await _storeService.GetAllStoresAsync();
        var brands = await _brandService.GetAllBrandsListAsync();
        return OkResponse(new { categories, stores, brands });
    }

    // POST /api/admin/products/{id}/approve
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _productService.BulkUpdateStatusAsync(new List<Guid> { id }, ProductStatus.Active);
        return ok ? OkResponse("Product approved.") : FailResponse("Failed to approve product.");
    }

    // POST /api/admin/products/{id}/reject
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest? request)
    {
        var ok = await _productService.BulkUpdateStatusAsync(new List<Guid> { id }, ProductStatus.Rejected, request?.Reason);
        return ok ? OkResponse("Product rejected.") : FailResponse("Failed to reject product.");
    }

    // POST /api/admin/products/{id}/suspend
    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] RejectRequest? request)
    {
        var ok = await _productService.BulkUpdateStatusAsync(new List<Guid> { id }, ProductStatus.Inactive, request?.Reason);
        return ok ? OkResponse("Product suspended.") : FailResponse("Failed to suspend product.");
    }

    // DELETE /api/admin/products/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromBody] RejectRequest? request)
    {
        var ok = await _productService.DeleteProductAsync(id, request?.Reason ?? "Deleted by admin.");
        return ok ? OkResponse("Product deleted.") : FailResponse("Failed to delete product.");
    }

    // POST /api/admin/products/bulk-update-status
    [HttpPost("bulk-update-status")]
    public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkStatusRequest request)
    {
        if (request.ProductIds == null || !request.ProductIds.Any())
            return FailResponse("No products selected.");

        if (request.Action.Equals("Delete", StringComparison.OrdinalIgnoreCase))
        {
            var deleted = 0;
            foreach (var id in request.ProductIds)
                if (await _productService.DeleteProductAsync(id, request.Reason ?? "No reason provided"))
                    deleted++;
            return OkResponse($"Deleted {deleted} product(s).");
        }

        var status = request.Action.ToLower() switch
        {
            "approve" => ProductStatus.Active,
            "reject" => ProductStatus.Rejected,
            "inactive" => ProductStatus.Inactive,
            _ => (ProductStatus?)null
        };

        if (status == null) return FailResponse("Invalid action.");

        var ok = await _productService.BulkUpdateStatusAsync(request.ProductIds, status.Value, request.Reason);
        return ok
            ? OkResponse($"Updated {request.ProductIds.Count} product(s) to {status}.")
            : FailResponse("Failed to update product statuses.");
    }
}

public class RejectRequest
{
    public string? Reason { get; set; }
}

public class BulkStatusRequest
{
    public List<Guid> ProductIds { get; set; } = new();
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
