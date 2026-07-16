using System.Security.Claims;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GearZone.Web.Pages.StoreOwner.Products
{
    [Authorize(Roles = "Store Owner")]
    public class CreateModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the product service in-process.
        private readonly IApiClient _api;

        public CreateModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public CreateProductDto Input { get; set; } = new();

        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<SelectListItem> BrandOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid? copyFromId, CancellationToken ct)
        {
            await LoadMetadataAsync(ct);

            if (copyFromId.HasValue)
            {
                var editDto = await _api.GetAsync<UpdateProductDto>($"/api/seller/products/{copyFromId.Value}", ct);
                if (editDto != null)
                {
                    Input = new CreateProductDto
                    {
                        Name = editDto.Name + " (Copy)",
                        Description = editDto.Description,
                        CategoryId = editDto.CategoryId,
                        BrandId = editDto.BrandId,
                        BasePrice = editDto.BasePrice,
                        IsDraft = true,
                        Specifications = editDto.Specifications,
                        Variants = editDto.Variants.Select(v => new ProductVariantDto
                        {
                            VariantName = v.VariantName,
                            Sku = "", // Clear SKU for cloning
                            Price = v.Price,
                            StockQuantity = v.StockQuantity,
                            Attributes = v.Attributes
                        }).ToList()
                    };
                    return Page();
                }
            }

            // Initialize with one default variant.
            if (!Input.Variants.Any())
            {
                Input.Variants.Add(new ProductVariantDto { VariantName = string.Empty, StockQuantity = 0 });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await LoadMetadataAsync(ct);
                return Page();
            }

            var result = await _api.PostAsync("/api/seller/products", Input, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToPage("./Index");
            }

            ModelState.AddModelError("", result.FirstError ?? "Failed to create product.");
            await LoadMetadataAsync(ct);
            return Page();
        }

        public async Task<JsonResult> OnGetAttributesAsync(int categoryId, CancellationToken ct)
        {
            var attributes = await _api.GetAsync<List<CategoryAttributeDto>>(
                $"/api/seller/products/attributes?categoryId={categoryId}", ct);
            return new JsonResult(attributes ?? new List<CategoryAttributeDto>());
        }

        public async Task<JsonResult> OnGetSpecificationsAsync(int categoryId, CancellationToken ct)
        {
            var specs = await _api.GetAsync<List<ProductSpecDto>>(
                $"/api/seller/products/specifications?categoryId={categoryId}", ct);
            return new JsonResult(specs ?? new List<ProductSpecDto>());
        }

        public async Task<JsonResult> OnPostCreateSpecificationAttributeAsync(int categoryId, string name, string? unit, string? valueType, CancellationToken ct)
        {
            if (categoryId <= 0) return new JsonResult(new { success = false, message = "Category is required" });
            if (string.IsNullOrWhiteSpace(name)) return new JsonResult(new { success = false, message = "Specification name is required" });

            var created = await _api.PostAsync<object>("/api/seller/products/specifications",
                new { categoryId, name, unit, valueType }, ct);
            if (!created.Success) return new JsonResult(new { success = false, message = created.FirstError ?? "Failed to create specification" });

            var specs = await _api.GetAsync<List<ProductSpecDto>>($"/api/seller/products/specifications?categoryId={categoryId}", ct);
            var match = specs?.FirstOrDefault(s => string.Equals(s.Key, name.Trim(), StringComparison.OrdinalIgnoreCase));
            return new JsonResult(new { success = true, id = match?.AttributeId ?? 0, name = name.Trim() });
        }

        public Task<JsonResult> OnPostCreateBrandAsync(string name, CancellationToken ct) =>
            CreateNamedAsync("/api/seller/products/brands", name, ct);

        public Task<JsonResult> OnPostCreateCategoryAsync(string name, CancellationToken ct) =>
            CreateNamedAsync("/api/seller/products/categories", name, ct);

        private async Task<JsonResult> CreateNamedAsync(string path, string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name)) return new JsonResult(new { success = false, message = "Name is required" });

            var result = await _api.PostAsync(path, new { name }, ct);
            if (!result.Success) return new JsonResult(new { success = false, message = result.FirstError ?? "Failed to create" });

            var id = await ResolveCreatedIdAsync(path, name, ct);
            return new JsonResult(new { success = true, id, name });
        }

        // The create endpoints return the new id inside a Created payload; re-reading the
        // metadata is the simplest way to resolve it with the shared ApiResult contract.
        private async Task<int> ResolveCreatedIdAsync(string path, string name, CancellationToken ct)
        {
            var metadata = await _api.GetAsync<SellerProductMetadataDto>("/api/seller/products/metadata", ct);
            if (metadata == null) return 0;

            return path.EndsWith("brands", StringComparison.OrdinalIgnoreCase)
                ? metadata.Brands.FirstOrDefault(b => string.Equals(b.Name, name.Trim(), StringComparison.OrdinalIgnoreCase))?.Id ?? 0
                : metadata.Categories.FirstOrDefault(c => string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
        }

        private async Task LoadMetadataAsync(CancellationToken ct)
        {
            var metadata = await _api.GetAsync<SellerProductMetadataDto>("/api/seller/products/metadata", ct)
                ?? new SellerProductMetadataDto();

            var allCategories = metadata.Categories;
            CategoryOptions = allCategories
                .Where(c => c.ParentId != null || !allCategories.Any(child => child.ParentId == c.Id))
                .Select(c =>
                {
                    var parent = c.ParentId.HasValue ? allCategories.FirstOrDefault(pc => pc.Id == c.ParentId.Value) : null;
                    var text = parent != null ? $"{parent.Name} > {c.Name}" : c.Name;
                    return new SelectListItem { Value = c.Id.ToString(), Text = text };
                })
                .OrderBy(s => s.Text)
                .ToList();

            BrandOptions = metadata.Brands
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name })
                .ToList();
        }
    }
}
