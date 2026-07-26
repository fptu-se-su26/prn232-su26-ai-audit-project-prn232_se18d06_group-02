using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GearZone.Infrastructure.External;

public sealed class AiInsightProviderResolver : IAiInsightProviderResolver
{
    private readonly IReadOnlyDictionary<string, IAiInsightProvider> _providers;
    private readonly AiInsightSettings _settings;

    public AiInsightProviderResolver(IEnumerable<IAiInsightProvider> providers, IOptions<AiInsightSettings> options)
    {
        _settings = options.Value;
        _providers = providers.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    public string ProviderName => string.IsNullOrWhiteSpace(_settings.Provider) ? "OpenAI" : _settings.Provider.Trim();
    public string Model => _providers.TryGetValue(ProviderName, out var provider) ? provider.Model : string.Empty;
    public bool IsEnabled => _settings.Enabled;

    public IAiInsightProvider Resolve()
    {
        if (!IsEnabled)
            throw new AiInsightUnavailableException("AI insights are disabled.");
        if (!_providers.TryGetValue(ProviderName, out var provider))
            throw new AiInsightUnavailableException($"Unsupported AI provider '{ProviderName}'. Use OpenAI or Gemini.");
        return provider;
    }
}

public abstract class AiInsightProviderBase
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    protected static object OutputSchema => new
    {
        type = "object",
        additionalProperties = false,
        properties = new Dictionary<string, object>
        {
            ["summary"] = new { type = "string" },
            ["highlights"] = InsightItemsSchema(),
            ["risks"] = InsightItemsSchema(),
            ["recommendations"] = new
            {
                type = "array",
                maxItems = 3,
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new Dictionary<string, object>
                    {
                        ["title"] = new { type = "string" },
                        ["action"] = new { type = "string" },
                        ["priority"] = new { type = "string", @enum = new[] { "low", "medium", "high" } },
                        ["metricKeys"] = new { type = "array", items = new { type = "string" }, minItems = 1 }
                    },
                    required = new[] { "title", "action", "priority", "metricKeys" }
                }
            }
        },
        required = new[] { "summary", "highlights", "risks", "recommendations" }
    };

    private static object InsightItemsSchema() => new
    {
        type = "array",
        maxItems = 3,
        items = new
        {
            type = "object",
            additionalProperties = false,
            properties = new Dictionary<string, object>
            {
                ["title"] = new { type = "string" },
                ["explanation"] = new { type = "string" },
                ["severity"] = new { type = "string", @enum = new[] { "info", "warning", "critical" } },
                ["metricKeys"] = new { type = "array", items = new { type = "string" }, minItems = 1 }
            },
            required = new[] { "title", "explanation", "severity", "metricKeys" }
        }
    };

    protected static AdminAiInsightDto ParseInsight(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<AdminAiInsightDto>(json, JsonOptions)
                ?? throw new JsonException("Empty structured output.");
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            throw new AiInsightUnavailableException("The AI provider returned invalid structured output.", ex);
        }
    }
}

public sealed class OpenAiInsightProvider : AiInsightProviderBase, IAiInsightProvider
{
    private readonly HttpClient _http;
    private readonly AiInsightSettings _settings;
    private readonly ILogger<OpenAiInsightProvider> _logger;

    public OpenAiInsightProvider(
        HttpClient http,
        IOptions<AiInsightSettings> options,
        ILogger<OpenAiInsightProvider> logger)
    {
        _http = http;
        _settings = options.Value;
        _logger = logger;
    }

    public string Name => "OpenAI";
    public string Model => _settings.OpenAiModel;

    public async Task<AdminAiInsightDto> GenerateAsync(AiInsightProviderRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenAiApiKey))
            throw new AiInsightUnavailableException("OPENAI_API_KEY is not configured.");

        using var message = new HttpRequestMessage(HttpMethod.Post, "responses");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.OpenAiApiKey);
        message.Content = JsonContent.Create(new
        {
            model = Model,
            instructions = request.Prompt,
            input = $"Allowed metric keys: {string.Join(", ", request.AllowedMetricKeys)}\nAggregate snapshot JSON:\n{request.SnapshotJson}",
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "gearzone_admin_bi_insight",
                    strict = true,
                    schema = OutputSchema
                }
            },
            max_output_tokens = 1200
        }, options: JsonOptions);

        using var response = await _http.SendAsync(message, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI insight request failed with status {StatusCode}.", (int)response.StatusCode);
            throw new AiInsightUnavailableException($"OpenAI returned HTTP {(int)response.StatusCode}.");
        }

        using var document = JsonDocument.Parse(raw);
        var outputText = document.RootElement.TryGetProperty("output_text", out var direct)
            ? direct.GetString()
            : FindOutputText(document.RootElement);
        if (string.IsNullOrWhiteSpace(outputText))
            throw new AiInsightUnavailableException("OpenAI returned no output text.");
        return ParseInsight(outputText);
    }

    private static string? FindOutputText(JsonElement root)
    {
        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
            return null;
        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                continue;
            foreach (var block in content.EnumerateArray())
            {
                if (block.TryGetProperty("type", out var type) && type.GetString() == "output_text" &&
                    block.TryGetProperty("text", out var text))
                    return text.GetString();
            }
        }
        return null;
    }
}

public sealed class GeminiInsightProvider : AiInsightProviderBase, IAiInsightProvider
{
    private readonly HttpClient _http;
    private readonly AiInsightSettings _settings;
    private readonly ILogger<GeminiInsightProvider> _logger;

    public GeminiInsightProvider(
        HttpClient http,
        IOptions<AiInsightSettings> options,
        ILogger<GeminiInsightProvider> logger)
    {
        _http = http;
        _settings = options.Value;
        _logger = logger;
    }

    public string Name => "Gemini";
    public string Model => _settings.GeminiModel;

    public async Task<AdminAiInsightDto> GenerateAsync(AiInsightProviderRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.GeminiApiKey))
            throw new AiInsightUnavailableException("GEMINI_API_KEY is not configured.");

        using var message = new HttpRequestMessage(HttpMethod.Post, $"models/{Uri.EscapeDataString(Model)}:generateContent");
        message.Headers.Add("x-goog-api-key", _settings.GeminiApiKey);
        message.Content = JsonContent.Create(new
        {
            systemInstruction = new { parts = new[] { new { text = request.Prompt } } },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = $"Allowed metric keys: {string.Join(", ", request.AllowedMetricKeys)}\nAggregate snapshot JSON:\n{request.SnapshotJson}" }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseJsonSchema = OutputSchema,
                maxOutputTokens = 1200,
                temperature = 0.2
            }
        }, options: JsonOptions);

        using var response = await _http.SendAsync(message, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini insight request failed with status {StatusCode}.", (int)response.StatusCode);
            throw new AiInsightUnavailableException($"Gemini returned HTTP {(int)response.StatusCode}.");
        }

        using var document = JsonDocument.Parse(raw);
        var text = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();
        if (string.IsNullOrWhiteSpace(text))
            throw new AiInsightUnavailableException("Gemini returned no output text.");
        return ParseInsight(text);
    }
}
