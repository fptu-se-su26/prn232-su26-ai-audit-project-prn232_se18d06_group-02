using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.External;

public sealed class AiInsightProviderRequest
{
    public string ReportType { get; init; } = string.Empty;
    public string Prompt { get; init; } = string.Empty;
    public string SnapshotJson { get; init; } = string.Empty;
    public IReadOnlySet<string> AllowedMetricKeys { get; init; } = new HashSet<string>();
}

public interface IAiInsightProvider
{
    string Name { get; }
    string Model { get; }
    Task<AdminAiInsightDto> GenerateAsync(AiInsightProviderRequest request, CancellationToken ct = default);
}

public interface IAiInsightProviderResolver
{
    IAiInsightProvider Resolve();
    string ProviderName { get; }
    string Model { get; }
    bool IsEnabled { get; }
}

public sealed class AiInsightUnavailableException : Exception
{
    public AiInsightUnavailableException(string message) : base(message) { }
    public AiInsightUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
