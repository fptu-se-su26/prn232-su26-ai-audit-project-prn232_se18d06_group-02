using GearZone.Application.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GearZone.Infrastructure.Auditing;

public sealed class AdminAuditRecorder : IAdminAuditRecorder
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AdminAuditSanitizer _sanitizer;
    private readonly ILogger<AdminAuditRecorder> _logger;

    public AdminAuditRecorder(
        IServiceScopeFactory scopeFactory,
        AdminAuditSanitizer sanitizer,
        ILogger<AdminAuditRecorder> logger)
    {
        _scopeFactory = scopeFactory;
        _sanitizer = sanitizer;
        _logger = logger;
    }

    public async Task RecordAsync(AdminAuditEvent auditEvent, CancellationToken ct = default)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var auditContext = scope.ServiceProvider.GetRequiredService<AdminAuditContext>();
            using (auditContext.Suppress())
            {
                db.AdminAuditLogs.Add(AdminAuditMapper.ToEntity(auditEvent, _sanitizer));
                await db.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[AUDIT-FALLBACK] {Action} {Module} {Outcome} Actor={ActorUserId} CorrelationId={CorrelationId}",
                auditEvent.Action,
                auditEvent.Module,
                auditEvent.Outcome,
                auditEvent.ActorUserId ?? "SYSTEM",
                auditEvent.CorrelationId);
        }
    }
}
