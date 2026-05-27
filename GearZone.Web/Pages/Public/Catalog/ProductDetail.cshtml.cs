using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Web.Pages.Public.Catalog.Models;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GearZone.Web.Pages.Public.Catalog
{
    public class ProductDetailModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly IProductReviewService _productReviewService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductDetailModel(
            ICatalogService catalogService,
            IProductReviewService productReviewService,
            UserManager<ApplicationUser> userManager)
        {
            _catalogService = catalogService;
            _productReviewService = productReviewService;
            _userManager = userManager;
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

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToPage("/Index");
            }

            var product = await LoadPageDataAsync(slug, includeRelatedProducts: true);
            if (product == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetReviewPanelAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return BadRequest();
            }

            var product = await LoadPageDataAsync(slug, includeRelatedProducts: false);
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

        private async Task<ProductDetailDto?> LoadPageDataAsync(string slug, bool includeRelatedProducts)
        {
            var product = await _catalogService.GetProductDetailBySlugAsync(slug);
            if (product == null)
            {
                return null;
            }

            Product = product;
            var currentUserId = _userManager.GetUserId(User);
            if (!string.IsNullOrWhiteSpace(currentUserId))
            {
                Product.EligibleReview = await _productReviewService.GetEligibleReviewForProductAsync(currentUserId, Product.Id);
            }

            ProductReviews = await _productReviewService.GetProductReviewsAsync(Product.Id, new ProductReviewQueryDto
            {
                Rating = ReviewRating,
                WithCommentOnly = WithCommentOnly,
                PageNumber = ReviewPage,
                PageSize = 5
            });

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
                RelatedProducts = await _catalogService.GetRelatedProductsAsync(Product.CategoryId, Product.Id, 4);
            }

            return Product;
        }
    }
}
