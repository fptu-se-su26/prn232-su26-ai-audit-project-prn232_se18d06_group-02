using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GearZone.Infrastructure.External;

public sealed class GeminiAiChatProvider : IAiChatProvider
{
    private const int MaxToolRounds = 4;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(750)
    ];

    private readonly HttpClient _httpClient;
    private readonly AiChatSettings _settings;
    private readonly ILogger<GeminiAiChatProvider> _logger;

    public GeminiAiChatProvider(
        HttpClient httpClient,
        IOptions<AiChatSettings> settings,
        ILogger<GeminiAiChatProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public string Model => _settings.GeminiModel;

    public bool IsEnabled =>
        _settings.Enabled &&
        !string.IsNullOrWhiteSpace(_settings.GeminiApiKey) &&
        !string.IsNullOrWhiteSpace(_settings.GeminiModel);

    public async Task<AiChatProviderResult> GenerateAsync(
        AiChatProviderRequest request,
        Func<string, string, CancellationToken, Task<AiToolExecutionResult>> executeTool,
        Func<string, CancellationToken, Task>? onToolStarted = null,
        CancellationToken ct = default)
    {
        if (!IsEnabled)
        {
            throw new AiChatUnavailableException(
                "AI chat is disabled or the Gemini configuration is incomplete.");
        }

        var contents = BuildInitialContents(request);
        var metadata = new AiChatMessageMetadataDto();
        var inputTokens = 0;
        var outputTokens = 0;

        for (var round = 0; round < MaxToolRounds; round++)
        {
            var response = await SendAsync(BuildPayload(request.SystemInstruction, contents), ct);
            inputTokens += response["usageMetadata"]?["promptTokenCount"]?.GetValue<int>() ?? 0;
            outputTokens += response["usageMetadata"]?["candidatesTokenCount"]?.GetValue<int>() ?? 0;

            if (IsBlocked(response))
            {
                return new AiChatProviderResult
                {
                    Text = "I can't help with that content. You can ask about GearZone products, policies, or orders.",
                    Model = Model,
                    InputTokens = inputTokens,
                    OutputTokens = outputTokens,
                    Metadata = metadata,
                    WasBlocked = true
                };
            }

            var candidateContent = response["candidates"]?[0]?["content"] as JsonObject
                ?? throw new AiChatUnavailableException("Gemini returned an empty response.");
            var parts = candidateContent["parts"] as JsonArray ?? [];
            var textParts = new List<string>();
            var calls = new List<GeminiFunctionCall>();

            foreach (var partNode in parts)
            {
                if (partNode?["text"]?.GetValue<string>() is { Length: > 0 } text)
                {
                    textParts.Add(text);
                }

                if (partNode?["functionCall"] is not JsonObject functionCall)
                {
                    continue;
                }

                var name = functionCall["name"]?.GetValue<string>() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                calls.Add(new GeminiFunctionCall(
                    name,
                    functionCall["id"]?.GetValue<string>(),
                    functionCall["args"]?.ToJsonString(JsonOptions) ?? "{}"));
            }

            if (calls.Count == 0)
            {
                var answer = string.Join(Environment.NewLine, textParts).Trim();
                if (string.IsNullOrWhiteSpace(answer))
                {
                    throw new AiChatUnavailableException("Gemini did not return a usable answer.");
                }

                return new AiChatProviderResult
                {
                    Text = answer,
                    Model = Model,
                    InputTokens = inputTokens,
                    OutputTokens = outputTokens,
                    Metadata = metadata
                };
            }

            // Gemini 3 requires the model content (including thought signatures and
            // function-call IDs) to be copied into the next request unchanged.
            contents.Add(candidateContent.DeepClone());

            var responseParts = new JsonArray();
            foreach (var call in calls)
            {
                if (onToolStarted is not null)
                {
                    await onToolStarted(call.Name, ct);
                }

                var result = await executeTool(call.Name, call.ArgumentsJson, ct);
                MergeMetadata(metadata, result.Metadata);

                JsonNode responseNode;
                try
                {
                    responseNode = JsonNode.Parse(result.Json) ?? new JsonObject();
                }
                catch (JsonException)
                {
                    responseNode = new JsonObject { ["error"] = "Tool returned invalid JSON." };
                }

                if (responseNode is not JsonObject)
                {
                    responseNode = new JsonObject { ["result"] = responseNode };
                }

                var functionResponse = new JsonObject
                {
                    ["name"] = call.Name,
                    ["response"] = responseNode
                };
                if (!string.IsNullOrWhiteSpace(call.Id))
                {
                    functionResponse["id"] = call.Id;
                }

                responseParts.Add(new JsonObject
                {
                    ["functionResponse"] = functionResponse
                });
            }

            contents.Add(new JsonObject
            {
                ["role"] = "user",
                ["parts"] = responseParts
            });
        }

        throw new AiChatUnavailableException("Gemini exceeded the allowed tool-call rounds.");
    }

    private JsonObject BuildPayload(string systemInstruction, JsonArray contents) => new()
    {
        ["system_instruction"] = new JsonObject
        {
            ["parts"] = new JsonArray(new JsonObject { ["text"] = systemInstruction })
        },
        ["contents"] = contents.DeepClone(),
        ["tools"] = ToolDefinitions.DeepClone(),
        ["tool_config"] = new JsonObject
        {
            ["function_calling_config"] = new JsonObject { ["mode"] = "AUTO" }
        },
        ["generationConfig"] = new JsonObject
        {
            ["maxOutputTokens"] = _settings.MaxOutputTokens,
            ["temperature"] = 0.25
        }
    };

    private async Task<JsonObject> SendAsync(JsonObject payload, CancellationToken ct)
    {
        var relativeUri = $"models/{Uri.EscapeDataString(Model)}:generateContent";

        for (var attempt = 0; ; attempt++)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, relativeUri)
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };
            message.Headers.TryAddWithoutValidation("x-goog-api-key", _settings.GeminiApiKey);

            using var response = await _httpClient.SendAsync(
                message,
                HttpCompletionOption.ResponseHeadersRead,
                ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return JsonNode.Parse(body) as JsonObject
                        ?? throw new AiChatUnavailableException("Gemini returned invalid JSON.");
                }
                catch (JsonException ex)
                {
                    throw new AiChatUnavailableException("Gemini returned invalid JSON.", ex);
                }
            }

            var retryable = response.StatusCode == HttpStatusCode.TooManyRequests ||
                            (int)response.StatusCode >= 500;
            if (retryable && attempt < RetryDelays.Length)
            {
                _logger.LogWarning(
                    "Gemini chat request returned {StatusCode}; retrying attempt {Attempt}.",
                    (int)response.StatusCode,
                    attempt + 2);
                await Task.Delay(RetryDelays[attempt], ct);
                continue;
            }

            _logger.LogWarning(
                "Gemini chat request failed with status {StatusCode}. Body={Body}",
                (int)response.StatusCode,
                Truncate(body, 500));
            throw new AiChatUnavailableException(
                $"Gemini request failed with HTTP {(int)response.StatusCode}.");
        }
    }

    private static JsonArray BuildInitialContents(AiChatProviderRequest request)
    {
        var contents = new JsonArray();
        foreach (var item in request.History)
        {
            contents.Add(TextContent(item.Role == "model" ? "model" : "user", item.Content));
        }

        var context = BuildPageContext(request.PageContext);
        var message = string.IsNullOrWhiteSpace(context)
            ? request.UserMessage
            : $"{request.UserMessage}\n\nCurrent page hints (validate with tools before using): {context}";
        contents.Add(TextContent("user", message));
        return contents;
    }

    private static JsonObject TextContent(string role, string text) => new()
    {
        ["role"] = role,
        ["parts"] = new JsonArray(new JsonObject { ["text"] = text })
    };

    private static string BuildPageContext(AiChatPageContextDto? context)
    {
        if (context is null) return string.Empty;
        var values = new List<string>();
        if (!string.IsNullOrWhiteSpace(context.ProductSlug))
        {
            values.Add($"productSlug={context.ProductSlug.Trim()}");
        }
        if (!string.IsNullOrWhiteSpace(context.StoreSlug))
        {
            values.Add($"storeSlug={context.StoreSlug.Trim()}");
        }
        return string.Join(", ", values);
    }

    private static bool IsBlocked(JsonObject response)
    {
        if (!string.IsNullOrWhiteSpace(response["promptFeedback"]?["blockReason"]?.GetValue<string>()))
        {
            return true;
        }

        var finishReason = response["candidates"]?[0]?["finishReason"]?.GetValue<string>();
        return finishReason is "SAFETY" or "BLOCKLIST" or "PROHIBITED_CONTENT";
    }

    private static void MergeMetadata(
        AiChatMessageMetadataDto target,
        AiChatMessageMetadataDto source)
    {
        foreach (var product in source.Products)
        {
            if (!target.Products.Any(x =>
                    string.Equals(x.Slug, product.Slug, StringComparison.OrdinalIgnoreCase)))
            {
                target.Products.Add(product);
            }
        }

        foreach (var order in source.Orders)
        {
            if (!target.Orders.Any(x => x.SubOrderId == order.SubOrderId))
            {
                target.Orders.Add(order);
            }
        }

        foreach (var item in source.Sources)
        {
            if (!target.Sources.Any(x =>
                    string.Equals(x.Type, item.Type, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Id, item.Id, StringComparison.OrdinalIgnoreCase)))
            {
                target.Sources.Add(item);
            }
        }

        foreach (var action in source.Actions)
        {
            if (!target.Actions.Any(x =>
                    string.Equals(x.Type, action.Type, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Url, action.Url, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.StoreSlug, action.StoreSlug, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.ProductSlug, action.ProductSlug, StringComparison.OrdinalIgnoreCase)))
            {
                target.Actions.Add(action);
            }
        }
    }

    private static string Truncate(string value, int length) =>
        value.Length <= length ? value : value[..length];

    private sealed record GeminiFunctionCall(string Name, string? Id, string ArgumentsJson);

    private static readonly JsonNode ToolDefinitions = JsonNode.Parse(
        """
        [
          {
            "function_declarations": [
              {
                "name": "search_products",
                "description": "Search current GearZone products, prices and stock. Use for recommendations and comparisons.",
                "parameters": {
                  "type": "object",
                  "properties": {
                    "query": { "type": "string", "description": "Optional residual product name, model or brand keywords. Omit broad product-type words when category_slug is enough." },
                    "category_slug": {
                      "type": "string",
                      "description": "Optional category. Supported slugs: keyboards, mechanical-keyboards, membrane-keyboards, keycaps, keyboard-switches, mice, gaming-mice, office-mice, mouse-pads, headsets, gaming-headsets, wireless-headphones, microphones, monitors, gaming-monitors, office-monitors, curved-monitors, pc-components, cpus, gpus, ram, motherboards, storage, power-supplies, pc-cases, gaming-furniture, setup-accessories, console-controllers."
                    },
                    "brand_slugs": {
                      "type": "array",
                      "items": { "type": "string" }
                    },
                    "min_price": { "type": "number" },
                    "max_price": { "type": "number" },
                    "in_stock_only": { "type": "boolean" },
                    "limit": { "type": "integer", "minimum": 1, "maximum": 8 }
                  }
                }
              },
              {
                "name": "get_product_details",
                "description": "Get validated details for one GearZone product.",
                "parameters": {
                  "type": "object",
                  "properties": {
                    "slugs": {
                      "type": "array",
                      "items": { "type": "string" },
                      "minItems": 1,
                      "maxItems": 3
                    }
                  },
                  "required": ["slugs"]
                }
              },
              {
                "name": "search_knowledge",
                "description": "Search published GearZone FAQ and policy knowledge.",
                "parameters": {
                  "type": "object",
                  "properties": {
                    "query": { "type": "string" },
                    "category": { "type": "string" },
                    "limit": { "type": "integer", "minimum": 1, "maximum": 5 }
                  },
                  "required": ["query"]
                }
              },
              {
                "name": "search_my_orders",
                "description": "Search the signed-in customer's own orders. Never use for a guest.",
                "parameters": {
                  "type": "object",
                  "properties": {
                    "search_term": { "type": "string" },
                    "status": { "type": "string" },
                    "limit": { "type": "integer", "minimum": 1, "maximum": 5 }
                  }
                }
              },
              {
                "name": "get_my_order_tracking",
                "description": "Get tracking for one order owned by the signed-in customer.",
                "parameters": {
                  "type": "object",
                  "properties": { "sub_order_id": { "type": "string", "format": "uuid" } },
                  "required": ["sub_order_id"]
                }
              },
              {
                "name": "request_login",
                "description": "Return a login action when personal data is requested by a guest.",
                "parameters": { "type": "object", "properties": {} }
              },
              {
                "name": "suggest_seller_chat",
                "description": "Suggest opening existing seller chat after a store/product context has been validated.",
                "parameters": {
                  "type": "object",
                  "properties": {
                    "store_slug": { "type": "string" },
                    "product_slug": { "type": "string" }
                  },
                  "required": ["store_slug"]
                }
              }
            ]
          }
        ]
        """)!;
}
