using System;
using System.Collections.Generic;
using System.Linq;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Web.Pages.Models;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages
{
    public class IndexModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the catalog service in-process.
        // The forwarded auth cookie lets the API personalise the payload when signed in.
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        public HomePageDto HomePage { get; private set; } = new();
        public IReadOnlyList<HomeHeroSlideViewModel> HeroSlides { get; private set; } = Array.Empty<HomeHeroSlideViewModel>();
        public IReadOnlyList<HomeServiceStripItemViewModel> ServiceItems { get; private set; } = Array.Empty<HomeServiceStripItemViewModel>();

        public async Task OnGetAsync(CancellationToken ct)
        {
            HomePage = await _api.GetAsync<HomePageDto>("/api/catalog/home", ct) ?? new HomePageDto();
            HeroSlides = BuildHeroSlides(HomePage);
            ServiceItems = BuildServiceItems(HomePage);
        }

        private static IReadOnlyList<HomeHeroSlideViewModel> BuildHeroSlides(HomePageDto homePage)
        {
            return homePage.HeroProducts
                .Where(product => !string.IsNullOrWhiteSpace(product.ImageUrl))
                .Take(3)
                .Select((product, index) => new HomeHeroSlideViewModel
                {
                    Key = $"hero-product-{product.Id}",
                    Eyebrow = string.IsNullOrWhiteSpace(product.BrandName) ? "Featured hardware" : product.BrandName,
                    Title = product.Name,
                    Description = $"Available from {product.StoreName} with current pricing and stock information.",
                    PrimaryLabel = "View product",
                    PrimaryHref = $"/product/{product.Slug}",
                    SecondaryLabel = "Visit store",
                    SecondaryHref = string.IsNullOrWhiteSpace(product.StoreSlug) ? "/products" : $"/store/{product.StoreSlug}",
                    ImageUrl = product.ImageUrl,
                    ImageAlt = product.Name,
                    Tags = CompactTags(product.BrandName, product.StoreName)
                })
                .ToList();
        }

        private static IReadOnlyList<HomeServiceStripItemViewModel> BuildServiceItems(HomePageDto homePage)
        {
            var defaultItems = new List<HomeServiceStripItemViewModel>
            {
                new() { Icon = "local_shipping", Title = "Fast shipping", Subtitle = "Nationwide", Href = "/products", Tone = "blue" },
                new() { Icon = "storefront", Title = "Official stores", Subtitle = "Verified sellers", Href = homePage.Stores.FirstOrDefault()?.Href ?? "/products", Tone = "sky" },
                new() { Icon = "bolt", Title = "Flash deals", Subtitle = "Daily picks", Href = "/#home-flash-zone", Tone = "orange" },
                new() { Icon = "chat_bubble", Title = "Quick chat", Subtitle = "Profile inbox", Href = "/profile?tab=messages", Tone = "violet" },
                new() { Icon = "inventory_2", Title = "Setup gear", Subtitle = "Browse all", Href = ResolveHref(homePage.RecommendedRail.ViewAllHref, "/products"), Tone = "emerald" },
                new() { Icon = "workspace_premium", Title = "Top categories", Subtitle = "Jump faster", Href = "/products", Tone = "slate" }
            };

            var quickActions = homePage.QuickActions
                .Where(action => !string.IsNullOrWhiteSpace(action.Href))
                .Select(action => new HomeServiceStripItemViewModel
                {
                    Icon = action.Icon,
                    Title = action.Title,
                    Subtitle = action.Subtitle,
                    Href = action.Href,
                    Tone = string.IsNullOrWhiteSpace(action.Tone) ? "blue" : action.Tone
                })
                .Take(6)
                .ToList();

            return quickActions.Count >= 4 ? quickActions : defaultItems;
        }

        private static List<string> CompactTags(params string?[] highlights)
        {
            return highlights
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList();
        }

        private static string ResolveHref(string? href, string fallback)
        {
            return string.IsNullOrWhiteSpace(href) ? fallback : href;
        }
    }
}
