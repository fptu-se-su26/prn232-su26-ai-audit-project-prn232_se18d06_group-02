using System.Text.Json;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Application.Features.Orders.Dtos;

namespace GearZone.Application.Features.AiChat;

public sealed class AiChatToolExecutor : IAiChatToolExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ICatalogService _catalog;
    private readonly IOrderService _orders;
    private readonly IAiKnowledgeRepository _knowledge;

    public AiChatToolExecutor(
        ICatalogService catalog,
        IOrderService orders,
        IAiKnowledgeRepository knowledge)
    {
        _catalog = catalog;
        _orders = orders;
        _knowledge = knowledge;
    }

    public async Task<AiToolExecutionResult> ExecuteAsync(
        string toolName,
        string argumentsJson,
        AiChatActor actor,
        CancellationToken ct = default)
    {
        using var document = ParseArguments(argumentsJson);
        var args = document.RootElement;

        return toolName switch
        {
            "search_products" => await SearchProductsAsync(args, ct),
            "get_product_details" => await GetProductDetailsAsync(args, ct),
            "search_knowledge" => await SearchKnowledgeAsync(args, ct),
            "search_my_orders" => await SearchOrdersAsync(args, actor, ct),
            "get_my_order_tracking" => await GetOrderTrackingAsync(args, actor, ct),
            "request_login" => LoginRequired(),
            "suggest_seller_chat" => await SuggestSellerChatAsync(args, ct),
            _ => JsonResult(new { error = "unknown_tool", tool = toolName })
        };
    }

    private async Task<AiToolExecutionResult> SearchProductsAsync(
        JsonElement args,
        CancellationToken ct)
    {
        var limit = Math.Clamp(GetInt(args, "limit") ?? 5, 1, 8);
        var normalizedSearch = AiProductSearchNormalizer.Normalize(
            GetString(args, "query"),
            GetString(args, "category_slug"),
            await _catalog.GetCategoriesAsync());
        var filter = new ProductFilterDto
        {
            Search = normalizedSearch.Search,
            CategorySlug = normalizedSearch.CategorySlug,
            CategorySlugs = normalizedSearch.CategorySlugs?.ToList(),
            BrandSlugs = GetStringArray(args, "brand_slugs")
                ?? (GetString(args, "brand_slug") is { Length: > 0 } brandSlug
                    ? new List<string> { brandSlug }
                    : null),
            MinPrice = GetDecimal(args, "min_price"),
            MaxPrice = GetDecimal(args, "max_price"),
            InStockOnly = GetBool(args, "in_stock_only") ?? true,
            SortBy = GetString(args, "sort_by"),
            PageNumber = 1,
            PageSize = limit
        };

        var products = await _catalog.GetProductsAsync(filter);
        var cards = products.Items.Select(MapProductCard).ToList();
        return JsonResult(
            new
            {
                count = cards.Count,
                products = cards.Select(x => new
                {
                    x.Name,
                    x.Slug,
                    x.BrandName,
                    x.Price,
                    x.StoreName,
                    x.StoreSlug,
                    x.Rating,
                    x.IsInStock,
                    x.Url
                })
            },
            new AiChatMessageMetadataDto
            {
                Products = cards,
                Sources = cards.Select(x => new AiChatSourceDto
                {
                    Type = "product",
                    Id = x.Slug,
                    Title = x.Name
                }).ToList()
            });
    }

    private async Task<AiToolExecutionResult> GetProductDetailsAsync(
        JsonElement args,
        CancellationToken ct)
    {
        var slugs = GetStringArray(args, "slugs")?.Take(3).ToArray()
            ?? (GetString(args, "product_slug") is { Length: > 0 } productSlug
                ? new[] { productSlug }
                : Array.Empty<string>());
        var details = new List<object>();
        var metadata = new AiChatMessageMetadataDto();

        foreach (var slug in slugs)
        {
            var product = await _catalog.GetProductDetailBySlugAsync(slug);
            if (product is null) continue;

            var minimumPrice = product.Variants.Count > 0
                ? product.Variants.Min(x => x.Price)
                : product.BasePrice;
            var inStock = product.Variants.Any(x => x.StockQuantity > 0);
            metadata.Products.Add(new AiProductCardDto
            {
                Name = product.Name,
                Slug = product.Slug,
                ImageUrl = product.ImageUrls.FirstOrDefault() ?? string.Empty,
                Price = minimumPrice,
                BrandName = product.BrandName,
                StoreName = product.StoreName,
                StoreSlug = product.StoreSlug,
                Rating = product.Rating,
                IsInStock = inStock,
                Url = $"/product/{product.Slug}"
            });
            metadata.Sources.Add(new AiChatSourceDto
            {
                Type = "product",
                Id = product.Slug,
                Title = product.Name
            });

            details.Add(new
            {
                product.Name,
                product.Slug,
                product.BrandName,
                product.CategoryName,
                product.Description,
                product.Rating,
                product.ReviewCount,
                product.StoreName,
                product.StoreSlug,
                price = minimumPrice,
                inStock,
                specifications = product.Specifications
                    .Take(30)
                    .Select(x => new { x.Name, x.Value }),
                variants = product.Variants
                    .Take(20)
                    .Select(x => new
                    {
                        x.VariantName,
                        x.Price,
                        x.StockQuantity
                    })
            });
        }

        return JsonResult(new { products = details }, metadata);
    }

    private async Task<AiToolExecutionResult> SearchKnowledgeAsync(
        JsonElement args,
        CancellationToken ct)
    {
        var query = GetString(args, "query") ?? string.Empty;
        var category = GetString(args, "category");
        var limit = Math.Clamp(GetInt(args, "limit") ?? 3, 1, 5);
        var articles = await _knowledge.SearchPublishedAsync(query, category, limit, ct);
        var sources = articles.Select(x => new AiChatSourceDto
        {
            Type = "knowledge",
            Id = x.Slug,
            Title = x.Title
        }).ToList();

        return JsonResult(
            new
            {
                count = articles.Count,
                articles = articles.Select(x => new
                {
                    x.Title,
                    category = x.Category.ToString(),
                    x.Keywords,
                    content = x.Content.Length <= 6000 ? x.Content : x.Content[..6000],
                    x.UpdatedAtUtc
                })
            },
            new AiChatMessageMetadataDto { Sources = sources });
    }

    private async Task<AiToolExecutionResult> SearchOrdersAsync(
        JsonElement args,
        AiChatActor actor,
        CancellationToken ct)
    {
        if (!actor.IsCustomer)
        {
            return AuthenticationRequired();
        }

        var result = await _orders.GetUserOrdersAsync(
            actor.CustomerUserId!,
            new UserOrderQueryDto
            {
                Status = GetString(args, "status") ?? "all",
                SearchTerm = GetString(args, "search_term") ?? GetString(args, "query"),
                PageNumber = 1,
                PageSize = Math.Clamp(GetInt(args, "limit") ?? 5, 1, 5)
            });

        var cards = result.Items.Select(MapOrderCard).ToList();
        return JsonResult(
            new
            {
                count = cards.Count,
                orders = cards
            },
            new AiChatMessageMetadataDto { Orders = cards });
    }

    private async Task<AiToolExecutionResult> GetOrderTrackingAsync(
        JsonElement args,
        AiChatActor actor,
        CancellationToken ct)
    {
        if (!actor.IsCustomer)
        {
            return AuthenticationRequired();
        }

        var rawId = GetString(args, "sub_order_id");
        if (!Guid.TryParse(rawId, out var subOrderId))
        {
            return JsonResult(new { error = "invalid_sub_order_id" });
        }

        var tracking = await _orders.GetUserOrderTrackingAsync(
            actor.CustomerUserId!,
            subOrderId,
            ct);
        if (tracking is null)
        {
            return JsonResult(new { error = "order_not_found_or_access_denied" });
        }

        var card = new AiOrderCardDto
        {
            SubOrderId = tracking.SubOrderId,
            OrderCode = tracking.OrderCode,
            StoreName = tracking.StoreName,
            Status = tracking.Status.ToString(),
            Subtotal = tracking.Subtotal,
            CreatedAtUtc = tracking.CreatedAt,
            Url = $"/orders/track/{tracking.SubOrderId}"
        };

        return JsonResult(
            new
            {
                tracking.OrderCode,
                tracking.StoreName,
                status = tracking.Status.ToString(),
                tracking.CreatedAt,
                tracking.UpdatedAt,
                tracking.DeliveredAt,
                tracking.Subtotal,
                tracking.ShippingFee,
                tracking.GrandTotal,
                tracking.ShippingProvider,
                tracking.TrackingNumber,
                items = tracking.Items.Select(x => new
                {
                    x.ProductName,
                    x.ProductSlug,
                    x.VariantName,
                    x.Quantity,
                    x.LineTotal
                }),
                history = tracking.StatusHistory.Select(x => new
                {
                    x.ChangedAt,
                    oldStatus = x.OldStatus?.ToString(),
                    newStatus = x.NewStatus.ToString(),
                    x.Note
                })
            },
            new AiChatMessageMetadataDto { Orders = new List<AiOrderCardDto> { card } });
    }

    private async Task<AiToolExecutionResult> SuggestSellerChatAsync(
        JsonElement args,
        CancellationToken ct)
    {
        var storeSlug = GetString(args, "store_slug");
        if (string.IsNullOrWhiteSpace(storeSlug))
        {
            return JsonResult(new { error = "store_slug_required" });
        }

        var store = await _catalog.GetStoreProfileAsync(storeSlug);
        if (store is null)
        {
            return JsonResult(new { error = "store_not_found" });
        }

        var productSlug = GetString(args, "product_slug");
        return JsonResult(
            new { ok = true, store = store.StoreName, storeSlug, productSlug },
            new AiChatMessageMetadataDto
            {
                Actions = new List<AiSuggestedActionDto>
                {
                    new()
                    {
                        Type = "seller_chat",
                        Label = $"Chat with {store.StoreName}",
                        StoreSlug = storeSlug,
                        ProductSlug = productSlug
                    }
                }
            });
    }

    private static AiToolExecutionResult AuthenticationRequired() =>
        JsonResult(
            new { error = "authentication_required", message = "The customer must sign in to access order information." },
            new AiChatMessageMetadataDto
            {
                Actions = new List<AiSuggestedActionDto>
                {
                    new()
                    {
                        Type = "login",
                        Label = "Sign in to view orders",
                        Url = "/Public/Auth/Login"
                    }
                }
            });

    private static AiToolExecutionResult LoginRequired() => AuthenticationRequired();

    private static AiProductCardDto MapProductCard(CatalogProductDto product) => new()
    {
        Name = product.Name,
        Slug = product.Slug,
        ImageUrl = product.ImageUrl,
        Price = product.BasePrice,
        BrandName = product.BrandName,
        StoreName = product.StoreName,
        StoreSlug = product.StoreSlug,
        Rating = product.Rating,
        IsInStock = product.IsInStock,
        Url = $"/product/{product.Slug}"
    };

    private static AiOrderCardDto MapOrderCard(UserOrderDto order) => new()
    {
        SubOrderId = order.SubOrderId,
        OrderCode = order.OrderCode,
        StoreName = order.StoreName,
        Status = order.Status.ToString(),
        Subtotal = order.Subtotal,
        CreatedAtUtc = order.CreatedAt,
        Url = $"/orders/track/{order.SubOrderId}"
    };

    private static AiToolExecutionResult JsonResult(
        object value,
        AiChatMessageMetadataDto? metadata = null) => new()
    {
        Json = JsonSerializer.Serialize(value, JsonOptions),
        Metadata = metadata ?? new AiChatMessageMetadataDto()
    };

    private static JsonDocument ParseArguments(string json)
    {
        try
        {
            return JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        }
        catch (JsonException)
        {
            return JsonDocument.Parse("{}");
        }
    }

    private static string? GetString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value)) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString()?.Trim() : value.ToString();
    }

    private static int? GetInt(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetInt32(out var parsed)
            ? parsed
            : null;

    private static decimal? GetDecimal(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetDecimal(out var parsed)
            ? parsed
            : null;

    private static bool? GetBool(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind is JsonValueKind.True or JsonValueKind.False) return value.GetBoolean();
        return bool.TryParse(value.ToString(), out var parsed) ? parsed : null;
    }

    private static List<string>? GetStringArray(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return value
            .EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.String)
            .Select(x => x.GetString()?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToList();
    }
}
