using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;

namespace GearZone.Web.Pages.Admin.Products
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminProductService _productService;
        private readonly IAdminCategoryService _categoryService;
        private readonly IAdminStoreService _storeService;
        private readonly IAdminBrandService _brandService;

        public IndexModel(IAdminProductService productService, IAdminCategoryService categoryService, IAdminStoreService storeService, IAdminBrandService brandService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _storeService = storeService;
            _brandService = brandService;
        }

        [BindProperty(SupportsGet = true)]
        public AdminProductQueryDto Query { get; set; } = new AdminProductQueryDto();

        [BindProperty(SupportsGet = true)]
        public string? DateRangeShortcut { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRange { get; set; }

        public PagedResult<AdminProductDto> Products { get; set; } = new PagedResult<AdminProductDto>();
        public AdminProductStatsDto Stats { get; set; } = new AdminProductStatsDto();

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Stores { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Brands { get; set; } = new List<SelectListItem>();

        /// <summary>Attributes for the currently selected category, used to render dynamic filters.</summary>
        public List<CategoryAttributeDto> CategoryAttributes { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(DateRangeShortcut))
            {
                var today = DateTime.UtcNow.Date;
                switch (DateRangeShortcut.ToLower())
                {
                    case "today":
                        Query.StartDate = today;
                        Query.EndDate = today;
                        break;
                    case "week":
                        Query.StartDate = today.AddDays(-7);
                        Query.EndDate = today;
                        break;
                    case "month":
                        Query.StartDate = today.AddDays(-30);
                        Query.EndDate = today;
                        break;
                    case "custom":
                        if (!string.IsNullOrEmpty(DateRange))
                        {
                            var dates = DateRange.Split(" to ");
                            if (dates.Length == 2)
                            {
                                if (DateTime.TryParse(dates[0], out var start)) Query.StartDate = start;
                                if (DateTime.TryParse(dates[1], out var end)) Query.EndDate = end;
                            }
                            else if (dates.Length == 1)
                            {
                                if (DateTime.TryParse(dates[0], out var start))
                                {
                                    Query.StartDate = start;
                                    Query.EndDate = start;
                                }
                            }
                        }
                        break;
                }
            }

            Stats = await _productService.GetProductStatsAsync();
            Products = await _productService.GetProductsAsync(Query);

            // Build hierarchical category list: roots first, then their children with "└ " prefix
            var categories = await _categoryService.GetAllCategoriesListAsync();
            var roots = categories.Where(c => c.ParentId == null).OrderBy(c => c.Name);
            var categoryItems = new List<SelectListItem>();
            foreach (var root in roots)
            {
                categoryItems.Add(new SelectListItem(root.Name, root.Id.ToString()));
                var children = categories
                    .Where(c => c.ParentId == root.Id)
                    .OrderBy(c => c.Name);
                foreach (var child in children)
                {
                    categoryItems.Add(new SelectListItem($"└ {child.Name}", child.Id.ToString()));
                }
            }
            Categories = categoryItems;

            var stores = await _storeService.GetAllStoresAsync();
            Stores = stores.Select(s => new SelectListItem(s.StoreName, s.Id.ToString())).ToList();

            var brands = await _brandService.GetAllBrandsListAsync();
            Brands = brands.Select(b => new SelectListItem(b.Name, b.Id.ToString())).ToList();

            // Load attributes for the selected category (if any)
            if (Query.CategoryId.HasValue && Query.CategoryId.Value > 0)
            {
                CategoryAttributes = await _categoryService.GetAttributesByCategoryIdAsync(Query.CategoryId.Value);
            }
        }

        /// <summary>AJAX endpoint: returns category attributes as JSON for dynamic filter rendering.</summary>
        public async Task<JsonResult> OnGetCategoryAttributesAsync(int categoryId)
        {
            if (categoryId <= 0)
                return new JsonResult(new List<object>());

            var attrs = await _categoryService.GetAttributesByCategoryIdAsync(categoryId);
            var result = attrs
                .Where(a => a.IsFilterable)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.FilterType,
                    Options = a.Options.Select(o => new { o.Id, o.Value })
                });
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostBulkUpdateStatusAsync(List<Guid> productIds, string actionType, string? reason = null)
        {
            if (productIds == null || !productIds.Any())
                return RedirectToPage();

            if (actionType.Equals("Delete", StringComparison.OrdinalIgnoreCase))
            {
                int deleteCount = 0;
                foreach (var id in productIds)
                {
                    if (await _productService.DeleteProductAsync(id, reason ?? "No reason provided"))
                        deleteCount++;
                }
                TempData["SuccessMessage"] = $"Successfully deleted {deleteCount} product(s).";
                return RedirectToPage(new { Query.SearchTerm, Query.Status, Query.CategoryId, Query.BrandId, Query.StoreId, Query.PageNumber, DateRangeShortcut, DateRange });
            }

            ProductStatus status;
            switch (actionType.ToLower())
            {
                case "approve":
                    status = ProductStatus.Active;
                    break;
                case "reject":
                    status = ProductStatus.Rejected;
                    break;
                case "inactive":
                    status = ProductStatus.Inactive;
                    break;
                default:
                    return RedirectToPage();
            }

            var success = await _productService.BulkUpdateStatusAsync(productIds, status, reason);

            if (success)
            {
                TempData["SuccessMessage"] = $"Successfully updated {productIds.Count} product(s) to {status}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update product statuses.";
            }

            return RedirectToPage(new { Query.SearchTerm, Query.Status, Query.CategoryId, Query.BrandId, Query.StoreId, Query.PageNumber, DateRangeShortcut, DateRange });
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id)
        {
            var success = await _productService.BulkUpdateStatusAsync(new List<Guid> { id }, ProductStatus.Active);
            if (success)
                TempData["SuccessMessage"] = "Product approved successfully.";
            else
                TempData["ErrorMessage"] = "Failed to approve product.";

            return RedirectToPage(new { Query.SearchTerm, Query.Status, Query.CategoryId, Query.BrandId, Query.StoreId, Query.PageNumber, DateRangeShortcut, DateRange });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id)
        {
            var success = await _productService.BulkUpdateStatusAsync(new List<Guid> { id }, ProductStatus.Rejected);
            if (success)
                TempData["SuccessMessage"] = "Product rejected.";
            else
                TempData["ErrorMessage"] = "Failed to reject product.";

            return RedirectToPage(new { Query.SearchTerm, Query.Status, Query.CategoryId, Query.BrandId, Query.StoreId, Query.PageNumber, DateRangeShortcut, DateRange });
        }
    }
}
