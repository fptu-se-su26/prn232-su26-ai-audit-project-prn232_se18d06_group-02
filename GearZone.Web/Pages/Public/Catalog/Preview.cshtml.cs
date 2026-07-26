using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Public.Catalog
{
    // Previews a not-yet-public product, so it must not be open to anonymous visitors.
    // The API scopes it further: a Store Owner only sees their own store's products.
    [Authorize(Roles = "Super Admin,Store Owner")]
    public class PreviewModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the admin product service in-process.
        private readonly IApiClient _api;

        public PreviewModel(IApiClient api)
        {
            _api = api;
        }

        public ProductDetailDto ProductData { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty) return NotFound();

            var adminProduct = await _api.GetAsync<AdminProductDetailDto>($"/api/products/{id}/preview", ct);
            if (adminProduct == null) return NotFound();

            if (adminProduct.Status == "Active" && !string.IsNullOrEmpty(adminProduct.Slug))
            {
                return RedirectToPage("/Public/Catalog/ProductDetail", new { slug = adminProduct.Slug });
            }

            ProductData = MapToPublicDetail(adminProduct);

            ViewData["IsPreview"] = true;
            ViewData["ProductReviews"] = null;

            return Page();
        }

        private ProductDetailDto MapToPublicDetail(AdminProductDetailDto src)
        {
            return new ProductDetailDto
            {
                Id = src.Id,
                Name = src.Name,
                Slug = src.Slug,
                Description = src.Description,
                BasePrice = src.BasePrice,
                SoldCount = src.SoldCount,
                Rating = 0,
                ReviewCount = 0,
                BrandName = src.Brand?.Name ?? "N/A",
                BrandSlug = src.Brand?.Slug ?? "",
                CategoryName = src.Category?.Name ?? "N/A",
                CategorySlug = src.Category?.Slug ?? "",
                StoreId = src.Store?.Id ?? Guid.Empty,
                StoreName = src.Store?.Name ?? "N/A",
                StoreSlug = src.Store?.Slug ?? "",
                ImageUrls = src.Images ?? new List<string>(),
                ReviewSummary = new ProductReviewSummaryDto(),
                Specifications = src.Specs?.Select(s => new SpecificationDto
                {
                    Name = s.AttributeName,
                    Value = string.Join(", ", s.Values)
                }).ToList() ?? new List<SpecificationDto>(),
                Variants = src.Variants?.Select(v => new VariantDetailDto
                {
                    Id = Guid.Empty,
                    Sku = v.Sku,
                    VariantName = v.Name,
                    Price = v.Price,
                    StockQuantity = v.Stock,
                    SelectedOptionIds = new List<int>()
                }).ToList() ?? new List<VariantDetailDto>()
            };
        }
    }
}
