using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using GearZone.Infrastructure;
using GearZone.Infrastructure.Auditing;
using GearZone.Api.Auditing;
using GearZone.Api.Controllers.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GearZone.Tests;

public sealed class AdminAuditTests
{
    [Fact]
    public async Task Service_FiltersSummarizesZeroFillsAndExportsEscapedCsv()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var firstId = Guid.NewGuid();
        db.AdminAuditLogs.AddRange(
            Log(firstId, new DateTime(2026, 7, 17, 17, 30, 0, DateTimeKind.Utc), "admin-1", "Finance", AdminAuditOutcome.Succeeded, AdminAuditRiskLevel.Critical,
                description: "Export, \"quoted\"", correlationId: "corr-1"),
            Log(Guid.NewGuid(), new DateTime(2026, 7, 18, 17, 30, 0, DateTimeKind.Utc), "admin-1", "Stores", AdminAuditOutcome.Failed, AdminAuditRiskLevel.High),
            Log(Guid.NewGuid(), new DateTime(2026, 7, 18, 2, 0, 0, DateTimeKind.Utc), "admin-2", "Reports", AdminAuditOutcome.Succeeded, AdminAuditRiskLevel.Low),
            Log(Guid.NewGuid(), new DateTime(2026, 7, 16, 16, 59, 0, DateTimeKind.Utc), "admin-3", "Users", AdminAuditOutcome.Succeeded, AdminAuditRiskLevel.Medium));
        await db.SaveChangesAsync();

        var service = new AdminAuditService(db);
        var query = new AdminAuditQueryDto
        {
            From = new DateOnly(2026, 7, 18),
            To = new DateOnly(2026, 7, 20),
            PageSize = 25
        };

        var logs = await service.GetLogsAsync(query);
        Assert.Equal(3, logs.TotalCount);
        Assert.Equal(3, logs.Items.Count);
        Assert.True(logs.Items[0].OccurredAtUtc >= logs.Items[1].OccurredAtUtc);

        var filtered = await service.GetLogsAsync(new AdminAuditQueryDto
        {
            From = query.From,
            To = query.To,
            Search = "corr-1",
            Module = "Finance",
            RiskLevel = AdminAuditRiskLevel.Critical,
            PageSize = 500
        });
        Assert.Single(filtered.Items);
        Assert.Equal(100, filtered.PageSize);

        var summary = await service.GetSummaryAsync(query);
        Assert.Equal(3, summary.TotalEvents);
        Assert.Equal(1, summary.FailedActions);
        Assert.Equal(2, summary.HighRiskEvents);
        Assert.Equal(2, summary.ActiveAdmins);
        Assert.Equal(3, summary.Trend.Count);
        Assert.Equal(0, summary.Trend[^1].Count);

        var optionsResult = await service.GetFilterOptionsAsync();
        Assert.Equal(3, optionsResult.Actors.Count);
        Assert.Contains("Finance", optionsResult.Modules);

        var detail = await service.GetDetailAsync(firstId);
        Assert.NotNull(detail);
        Assert.Equal("corr-1", detail!.CorrelationId);

