using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.StoreOwner.Products
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private readonly ISellerProductService _productService;
        private readonly ISellerStoreService _storeService;

        public IndexModel(ISellerProductService productService, ISellerStoreService storeService)
        {
            _productService = productService;
            _storeService = storeService;
        }

        public List<SellerProductListDto> Products { get; set; } = new();
        public ProductStatsViewModel Stats { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Brand> Brands { get; set; } = new();

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

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Public/Auth/Login");

            var store = await _storeService.GetStoreByOwnerIdAsync(userId);
            if (store == null) return RedirectToPage("/StoreOwner/Dashboard");

            // Fetch all data (Service is currently simple)
            var allProducts = await _productService.GetProductsByStoreAsync(store.Id);
            Categories = await _productService.GetCategoriesAsync();
            Brands = await _productService.GetBrandsAsync();

            // Calculate Stats
            Stats = new ProductStatsViewModel
            {
                TotalProducts = allProducts.Count,
                ActiveProducts = allProducts.Count(p => p.Status == "Active"),
                OutofStockProducts = allProducts.Count(p => p.TotalStock == 0),
                DraftProducts = allProducts.Count(p => p.Status == "Draft"),
                PendingProducts = allProducts.Count(p => p.Status == "Pending")
            };

            // Apply Filters in-memory
            var query = allProducts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = NormalizeForSearch(SearchTerm);
                query = query.Where(p => 
                    NormalizeForSearch(p.Name).Contains(term, StringComparison.Ordinal) ||
                    NormalizeForSearch(p.CategoryName).Contains(term, StringComparison.Ordinal) ||
                    NormalizeForSearch(p.BrandName).Contains(term, StringComparison.Ordinal));
            }

            if (!string.IsNullOrWhiteSpace(Status))
            {
                query = query.Where(p => p.Status == Status);
            }

            if (CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == CategoryId.Value);
            }

            if (BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == BrandId.Value);
            }

            // Sorting
            query = SortBy switch
            {
                "name" => SortDirection == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "price" => SortDirection == "asc" ? query.OrderBy(p => p.BasePrice) : query.OrderByDescending(p => p.BasePrice),
                "stock" => SortDirection == "asc" ? query.OrderBy(p => p.TotalStock) : query.OrderByDescending(p => p.TotalStock),
                _ => SortDirection == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
            };

            TotalCount = query.Count();
            Products = query.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var store = await _storeService.GetStoreByOwnerIdAsync(userId!);
            
            if (store == null) return RedirectToPage("/StoreOwner/Dashboard");

            try
            {
                await _productService.ToggleProductStatusAsync(id, store.Id);
                TempData["SuccessMessage"] = "Product status updated!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { SearchTerm, Status, CategoryId, BrandId, SortBy, SortDirection, PageNumber });
        }

        public class ProductStatsViewModel
        {
            public int TotalProducts { get; set; }
            public int ActiveProducts { get; set; }
            public int OutofStockProducts { get; set; }
            public int DraftProducts { get; set; }
            public int PendingProducts { get; set; }
        }

        private static string NormalizeForSearch(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

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
    }
}
