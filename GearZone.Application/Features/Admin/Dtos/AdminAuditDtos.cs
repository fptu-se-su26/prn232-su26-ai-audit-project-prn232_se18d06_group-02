using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Admin.Dtos;

public sealed class AdminAuditQueryDto
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string? Search { get; set; }
    public string? ActorUserId { get; set; }
    public string? Module { get; set; }
    public string? Action { get; set; }
    public AdminAuditOutcome? Outcome { get; set; }
    public AdminAuditRiskLevel? RiskLevel { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class AdminAuditLogListItemDto
{
    public Guid Id { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string? ActorUserId { get; set; }
    public string ActorDisplayName { get; set; } = "System";
    public string? ActorEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public AdminAuditOutcome Outcome { get; set; }
    public AdminAuditRiskLevel RiskLevel { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? EntityDisplayName { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
}

public sealed class AdminAuditDetailDto : AdminAuditLogListItemDto
{
    public string? Reason { get; set; }
    public List<AuditChangeDto> Changes { get; set; } = new();
    public Dictionary<string, string?> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string? HttpMethod { get; set; }
    public string? RequestPath { get; set; }
    public int? StatusCode { get; set; }
    public string? UserAgent { get; set; }
    public long? DurationMs { get; set; }
    public string? TargetUrl { get; set; }
}

public sealed class AuditChangeDto
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public sealed class AdminAuditSummaryDto
{
    public int TotalEvents { get; set; }
    public int FailedActions { get; set; }
    public int HighRiskEvents { get; set; }
    public int ActiveAdmins { get; set; }
    public List<AdminAuditTrendPointDto> Trend { get; set; } = new();
    public List<AdminAuditBreakdownDto> ModuleBreakdown { get; set; } = new();
    public List<AdminAuditBreakdownDto> OutcomeBreakdown { get; set; } = new();
}

public sealed class AdminAuditTrendPointDto
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public sealed class AdminAuditBreakdownDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class AdminAuditFilterOptionsDto
{
    public List<AdminAuditActorOptionDto> Actors { get; set; } = new();
    public List<string> Modules { get; set; } = new();
    public List<string> Actions { get; set; } = new();
}

public sealed class AdminAuditActorOptionDto
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class AdminAuditListResponseDto
{
    public PagedResult<AdminAuditLogListItemDto> Logs { get; set; } = new();
}

public sealed record AdminAuditFileDto(byte[] Content, string ContentType, string FileName);
