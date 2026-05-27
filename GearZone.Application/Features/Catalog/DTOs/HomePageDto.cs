using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Catalog.DTOs
{
    public class HomePageDto
    {
        public HomeHeroDto Hero { get; set; } = new();
        public HomePromoCardDto PromoCard { get; set; } = new();
        public List<CatalogProductDto> HeroProducts { get; set; } = new();
        public List<HomeQuickActionDto> QuickActions { get; set; } = new();
        public List<HomeCategoryTileDto> Categories { get; set; } = new();
        public HomeProductRailDto FlashRail { get; set; } = new();
        public List<HomeStoreCardDto> Stores { get; set; } = new();
        public HomeProductRailDto RecommendedRail { get; set; } = new();
    }

    public class HomeHeroDto
    {
        public string Eyebrow { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AccentTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PrimaryLabel { get; set; } = string.Empty;
        public string PrimaryHref { get; set; } = string.Empty;
        public string SecondaryLabel { get; set; } = string.Empty;
        public string SecondaryHref { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? ProductSlug { get; set; }
        public string? ProductName { get; set; }
        public string? StoreName { get; set; }
        public string? StoreHref { get; set; }
        public List<string> Highlights { get; set; } = new();
    }

    public class HomePromoCardDto
    {
        public string Eyebrow { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LinkLabel { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class HomeQuickActionDto
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string Tone { get; set; } = string.Empty;
    }

    public class HomeCategoryTileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public string Subtitle { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string Tone { get; set; } = string.Empty;
    }

    public class HomeProductRailDto
    {
        public string Eyebrow { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ViewAllLabel { get; set; } = string.Empty;
        public string ViewAllHref { get; set; } = string.Empty;
        public List<CatalogProductDto> Products { get; set; } = new();
    }

    public class HomeStoreCardDto
    {
        public Guid Id { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string Province { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int TotalSold { get; set; }
        public decimal Rating { get; set; }
        public int ReviewCount { get; set; }
        public int FollowerCount { get; set; }
        public string Href { get; set; } = string.Empty;
    }
}
