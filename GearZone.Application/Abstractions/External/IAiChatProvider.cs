using GearZone.Application.Features.AiChat.Dtos;

namespace GearZone.Application.Abstractions.External;

public sealed class AiChatProviderRequest
{
    public string SystemInstruction { get; init; } = string.Empty;
    public string UserMessage { get; init; } = string.Empty;
    public IReadOnlyList<AiChatProviderHistoryItem> History { get; init; } = Array.Empty<AiChatProviderHistoryItem>();
    public AiChatActor Actor { get; init; } = new(null, null);
    public AiChatPageContextDto? PageContext { get; init; }
}

public sealed record AiChatProviderHistoryItem(string Role, string Content);

public sealed class AiChatProviderResult
{
    public string Text { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public AiChatMessageMetadataDto Metadata { get; set; } = new();
    public bool WasBlocked { get; set; }
}

public sealed class AiToolExecutionResult
{
    public string Json { get; set; } = "{}";
    public AiChatMessageMetadataDto Metadata { get; set; } = new();
}

public interface IAiChatProvider
{
    string Model { get; }
    bool IsEnabled { get; }

    Task<AiChatProviderResult> GenerateAsync(
        AiChatProviderRequest request,
        Func<string, string, CancellationToken, Task<AiToolExecutionResult>> executeTool,
        Func<string, CancellationToken, Task>? onToolStarted = null,
        CancellationToken ct = default);
}

public sealed class AiChatUnavailableException : Exception
{
    public AiChatUnavailableException(string message) : base(message) { }
    public AiChatUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
