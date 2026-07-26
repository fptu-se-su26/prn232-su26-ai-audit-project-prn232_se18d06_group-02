using System.Security.Claims;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Products
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the product service in-process.
        // Filtering/sorting/paging and the stat tiles are computed by the API.
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        public List<SellerProductListDto> Products { get; set; } = new();
        public SellerProductStatsDto Stats { get; set; } = new();
        public List<SellerCategoryOptionDto> Categories { get; set; } = new();
        public List<SellerBrandOptionDto> Brands { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? BrandId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "createdAt";

        [BindProperty(SupportsGet = true)]
        public string SortDirection { get; set; } = "desc";

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Public/Auth/Login");

            PageNumber = PageNumber < 1 ? 1 : PageNumber;

            var data = await _api.GetAsync<SellerProductListResponseDto>($"/api/seller/products{BuildQueryString()}", ct);
            if (data == null) return RedirectToPage("/StoreOwner/Dashboard"); // no store

            Products = data.Items;
            Stats = data.Stats;
            TotalCount = data.TotalCount;
            PageSize = data.PageSize > 0 ? data.PageSize : PageSize;

            var metadata = await _api.GetAsync<SellerProductMetadataDto>("/api/seller/products/metadata", ct);
            if (metadata != null)
            {
                Categories = metadata.Categories;
                Brands = metadata.Brands;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Public/Auth/Login");

            var result = await _api.PatchAsync($"/api/seller/products/{id}/toggle-status", ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Product status updated!";
            }
            else
            {
                TempData["ErrorMessage"] = result.FirstError ?? "Failed to update product status.";
            }

            return RedirectToPage(new { SearchTerm, Status, CategoryId, BrandId, SortBy, SortDirection, PageNumber });
        }

        private string BuildQueryString()
        {
            var parts = new List<string>
            {
                $"page={PageNumber}",
                $"pageSize={PageSize}",
                $"sortBy={Uri.EscapeDataString(SortBy ?? "createdAt")}",
                $"sortDir={Uri.EscapeDataString(SortDirection ?? "desc")}"
            };

            if (!string.IsNullOrWhiteSpace(SearchTerm)) parts.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
            if (!string.IsNullOrWhiteSpace(Status)) parts.Add($"status={Uri.EscapeDataString(Status)}");
            if (CategoryId.HasValue) parts.Add($"categoryId={CategoryId.Value}");
            if (BrandId.HasValue) parts.Add($"brandId={BrandId.Value}");

            return "?" + string.Join("&", parts);
        }
    }
}
