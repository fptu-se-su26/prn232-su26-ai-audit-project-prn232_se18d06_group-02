using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Entities;
using System.Text.Json;

namespace GearZone.Infrastructure.Auditing;

internal static class AdminAuditMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static AdminAuditLog ToEntity(AdminAuditEvent source, AdminAuditSanitizer sanitizer) => new()
    {
        Id = Guid.NewGuid(),
        OccurredAtUtc = source.OccurredAtUtc.Kind == DateTimeKind.Utc
            ? source.OccurredAtUtc
            : source.OccurredAtUtc.ToUniversalTime(),
        ActorUserId = Trim(source.ActorUserId, 450),
        ActorDisplayName = Trim(source.ActorDisplayName, 200),
        ActorEmail = Trim(source.ActorEmail, 256),
        Action = Trim(source.Action, 100) ?? "UNKNOWN",
        Module = Trim(source.Module, 50) ?? "System",
        Outcome = source.Outcome,
        RiskLevel = source.RiskLevel,
        EntityType = Trim(source.EntityType, 100),
        EntityId = Trim(source.EntityId, 100),
        EntityDisplayName = sanitizer.SanitizeFreeText(source.EntityDisplayName, 300),
        Description = sanitizer.SanitizeFreeText(source.Description),
        Reason = sanitizer.SanitizeFreeText(source.Reason),
        ChangesJson = source.Changes.Count == 0 ? null : JsonSerializer.Serialize(source.Changes, JsonOptions),
        MetadataJson = source.Metadata.Count == 0 ? null : JsonSerializer.Serialize(
            source.Metadata.ToDictionary(x => Trim(x.Key, 100) ?? "metadata", x => sanitizer.SanitizeFreeText(x.Value, 500)),
            JsonOptions),
        HttpMethod = Trim(source.HttpMethod, 10),
        RequestPath = sanitizer.SanitizeFreeText(source.RequestPath),
        StatusCode = source.StatusCode,
        IpAddress = Trim(source.IpAddress, 64),
        UserAgent = sanitizer.SanitizeFreeText(source.UserAgent),
        CorrelationId = Trim(source.CorrelationId, 128),
        DurationMs = source.DurationMs
    };

    private static string? Trim(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
