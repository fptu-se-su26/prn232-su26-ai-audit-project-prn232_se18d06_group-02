using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Domain.Entities;
using GearZone.Application.Abstractions.Services;

namespace GearZone.Web.Pages.Public.StoreProfile
{
    public class DetailModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(ICatalogService catalogService, UserManager<ApplicationUser> userManager)
        {
            _catalogService = catalogService;
            _userManager = userManager;
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

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var storeProfile = await _catalogService.GetStoreProfileAsync(slug, currentUserId);
            if (storeProfile == null)
                return NotFound();

            Store = storeProfile;

            var filter = new ProductFilterDto
            {
                StoreId = Store.Id,
                CategorySlug = CategorySlug,
                SortBy = SortBy ?? "popular",
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                PageNumber = PageNumber,
                PageSize = 20
            };

            Products = await _catalogService.GetProductsAsync(filter);
            Categories = await _catalogService.GetCategoriesAsync();

            return Page();
        }

        // AJAX: Toggle Follow
        public async Task<IActionResult> OnPostFollowAsync(string slug)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return new JsonResult(new { success = false, message = "Please login first" }) { StatusCode = 401 };

                if (string.IsNullOrWhiteSpace(slug))
                    return new JsonResult(new { success = false, message = "Missing store slug" }) { StatusCode = 400 };

                var store = await _catalogService.GetStoreProfileAsync(slug.Trim());
                if (store == null)
                    return new JsonResult(new { success = false, message = $"Store not found for slug '{slug}'" }) { StatusCode = 404 };

                var isFollowing = await _catalogService.ToggleFollowAsync(userId, store.Id);
                var followerCount = await _catalogService.GetFollowerCountAsync(store.Id);

                return new JsonResult(new { success = true, isFollowing, followerCount });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 500 };
            }
        }

    }
}
