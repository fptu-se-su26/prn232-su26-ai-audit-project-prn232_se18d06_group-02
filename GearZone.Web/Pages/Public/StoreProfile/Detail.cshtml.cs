using System.Globalization;
using System.Security.Claims;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Public.StoreProfile
{
    public class DetailModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the catalog service in-process.
        private readonly IApiClient _api;

        public DetailModel(IApiClient api)
        {
            _api = api;
        }

        public StoreProfileDto Store { get; set; } = new();
        public PagedResult<CatalogProductDto> Products { get; set; } = new(new List<CatalogProductDto>(), 0, 1, 20);
        public List<CatalogCategoryDto> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? CategorySlug { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var storeProfile = await _api.GetAsync<StoreProfileDto>($"/api/stores/{Uri.EscapeDataString(slug)}", ct);
            if (storeProfile == null)
                return NotFound();

            Store = storeProfile;

            Products = await _api.GetAsync<PagedResult<CatalogProductDto>>(
                $"/api/stores/{Uri.EscapeDataString(slug)}/products{BuildProductsQuery()}", ct)
                ?? new PagedResult<CatalogProductDto>(new List<CatalogProductDto>(), 0, 1, 20);

            Categories = await _api.GetAsync<List<CatalogCategoryDto>>("/api/catalog/categories", ct)
                ?? new List<CatalogCategoryDto>();

            return Page();
        }

        // AJAX: Toggle Follow
        public async Task<IActionResult> OnPostFollowAsync(string slug, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new JsonResult(new { success = false, message = "Please login first" }) { StatusCode = 401 };

            if (string.IsNullOrWhiteSpace(slug))
                return new JsonResult(new { success = false, message = "Missing store slug" }) { StatusCode = 400 };

            var result = await _api.PostAndReadAsync<StoreFollowResultDto>(
                $"/api/stores/{Uri.EscapeDataString(slug.Trim())}/follow", ct);

            if (result == null)
                return new JsonResult(new { success = false, message = $"Store not found for slug '{slug}'" }) { StatusCode = 404 };

            return new JsonResult(new { success = true, isFollowing = result.IsFollowing, followerCount = result.FollowerCount });
        }

        private string BuildProductsQuery()
        {
            var parts = new List<string>
            {
                $"pageNumber={(PageNumber > 0 ? PageNumber : 1)}",
                "pageSize=20",
                $"sortBy={Uri.EscapeDataString(SortBy ?? "popular")}"
            };

            if (!string.IsNullOrWhiteSpace(CategorySlug)) parts.Add($"categorySlug={Uri.EscapeDataString(CategorySlug)}");
            if (MinPrice.HasValue) parts.Add($"minPrice={MinPrice.Value.ToString(CultureInfo.InvariantCulture)}");
            if (MaxPrice.HasValue) parts.Add($"maxPrice={MaxPrice.Value.ToString(CultureInfo.InvariantCulture)}");

            return "?" + string.Join("&", parts);
        }
    }
}
