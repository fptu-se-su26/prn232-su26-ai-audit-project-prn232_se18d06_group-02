using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Web.Pages.Public.Catalog.Models;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GearZone.Web.Pages.Public.Catalog
{
    public class ProductDetailModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the catalog/review services in-process.
        // The forwarded auth cookie lets the API resolve EligibleReview for the signed-in user.
        private readonly IApiClient _api;

        public ProductDetailModel(IApiClient api)
        {
            _api = api;
        }

        public ProductDetailDto Product { get; set; } = default!;
        public List<CatalogProductDto> RelatedProducts { get; set; } = new();
        public PagedResult<ProductReviewListItemDto> ProductReviews { get; set; } = new();
        public ProductReviewPanelViewModel ReviewPanel { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? ReviewRating { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool WithCommentOnly { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ReviewPage { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToPage("/Index");
            }

            var product = await LoadPageDataAsync(slug, includeRelatedProducts: true, ct);
            if (product == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetReviewPanelAsync(string slug, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return BadRequest();
            }

            var product = await LoadPageDataAsync(slug, includeRelatedProducts: false, ct);
            if (product == null)
            {
                return NotFound();
            }

            return new PartialViewResult
            {
                ViewName = "/Pages/Public/Catalog/Partials/_ProductReviewPanel.cshtml",
                ViewData = new ViewDataDictionary<ProductReviewPanelViewModel>(ViewData, ReviewPanel)
            };
        }

        private async Task<ProductDetailDto?> LoadPageDataAsync(string slug, bool includeRelatedProducts, CancellationToken ct)
        {
            var parts = new List<string> { $"reviewPage={(ReviewPage < 1 ? 1 : ReviewPage)}" };
            if (ReviewRating.HasValue) parts.Add($"reviewRating={ReviewRating.Value}");
            if (WithCommentOnly) parts.Add("withCommentOnly=true");

            var data = await _api.GetAsync<ProductDetailPageDto>(
                $"/api/products/{Uri.EscapeDataString(slug)}?{string.Join("&", parts)}", ct);

            if (data?.Product == null)
            {
                return null;
            }

            Product = data.Product;
            ProductReviews = data.Reviews;

            ReviewPanel = new ProductReviewPanelViewModel
            {
                ProductSlug = Product.Slug,
                ReviewSummary = Product.ReviewSummary,
                EligibleReview = Product.EligibleReview,
                ProductReviews = ProductReviews,
                ReviewRating = ReviewRating,
                WithCommentOnly = WithCommentOnly
            };

            ViewData["ProductReviews"] = ProductReviews;
            ViewData["ReviewRating"] = ReviewRating;
            ViewData["WithCommentOnly"] = WithCommentOnly;

            if (includeRelatedProducts)
            {
                RelatedProducts = data.RelatedProducts;
            }

            return Product;
        }
    }
}
