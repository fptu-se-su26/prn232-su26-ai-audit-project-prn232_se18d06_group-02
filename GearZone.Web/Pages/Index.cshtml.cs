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
            var heroProducts = homePage.HeroProducts
                .Where(product => !string.IsNullOrWhiteSpace(product.ImageUrl))
                .Take(5)
                .ToList();

            if (heroProducts.Any())
            {
                return heroProducts
                    .Select((product, index) => new HomeHeroSlideViewModel
                    {
                        Key = $"hero-product-{index + 1}",
                        Eyebrow = string.IsNullOrWhiteSpace(product.BrandName) ? "Fresh listing" : $"{product.BrandName} update",
                        Title = product.Name,
                        Description = $"{product.StoreName} just refreshed this listing, so the homepage now shows the latest product data and primary image without waiting for a hardcoded homepage refresh.",
                        PrimaryLabel = "View product",
                        PrimaryHref = $"/product/{product.Slug}",
                        SecondaryLabel = string.IsNullOrWhiteSpace(product.StoreSlug) ? "Browse catalog" : "Visit store",
                        SecondaryHref = string.IsNullOrWhiteSpace(product.StoreSlug) ? "/products" : $"/store/{product.StoreSlug}",
                        ImageUrl = product.ImageUrl,
                        ImageAlt = product.Name,
                        Tags = CompactTags(
                            product.BrandName,
                            product.StoreName,
                            product.HighlightTags.FirstOrDefault() ?? (product.IsInStock ? "In stock" : "Out of stock"))
                    })
                    .ToList();
            }

            return BuildFallbackHeroSlides(homePage);
        }

        private static IReadOnlyList<HomeHeroSlideViewModel> BuildFallbackHeroSlides(HomePageDto homePage)
        {
            var featuredStore = homePage.Stores.FirstOrDefault();
            var recommendedProduct = homePage.RecommendedRail.Products.FirstOrDefault();
            var flashProduct = homePage.FlashRail.Products.FirstOrDefault();
            var secondRecommendedProduct = homePage.RecommendedRail.Products.Skip(1).FirstOrDefault();
            var componentsHref = homePage.Categories
                .FirstOrDefault(category =>
                    category.Name.Contains("component", StringComparison.OrdinalIgnoreCase) ||
                    category.Name.Contains("processor", StringComparison.OrdinalIgnoreCase) ||
                    category.Name.Contains("graphics", StringComparison.OrdinalIgnoreCase))?.Href;
            var peripheralsHref = homePage.Categories
                .FirstOrDefault(category =>
                    category.Name.Contains("keyboard", StringComparison.OrdinalIgnoreCase) ||
                    category.Name.Contains("mouse", StringComparison.OrdinalIgnoreCase) ||
                    category.Name.Contains("headset", StringComparison.OrdinalIgnoreCase))?.Href;
            var monitorsHref = homePage.Categories
                .FirstOrDefault(category =>
                    category.Name.Contains("monitor", StringComparison.OrdinalIgnoreCase) ||
                    category.Name.Contains("display", StringComparison.OrdinalIgnoreCase))?.Href;
            var setupHref = homePage.Categories
                .FirstOrDefault(category =>
                    category.Name.Contains("setup", StringComparison.OrdinalIgnoreCase) ||
                    category.Name.Contains("furniture", StringComparison.OrdinalIgnoreCase) ||
                    category.Name.Contains("accessories", StringComparison.OrdinalIgnoreCase))?.Href;

            return new List<HomeHeroSlideViewModel>
            {
                new()
                {
                    Key = "hero-01",
                    Eyebrow = "Weekend setup sale",
                    Title = "Build the dream desk in one scroll.",
                    Description = "A tighter hero, direct product routes, and a cleaner marketplace rhythm without oversized promo blocks.",
                    PrimaryLabel = flashProduct != null ? "Shop flagship deal" : "Shop all gear",
                    PrimaryHref = flashProduct != null ? $"/product/{flashProduct.Slug}" : "/products",
                    SecondaryLabel = "Browse components",
                    SecondaryHref = ResolveHref(componentsHref, "/products"),
                    ImageUrl = "/images/home-hero/hero-01.svg",
                    ImageAlt = "Gaming desk setup hero artwork",
                    Tags = CompactTags(
                        "Fast desk deals",
                        "Verified routes",
                        "Direct to product")
                },
                new()
                {
                    Key = "hero-02",
                    Eyebrow = "Peripheral focus",
                    Title = "Keyboards, audio, and monitor picks that land fast.",
                    Description = "Move from a single banner straight into the most browsed setup categories without the right-side clutter.",
                    PrimaryLabel = recommendedProduct != null ? "View spotlight gear" : "Explore recommendations",
                    PrimaryHref = recommendedProduct != null
                        ? $"/product/{recommendedProduct.Slug}"
                        : ResolveHref(homePage.RecommendedRail.ViewAllHref, "/products"),
                    SecondaryLabel = "Browse peripherals",
                    SecondaryHref = ResolveHref(peripheralsHref, ResolveHref(homePage.RecommendedRail.ViewAllHref, "/products")),
                    ImageUrl = "/images/home-hero/hero-02.svg",
                    ImageAlt = "Peripherals and monitor hero artwork",
                    Tags = CompactTags(
                        recommendedProduct?.BrandName,
                        "Compact category scan",
                        "Creator-ready picks")
                },
                new()
                {
                    Key = "hero-03",
                    Eyebrow = "Official store route",
                    Title = "Visit the official GearZone store without leaving the flow.",
                    Description = "One clean banner still gives quick access to the verified store, message center, and seeded catalog routes underneath.",
                    PrimaryLabel = featuredStore != null ? "Visit official store" : "See flash picks",
                    PrimaryHref = featuredStore?.Href ?? (flashProduct != null ? $"/product/{flashProduct.Slug}" : "/products"),
                    SecondaryLabel = "Open messages",
                    SecondaryHref = "/profile?tab=messages",
                    ImageUrl = "/images/home-hero/hero-03.svg",
                    ImageAlt = "Official store and support hero artwork",
                    Tags = CompactTags(
                        featuredStore?.StoreName,
                        "Profile inbox",
                        "Realtime chat")
                },
                new()
                {
                    Key = "hero-04",
                    Eyebrow = "Display and creator lane",
                    Title = "Refresh your screen side with cleaner visuals.",
                    Description = "Keep the hero short and wide, then jump into monitor and creator gear from a single focused message.",
                    PrimaryLabel = secondRecommendedProduct != null ? "See creator pick" : "Explore setup gear",
                    PrimaryHref = secondRecommendedProduct != null
                        ? $"/product/{secondRecommendedProduct.Slug}"
                        : ResolveHref(homePage.RecommendedRail.ViewAllHref, "/products"),
                    SecondaryLabel = "Browse monitors",
                    SecondaryHref = ResolveHref(monitorsHref, "/products"),
                    ImageUrl = "/images/home-hero/hero-04.svg",
                    ImageAlt = "Display setup hero artwork",
                    Tags = CompactTags(
                        "Monitor lane",
                        "Creator-ready",
                        "Low profile layout")
                },
                new()
                {
                    Key = "hero-05",
                    Eyebrow = "Complete setup",
                    Title = "Finish the whole rig with the parts that matter.",
                    Description = "A short banner on top, then tighter cards below so the rest of the homepage still gets room to breathe.",
                    PrimaryLabel = "Browse setup gear",
                    PrimaryHref = ResolveHref(setupHref, ResolveHref(homePage.RecommendedRail.ViewAllHref, "/products")),
                    SecondaryLabel = "See all categories",
                    SecondaryHref = "/products",
                    ImageUrl = "/images/home-hero/hero-05.svg",
                    ImageAlt = "Complete setup hero artwork",
                    Tags = CompactTags(
                        "Tighter cards",
                        "Smaller hero",
                        "Cleaner scan")
                }
            };
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
