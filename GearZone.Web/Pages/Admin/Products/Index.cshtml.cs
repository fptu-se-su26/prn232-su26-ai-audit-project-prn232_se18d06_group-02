using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;

namespace GearZone.Web.Pages.Admin.Products
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
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

        public async Task OnGetAsync(CancellationToken ct)
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

            Query.PageNumber = Query.PageNumber < 1 ? 1 : Query.PageNumber;
            Query.PageSize = Query.PageSize < 1 ? 10 : Query.PageSize;

            var productsTask = _api.GetAsync<AdminProductListResponseDto>(
                $"/api/admin/products{ApiQueryStringBuilder.Build(Query)}", ct);
            var metadataTask = _api.GetAsync<AdminProductMetadataDto>("/api/admin/products/metadata", ct);
            Task<List<CategoryAttributeDto>?>? attributesTask = null;
            if (Query.CategoryId is > 0)
            {
                attributesTask = _api.GetAsync<List<CategoryAttributeDto>>(
                    $"/api/admin/categories/{Query.CategoryId.Value}/attributes", ct);
            }

            await Task.WhenAll(new Task[] { productsTask, metadataTask }
                .Concat(attributesTask is null ? Array.Empty<Task>() : new Task[] { attributesTask }));

            var data = await productsTask;
            if (data is not null)
            {
                Stats = data.Stats;
                Products = data.Products;
            }

            // Build hierarchical category list: roots first, then their children with "└ " prefix
            var metadata = await metadataTask ?? new AdminProductMetadataDto();
            var categories = metadata.Categories;
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

            Stores = metadata.Stores.Select(s => new SelectListItem(s.StoreName, s.Id.ToString())).ToList();

            Brands = metadata.Brands.Select(b => new SelectListItem(b.Name, b.Id.ToString())).ToList();

            // Load attributes for the selected category (if any)
            if (Query.CategoryId.HasValue && Query.CategoryId.Value > 0)
            {
                CategoryAttributes = await attributesTask! ?? new();
            }
        }

        /// <summary>AJAX endpoint: returns category attributes as JSON for dynamic filter rendering.</summary>
        public async Task<JsonResult> OnGetCategoryAttributesAsync(int categoryId, CancellationToken ct)
        {
            if (categoryId <= 0)
                return new JsonResult(new List<object>());

            var attrs = await _api.GetAsync<List<CategoryAttributeDto>>(
                $"/api/admin/categories/{categoryId}/attributes", ct) ?? new();
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

        public async Task<IActionResult> OnPostBulkUpdateStatusAsync(
            List<Guid> productIds, string actionType, string? reason = null, CancellationToken ct = default)
        {
            if (productIds == null || !productIds.Any())
                return RedirectToPage();

            var result = await _api.PostAsync(
                "/api/admin/products/bulk-update-status",
                new { productIds, action = actionType, reason }, ct);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message ?? "Products updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update product statuses.";
            }

            return RedirectToPage(new { Query.SearchTerm, Query.Status, Query.CategoryId, Query.BrandId, Query.StoreId, Query.PageNumber, DateRangeShortcut, DateRange });
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id, CancellationToken ct)
        {
            var result = await _api.PostAsync($"/api/admin/products/{id}/approve", ct);
            if (result.Success)
                TempData["SuccessMessage"] = "Product approved successfully.";
            else
                TempData["ErrorMessage"] = "Failed to approve product.";

            return RedirectToPage(new { Query.SearchTerm, Query.Status, Query.CategoryId, Query.BrandId, Query.StoreId, Query.PageNumber, DateRangeShortcut, DateRange });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id, CancellationToken ct)
        {
            var result = await _api.PostAsync(
                $"/api/admin/products/{id}/reject", new { reason = (string?)null }, ct);
            if (result.Success)
                TempData["SuccessMessage"] = "Product rejected.";
            else
                TempData["ErrorMessage"] = "Failed to reject product.";

            return RedirectToPage(new { Query.SearchTerm, Query.Status, Query.CategoryId, Query.BrandId, Query.StoreId, Query.PageNumber, DateRangeShortcut, DateRange });
        }
    }
}
