using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace GearZone.Infrastructure.Auditing;

public sealed class AdminAuditService : IAdminAuditService
{
    private readonly ApplicationDbContext _db;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    public AdminAuditService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AdminAuditLogListItemDto>> GetLogsAsync(
        AdminAuditQueryDto query,
        CancellationToken ct = default)
    {
        NormalizePaging(query);
        var range = ResolveRange(query);
        var source = ApplyFilters(_db.AdminAuditLogs.AsNoTracking(), query, range);
        var total = await source.CountAsync(ct);
        var items = await source
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new AdminAuditLogListItemDto
            {
                Id = x.Id,
                OccurredAtUtc = x.OccurredAtUtc,
                ActorUserId = x.ActorUserId,
                ActorDisplayName = x.ActorDisplayName ?? x.ActorEmail ?? "System",
                ActorEmail = x.ActorEmail,
                Action = x.Action,
                Module = x.Module,
                Outcome = x.Outcome,
                RiskLevel = x.RiskLevel,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                EntityDisplayName = x.EntityDisplayName,
                Description = x.Description,
                IpAddress = x.IpAddress,
                CorrelationId = x.CorrelationId
            })
            .ToListAsync(ct);

        return new PagedResult<AdminAuditLogListItemDto>(items, total, query.PageNumber, query.PageSize);
    }

    public async Task<AdminAuditDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _db.AdminAuditLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return null;

        return new AdminAuditDetailDto
        {
            Id = item.Id,
            OccurredAtUtc = item.OccurredAtUtc,
            ActorUserId = item.ActorUserId,
            ActorDisplayName = item.ActorDisplayName ?? item.ActorEmail ?? "System",
            ActorEmail = item.ActorEmail,
            Action = item.Action,
            Module = item.Module,
            Outcome = item.Outcome,
            RiskLevel = item.RiskLevel,
            EntityType = item.EntityType,
            EntityId = item.EntityId,
            EntityDisplayName = item.EntityDisplayName,
            Description = item.Description,
            Reason = item.Reason,
            IpAddress = item.IpAddress,
            CorrelationId = item.CorrelationId,
            HttpMethod = item.HttpMethod,
            RequestPath = item.RequestPath,
            StatusCode = item.StatusCode,
            UserAgent = item.UserAgent,
            DurationMs = item.DurationMs,
            Changes = Deserialize<List<AuditChangeDto>>(item.ChangesJson) ?? new(),
            Metadata = Deserialize<Dictionary<string, string?>>(item.MetadataJson)
                ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase),
            TargetUrl = BuildTargetUrl(item.EntityType, item.EntityId, item.EntityDisplayName)
        };
    }

    public async Task<AdminAuditSummaryDto> GetSummaryAsync(
        AdminAuditQueryDto query,
        CancellationToken ct = default)
    {
        var range = ResolveRange(query);
        var source = ApplyFilters(_db.AdminAuditLogs.AsNoTracking(), query, range);

        var total = await source.CountAsync(ct);
        var failed = await source.CountAsync(x => x.Outcome == AdminAuditOutcome.Failed, ct);
        var highRisk = await source.CountAsync(x => x.RiskLevel == AdminAuditRiskLevel.High || x.RiskLevel == AdminAuditRiskLevel.Critical, ct);
        var admins = await source.Where(x => x.ActorUserId != null).Select(x => x.ActorUserId).Distinct().CountAsync(ct);
        var trendRows = await source
            .GroupBy(x => x.OccurredAtUtc.AddHours(7).Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var modules = await source
            .GroupBy(x => x.Module)
            .Select(g => new AdminAuditBreakdownDto { Label = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToListAsync(ct);
        var outcomes = await source
            .GroupBy(x => x.Outcome)
            .Select(g => new { Outcome = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var trendByLocalDate = trendRows
            .GroupBy(x => DateOnly.FromDateTime(x.Date))
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Count));
        var trend = new List<AdminAuditTrendPointDto>();
        for (var date = range.From; date <= range.To; date = date.AddDays(1))
        {
            trend.Add(new AdminAuditTrendPointDto
            {
                Date = date,
                Count = trendByLocalDate.GetValueOrDefault(date)
            });
        }

        return new AdminAuditSummaryDto
        {
            TotalEvents = total,
            FailedActions = failed,
            HighRiskEvents = highRisk,
            ActiveAdmins = admins,
            Trend = trend,
            ModuleBreakdown = modules,
            OutcomeBreakdown = outcomes
                .Select(x => new AdminAuditBreakdownDto { Label = x.Outcome.ToString(), Count = x.Count })
                .OrderByDescending(x => x.Count)
                .ToList()
        };
    }

    public async Task<AdminAuditFilterOptionsDto> GetFilterOptionsAsync(CancellationToken ct = default)
    {
        var source = _db.AdminAuditLogs.AsNoTracking();
        var actors = await source
            .Where(x => x.ActorUserId != null)
            .GroupBy(x => x.ActorUserId!)
            .Select(g => new AdminAuditActorOptionDto
            {
                UserId = g.Key,
                DisplayName = g.OrderByDescending(x => x.OccurredAtUtc)
                    .Select(x => x.ActorDisplayName ?? x.ActorEmail ?? x.ActorUserId!)
                    .First()
            })
            .OrderBy(x => x.DisplayName)
            .ToListAsync(ct);
        var modules = await source.Select(x => x.Module).Distinct().OrderBy(x => x).ToListAsync(ct);
        var actions = await source.Select(x => x.Action).Distinct().OrderBy(x => x).ToListAsync(ct);

        return new AdminAuditFilterOptionsDto
        {
            Actors = actors,
            Modules = modules,
            Actions = actions
        };
    }

    public async Task<AdminAuditFileDto> ExportCsvAsync(AdminAuditQueryDto query, CancellationToken ct = default)
    {
        var range = ResolveRange(query);
        var rows = await ApplyFilters(_db.AdminAuditLogs.AsNoTracking(), query, range)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(x => new
            {
                x.OccurredAtUtc,
                Actor = x.ActorDisplayName ?? x.ActorEmail ?? "System",
                x.ActorEmail,
                x.Action,
                x.Module,
                x.Outcome,
                x.RiskLevel,
                x.EntityType,
                x.EntityId,
                Target = x.EntityDisplayName,
                x.Description,
                x.Reason,
                x.HttpMethod,
                x.RequestPath,
                x.StatusCode,
                x.IpAddress,
                x.CorrelationId
            })
            .ToListAsync(ct);

        var csv = new StringBuilder();
        csv.AppendLine("Occurred At (UTC),Admin,Admin Email,Action,Module,Outcome,Risk,Entity Type,Entity ID,Target,Description,Reason,HTTP Method,Request Path,Status Code,IP Address,Correlation ID");
        foreach (var row in rows)
        {
            AppendCsvRow(csv,
                row.OccurredAtUtc.ToString("O"), row.Actor, row.ActorEmail, row.Action, row.Module,
                row.Outcome.ToString(), row.RiskLevel.ToString(), row.EntityType, row.EntityId, row.Target,
                row.Description, row.Reason, row.HttpMethod, row.RequestPath, row.StatusCode?.ToString(),
                row.IpAddress, row.CorrelationId);
        }

        var body = Encoding.UTF8.GetBytes(csv.ToString());
        var preamble = Encoding.UTF8.GetPreamble();
        var content = new byte[preamble.Length + body.Length];
        Buffer.BlockCopy(preamble, 0, content, 0, preamble.Length);
        Buffer.BlockCopy(body, 0, content, preamble.Length, body.Length);
        return new AdminAuditFileDto(
            content,
            "text/csv; charset=utf-8",
            $"admin-audit-logs-{range.From:yyyy-MM-dd}-{range.To:yyyy-MM-dd}.csv");
    }

    private static IQueryable<AdminAuditLog> ApplyFilters(
        IQueryable<AdminAuditLog> source,
        AdminAuditQueryDto query,
        AuditRange range)
    {
        source = source.Where(x => x.OccurredAtUtc >= range.StartUtc && x.OccurredAtUtc < range.EndExclusiveUtc);
        if (!string.IsNullOrWhiteSpace(query.ActorUserId)) source = source.Where(x => x.ActorUserId == query.ActorUserId);
        if (!string.IsNullOrWhiteSpace(query.Module)) source = source.Where(x => x.Module == query.Module);
        if (!string.IsNullOrWhiteSpace(query.Action)) source = source.Where(x => x.Action == query.Action);
        if (query.Outcome.HasValue) source = source.Where(x => x.Outcome == query.Outcome.Value);
        if (query.RiskLevel.HasValue) source = source.Where(x => x.RiskLevel == query.RiskLevel.Value);
        if (!string.IsNullOrWhiteSpace(query.EntityType)) source = source.Where(x => x.EntityType == query.EntityType);
        if (!string.IsNullOrWhiteSpace(query.EntityId)) source = source.Where(x => x.EntityId == query.EntityId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.Like(x.Action, term) ||
                EF.Functions.Like(x.Module, term) ||
                (x.EntityId != null && EF.Functions.Like(x.EntityId, term)) ||
                (x.EntityDisplayName != null && EF.Functions.Like(x.EntityDisplayName, term)) ||
                (x.Description != null && EF.Functions.Like(x.Description, term)) ||
                (x.CorrelationId != null && EF.Functions.Like(x.CorrelationId, term)));
        }
        return source;
    }

    private static AuditRange ResolveRange(AdminAuditQueryDto query)
    {
        if (query.From.HasValue != query.To.HasValue)
            throw new ArgumentException("Both from and to are required for a custom date range.");

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone));
        var from = query.From ?? today.AddDays(-6);
        var to = query.To ?? today;
        if (from > to) throw new ArgumentException("From date must be on or before to date.");
        if (to.DayNumber - from.DayNumber + 1 > 366)
            throw new ArgumentException("Audit date range cannot exceed 366 days.");

        var startLocal = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        var endLocal = DateTime.SpecifyKind(to.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        return new AuditRange(
            from,
            to,
            TimeZoneInfo.ConvertTimeToUtc(startLocal, VietnamTimeZone),
            TimeZoneInfo.ConvertTimeToUtc(endLocal, VietnamTimeZone));
    }

    private static void NormalizePaging(AdminAuditQueryDto query)
    {
        query.PageNumber = Math.Max(query.PageNumber, 1);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);
    }

    private static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
        catch (JsonException) { return default; }
    }

    private static string? BuildTargetUrl(string? entityType, string? entityId, string? entityDisplayName)
    {
        if (string.IsNullOrWhiteSpace(entityId)) return null;
        return entityType switch
        {
            "ApplicationUser" => string.IsNullOrWhiteSpace(entityDisplayName)
                ? "/admin/users"
                : $"/admin/users?Query.SearchTerm={Uri.EscapeDataString(entityDisplayName)}",
            "Store" => Guid.TryParse(entityId, out _) ? $"/admin/stores/{entityId}" : null,
            "Product" => Guid.TryParse(entityId, out _) ? $"/admin/products/{entityId}" : null,
            "PayoutBatch" => Guid.TryParse(entityId, out _) ? $"/Admin/PayoutBatches/Details/{entityId}" : null,
            "PayoutTransaction" => Guid.TryParse(entityId, out _) ? $"/admin/payouts/transactions/{entityId}" : null,
            _ => null
        };
    }

    private static void AppendCsvRow(StringBuilder builder, params string?[] values) =>
        builder.AppendLine(string.Join(",", values.Select(EscapeCsv)));

    private static string EscapeCsv(string? value)
    {
        var safe = value ?? string.Empty;
        return safe.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{safe.Replace("\"", "\"\"")}\""
            : safe;
    }

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        foreach (var id in new[] { "Asia/Ho_Chi_Minh", "SE Asia Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
        }
        return TimeZoneInfo.CreateCustomTimeZone("Vietnam", TimeSpan.FromHours(7), "Vietnam", "Vietnam");
    }

    private sealed record AuditRange(DateOnly From, DateOnly To, DateTime StartUtc, DateTime EndExclusiveUtc);
}
