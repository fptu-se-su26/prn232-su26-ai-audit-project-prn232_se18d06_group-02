using System.Reflection;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.AiChat;
using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Application.Features.Catalog.DTOs;

namespace GearZone.Tests;

public sealed class AiChatToolExecutorTests
{
    [Theory]
    [InlineData("""{"query":"headphone","max_price":2000000}""", "headsets", null)]
    [InlineData("""{"query":"tai nghe","max_price":2000000}""", "headsets", null)]
    [InlineData("""{"query":"gaming headset HyperX"}""", "gaming-headsets", "hyperx")]
    [InlineData("""{"query":"tai nghe không dây"}""", "wireless-headphones", null)]
    [InlineData("""{"query":"headphone","category_slug":"headphones"}""", "headsets", null)]
    [InlineData("""{"category_slug":"headsets"}""", "headsets", null)]
    public async Task SearchProducts_NormalizesCategoryAliasesAndKeepsOnlyResidualKeywords(
        string arguments,
        string expectedCategorySlug,
        string? expectedSearch)
    {
        var (catalog, capture) = CreateCatalogCapture();
        var executor = new AiChatToolExecutor(
            catalog,
            CreateThrowingProxy<IOrderService>(),
            CreateThrowingProxy<IAiKnowledgeRepository>());

        await executor.ExecuteAsync(
            "search_products",
            arguments,
            new AiChatActor(null, "guest-session-hash"));

        Assert.NotNull(capture.LastFilter);
        Assert.Equal(expectedCategorySlug, capture.LastFilter.CategorySlug);
        Assert.Equal(expectedSearch, capture.LastFilter.Search);
        Assert.True(capture.LastFilter.InStockOnly);
        if (expectedCategorySlug == "headsets")
        {
            Assert.Equal(
                ["gaming-headsets", "wireless-headphones"],
                capture.LastFilter.CategorySlugs);
        }
    }

    [Fact]
    public async Task SearchProducts_PreservesBudgetWhileNormalizingVietnameseCategory()
    {
        var (catalog, capture) = CreateCatalogCapture();
        var executor = new AiChatToolExecutor(
            catalog,
            CreateThrowingProxy<IOrderService>(),
            CreateThrowingProxy<IAiKnowledgeRepository>());

        await executor.ExecuteAsync(
            "search_products",
            """{"query":"gợi ý tai nghe cho tôi","min_price":1000000,"max_price":2000000}""",
            new AiChatActor(null, "guest-session-hash"));

        Assert.NotNull(capture.LastFilter);
        Assert.Equal("headsets", capture.LastFilter.CategorySlug);
        Assert.Null(capture.LastFilter.Search);
        Assert.Equal(1_000_000m, capture.LastFilter.MinPrice);
        Assert.Equal(2_000_000m, capture.LastFilter.MaxPrice);
    }

    [Theory]
    [InlineData("search_my_orders", "{}")]
    [InlineData("get_my_order_tracking", """{"sub_order_id":"11111111-1111-1111-1111-111111111111"}""")]
    public async Task PersonalOrderTools_RejectGuestBeforeCallingAnyDataService(
        string toolName,
        string arguments)
    {
        var executor = new AiChatToolExecutor(
            CreateThrowingProxy<ICatalogService>(),
            CreateThrowingProxy<IOrderService>(),
            CreateThrowingProxy<IAiKnowledgeRepository>());

        var result = await executor.ExecuteAsync(
            toolName,
            arguments,
            new AiChatActor(null, "guest-session-hash"));

        Assert.Contains("authentication_required", result.Json, StringComparison.Ordinal);
        var action = Assert.Single(result.Metadata.Actions);
        Assert.Equal("login", action.Type);
    }

    private static T CreateThrowingProxy<T>() where T : class =>
        DispatchProxy.Create<T, ThrowingProxy>();

    private static (ICatalogService Service, CatalogCaptureProxy Capture) CreateCatalogCapture()
    {
        var service = DispatchProxy.Create<ICatalogService, CatalogCaptureProxy>();
        return (service, (CatalogCaptureProxy)(object)service);
    }

    public class ThrowingProxy : DispatchProxy
    {
        protected override object? Invoke(
            MethodInfo? targetMethod,
            object?[]? args) =>
            throw new InvalidOperationException(
                $"{targetMethod?.Name ?? "A dependency"} must not be called.");
    }

    public class CatalogCaptureProxy : DispatchProxy
    {
        public ProductFilterDto? LastFilter { get; private set; }

        protected override object? Invoke(
            MethodInfo? targetMethod,
            object?[]? args)
        {
            if (targetMethod?.Name == nameof(ICatalogService.GetCategoriesAsync))
            {
                return Task.FromResult(new List<CatalogCategoryDto>
                {
                    new()
                    {
                        Id = 3,
                        Name = "Headsets",
                        Slug = "headsets",
                        SubCategories =
                        [
                            new()
                            {
                                Id = 31,
                                Name = "Gaming Headsets",
                                Slug = "gaming-headsets"
                            },
                            new()
                            {
                                Id = 32,
                                Name = "Wireless Headphones",
                                Slug = "wireless-headphones"
                            }
                        ]
                    }
                });
            }

            if (targetMethod?.Name == nameof(ICatalogService.GetProductsAsync))
            {
                LastFilter = Assert.IsType<ProductFilterDto>(args![0]);
                return Task.FromResult(
                    new PagedResult<CatalogProductDto>(
                        [],
                        0,
                        LastFilter.PageNumber,
                        LastFilter.PageSize));
            }

            throw new InvalidOperationException(
                $"{targetMethod?.Name ?? "A dependency"} must not be called.");
        }
    }
}
