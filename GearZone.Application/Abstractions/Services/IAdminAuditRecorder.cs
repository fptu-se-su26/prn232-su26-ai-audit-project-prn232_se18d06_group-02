using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;

namespace GearZone.Application.Abstractions.Services;

public interface IAdminAuditRecorder
{
    Task RecordAsync(AdminAuditEvent auditEvent, CancellationToken ct = default);
}

public sealed class AdminAuditEvent
{
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string? ActorUserId { get; set; }
    public string? ActorDisplayName { get; set; }
    public string? ActorEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public AdminAuditOutcome Outcome { get; set; }
    public AdminAuditRiskLevel RiskLevel { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? EntityDisplayName { get; set; }
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public List<AuditChangeDto> Changes { get; set; } = new();
    public Dictionary<string, string?> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string? HttpMethod { get; set; }
    public string? RequestPath { get; set; }
    public int? StatusCode { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public long? DurationMs { get; set; }
}