        var file = await service.ExportCsvAsync(query);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, file.Content[..3]);
        var csv = Encoding.UTF8.GetString(file.Content[3..]);
        Assert.Contains("\"Export, \"\"quoted\"\"\"", csv);
        Assert.EndsWith(".csv", file.FileName);
    }

    [Fact]
    public async Task SaveChangesInterceptor_WritesOneAuditAndRedactsSensitiveSetting()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var auditContext = new AdminAuditContext();
        var sanitizer = new AdminAuditSanitizer();
        var interceptor = new AuditSaveChangesInterceptor(auditContext, sanitizer);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(interceptor)
            .Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        using (auditContext.Begin(Event("BRAND_CREATED", "Catalog", "corr-brand", "Brand")))
        {
            db.Brands.Add(new Brand { Name = "Audit Brand", Slug = "audit-brand", IsApproved = true });
            await db.SaveChangesAsync();
        }

        var setting = new SystemSetting
        {
            Id = Guid.NewGuid(),
            Key = "PAYOS_API_KEY",
            Value = "old-secret",
            GroupName = "Payments",
            Description = "Sensitive integration setting"
        };
        db.SystemSettings.Add(setting);
        await db.SaveChangesAsync();

        using (auditContext.Begin(Event("PLATFORM_SETTINGS_UPDATED", "Settings", "corr-setting", "SystemSetting")))
        {
            setting.Value = "new-secret";
            setting.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        db.ChangeTracker.Clear();
        var rows = await db.AdminAuditLogs.OrderBy(x => x.OccurredAtUtc).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal("Audit Brand", rows[0].EntityDisplayName);
        Assert.NotNull(rows[0].DurationMs);
        Assert.Contains("Audit Brand", rows[0].ChangesJson);
        Assert.DoesNotContain("old-secret", rows[1].ChangesJson);
        Assert.DoesNotContain("new-secret", rows[1].ChangesJson);
        Assert.Contains("[REDACTED]", rows[1].ChangesJson);
    }

    [Fact]
    public void Sanitizer_RemovesSecretsAndMasksLongAccountNumbers()
    {
        var sanitizer = new AdminAuditSanitizer();
        var value = sanitizer.SanitizeFreeText("api_key=abc123 bank=1234567890123456 backup=1234-5678-9012 password=hunter2");

        Assert.NotNull(value);
        Assert.DoesNotContain("abc123", value);
        Assert.DoesNotContain("hunter2", value);
        Assert.DoesNotContain("1234567890123456", value);
        Assert.DoesNotContain("1234-5678-9012", value);
        Assert.Contains("12***56", value);
        Assert.Contains("12***12", value);
    }

    [Fact]
    public async Task Service_RejectsInvalidOrOversizedRanges()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var service = new AdminAuditService(db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetLogsAsync(new AdminAuditQueryDto
        {
            From = new DateOnly(2026, 7, 18)
        }));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetSummaryAsync(new AdminAuditQueryDto
        {
            From = new DateOnly(2025, 1, 1),
            To = new DateOnly(2026, 7, 18)
        }));
    }

    [Fact]
    public void AuditController_IsSuperAdminReadOnlyAndAuditsCsvExport()
    {
        var authorize = Assert.Single(typeof(AuditLogsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>());
        Assert.Equal("Super Admin", authorize.Roles);

        var methods = typeof(AuditLogsController).GetMethods()
            .Where(x => x.DeclaringType == typeof(AuditLogsController))
            .ToList();
        Assert.DoesNotContain(methods, x =>
            x.IsDefined(typeof(HttpPostAttribute), true) ||
            x.IsDefined(typeof(HttpPutAttribute), true) ||
            x.IsDefined(typeof(HttpPatchAttribute), true) ||
            x.IsDefined(typeof(HttpDeleteAttribute), true));

        var export = typeof(AuditLogsController).GetMethod(nameof(AuditLogsController.Export));
        var audit = Assert.Single(export!.GetCustomAttributes(typeof(AdminAuditActionAttribute), true)
            .Cast<AdminAuditActionAttribute>());
        Assert.Equal("AUDIT_LOG_EXPORTED", audit.Action);
        Assert.Equal(AdminAuditRiskLevel.Medium, audit.RiskLevel);
    }

    private static AdminAuditEvent Event(string action, string module, string correlationId, string entityType) => new()
    {
        OccurredAtUtc = DateTime.UtcNow,
        ActorUserId = "admin-1",
        ActorDisplayName = "Super Admin",
        ActorEmail = "admin@test.local",
        Action = action,
        Module = module,
        Outcome = AdminAuditOutcome.Succeeded,
        RiskLevel = AdminAuditRiskLevel.Medium,
        EntityType = entityType,
        CorrelationId = correlationId
    };

    private static AdminAuditLog Log(
        Guid id,
        DateTime occurredAtUtc,
        string actorId,
        string module,
        AdminAuditOutcome outcome,
        AdminAuditRiskLevel risk,
        string? description = null,
        string? correlationId = null) => new()
    {
        Id = id,
        OccurredAtUtc = occurredAtUtc,
        ActorUserId = actorId,
        ActorDisplayName = actorId,
        ActorEmail = $"{actorId}@test.local",
        Action = "TEST_ACTION",
        Module = module,
        Outcome = outcome,
        RiskLevel = risk,
        EntityType = "Store",
        EntityId = Guid.NewGuid().ToString(),
        EntityDisplayName = "Test Store",
        Description = description,
        CorrelationId = correlationId
    };
}
