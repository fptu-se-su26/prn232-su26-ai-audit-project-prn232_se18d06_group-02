using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using GearZone.Application.Abstractions.Services;

namespace GearZone.Web.Pages.Public.Catalog
{
    public class BrowseModel : PageModel
    {
        private readonly ICatalogService _catalogService;

        public BrowseModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
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

        public async Task<IActionResult> OnGetAsync(string? slug)
        {
            CategorySlug = slug;
            BuildFilterFromQuery();

            Sidebar = await _catalogService.GetFiltersForCategoryAsync(Filter.CategorySlug);
            Products = await _catalogService.GetProductsAsync(Filter);
            
            if (Products.Items.Any())
            {
                CategoryId = Products.Items.First().CategoryId;
            }

            return Page();
        }

        public async Task<IActionResult> OnGetLoadMoreAsync(string? slug)
        {
            CategorySlug = slug;
            BuildFilterFromQuery();
            
            Products = await _catalogService.GetProductsAsync(Filter);
            return new PartialViewResult
            {
                ViewName = "_ProductGridPartial",
                ViewData = new ViewDataDictionary<List<CatalogProductDto>>(ViewData, Products.Items) { ["ViewMode"] = Filter.ViewMode }
            };
        }

        public async Task<IActionResult> OnGetSuggestionsAsync(string query)
        {
            var suggestions = await _catalogService.GetProductSuggestionsAsync(query);
            return new JsonResult(suggestions);
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
