using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GearZone.Web.Pages.Public.Catalog
{
    public class BrowseModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the catalog service in-process.
        private readonly IApiClient _api;

        public BrowseModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty(Name = "brand", SupportsGet = true)]
        public List<string> Brand { get; set; } = new();

        [BindProperty(Name = "minPrice", SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(Name = "maxPrice", SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(Name = "sort", SupportsGet = true)]
        public string? Sort { get; set; }

        [BindProperty(Name = "page", SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(Name = "viewMode", SupportsGet = true)]
        public string? ViewMode { get; set; }

        [BindProperty(Name = "search", SupportsGet = true)]
        public string? Search { get; set; }

        public string? CategorySlug { get; set; }
        public int CategoryId { get; set; }

        public ProductFilterDto Filter { get; set; } = new ProductFilterDto();

        public CatalogFilterSidebarDto Sidebar { get; set; } = new CatalogFilterSidebarDto();

        public Application.Common.Models.PagedResult<CatalogProductDto> Products { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? slug, CancellationToken ct)
        {
            CategorySlug = slug;
            BuildFilterFromQuery();

            Sidebar = await _api.GetAsync<CatalogFilterSidebarDto>(
                $"/api/catalog/filters?slug={Uri.EscapeDataString(Filter.CategorySlug ?? string.Empty)}", ct)
                ?? new CatalogFilterSidebarDto();

            Products = await LoadProductsAsync(ct);

            if (Products.Items.Any())
            {
                CategoryId = Products.Items.First().CategoryId;
            }

            return Page();
        }

        public async Task<IActionResult> OnGetLoadMoreAsync(string? slug, CancellationToken ct)
        {
            CategorySlug = slug;
            BuildFilterFromQuery();

            Products = await LoadProductsAsync(ct);
            return new PartialViewResult
            {
                ViewName = "_ProductGridPartial",
                ViewData = new ViewDataDictionary<List<CatalogProductDto>>(ViewData, Products.Items) { ["ViewMode"] = Filter.ViewMode }
            };
        }

        public async Task<IActionResult> OnGetSuggestionsAsync(string query, CancellationToken ct)
        {
            var suggestions = await _api.GetAsync<List<ProductSuggestionDto>>(
                $"/api/products/suggestions?query={Uri.EscapeDataString(query ?? string.Empty)}", ct);
            return new JsonResult(suggestions ?? new List<ProductSuggestionDto>());
        }

        private async Task<Application.Common.Models.PagedResult<CatalogProductDto>> LoadProductsAsync(CancellationToken ct) =>
            await _api.GetAsync<Application.Common.Models.PagedResult<CatalogProductDto>>($"/api/products{BuildApiQueryString()}", ct)
            ?? new Application.Common.Models.PagedResult<CatalogProductDto>();

        // Maps the page's query names onto ProductFilterDto's, and re-emits the dynamic
        // attribute filters so the API can rebuild Filter.Attributes the same way.
        private string BuildApiQueryString()
        {
            var parts = new List<string>
            {
                $"pageNumber={Filter.PageNumber}",
                $"pageSize={Filter.PageSize}",
                $"viewMode={Uri.EscapeDataString(Filter.ViewMode)}"
            };

            if (!string.IsNullOrWhiteSpace(Filter.Search)) parts.Add($"search={Uri.EscapeDataString(Filter.Search)}");
            if (!string.IsNullOrWhiteSpace(Filter.CategorySlug)) parts.Add($"categorySlug={Uri.EscapeDataString(Filter.CategorySlug)}");
            if (Filter.MinPrice.HasValue) parts.Add($"minPrice={Filter.MinPrice.Value.ToString(CultureInfo.InvariantCulture)}");
            if (Filter.MaxPrice.HasValue) parts.Add($"maxPrice={Filter.MaxPrice.Value.ToString(CultureInfo.InvariantCulture)}");
            if (Filter.InStockOnly == true) parts.Add("inStockOnly=true");
            if (!string.IsNullOrWhiteSpace(Filter.SortBy)) parts.Add($"sortBy={Uri.EscapeDataString(Filter.SortBy)}");

            if (Filter.BrandSlugs != null)
            {
                foreach (var brand in Filter.BrandSlugs.Where(b => !string.IsNullOrWhiteSpace(b)))
                {
                    parts.Add($"brand={Uri.EscapeDataString(brand)}");
                }
            }

            if (Filter.Attributes != null)
            {
                foreach (var (key, values) in Filter.Attributes)
                {
                    foreach (var value in values.Where(v => !string.IsNullOrWhiteSpace(v)))
                    {
                        parts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
                    }
                }
            }

            return "?" + string.Join("&", parts);
        }

        private void BuildFilterFromQuery()
        {
            // Fallback bindings for AJAX handler requests
            if (int.TryParse(Request.Query["page"], out int p)) PageNumber = p;
            if (decimal.TryParse(Request.Query["minPrice"], out decimal min)) MinPrice = min;
            if (decimal.TryParse(Request.Query["maxPrice"], out decimal max)) MaxPrice = max;
            if (Request.Query.TryGetValue("sort", out var s)) Sort = s;
            if (Request.Query.TryGetValue("viewMode", out var vm)) ViewMode = vm;
            if (Request.Query.TryGetValue("search", out var q)) Search = q;
            if (Request.Query.TryGetValue("brand", out var b)) Brand = b.ToList();

            Filter = new ProductFilterDto
            {
                Search = Search,
                CategorySlug = CategorySlug,
                BrandSlugs = Brand.Any() ? Brand.SelectMany(br => br.Split(',')).ToList() : null,
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                SortBy = Sort,
                ViewMode = string.IsNullOrEmpty(ViewMode) ? "grid" : ViewMode,
                PageNumber = PageNumber > 0 ? PageNumber : 1,
                PageSize = 12, // Fixed page size
                InStockOnly = Request.Query["inStock"] == "true",
                Attributes = new Dictionary<string, List<string>>()
            };

            // Parse dynamic attributes from query string (excluding known parameters)
            var knownParams = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            {
                "brand", "minPrice", "maxPrice", "sort", "page", "inStock", "handler", "viewMode", "search"
            };

            foreach (var key in Request.Query.Keys)
            {
                if (!knownParams.Contains(key))
                {
                    var values = Request.Query[key].ToArray();
                    if (values.Length > 0 && values[0] != null)
                    {
                        // Clean values (in case they come in as comma-separated)
                        var valList = new List<string>();
                        foreach (var v in values)
                        {
                            if (v != null) valList.AddRange(v.Split(','));
                        }
                        Filter.Attributes[key] = valList;
                    }
                }
            }
        }
    }
}
