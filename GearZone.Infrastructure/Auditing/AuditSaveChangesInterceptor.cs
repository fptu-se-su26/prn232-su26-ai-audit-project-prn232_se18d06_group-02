using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GearZone.Infrastructure.Auditing;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly AdminAuditContext _auditContext;
    private readonly AdminAuditSanitizer _sanitizer;

    public AuditSaveChangesInterceptor(AdminAuditContext auditContext, AdminAuditSanitizer sanitizer)
    {
        _auditContext = auditContext;
        _sanitizer = sanitizer;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AppendAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AppendAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AppendAudit(DbContext? db)
    {
        var operation = _auditContext.Current;
        if (db is null || operation is null || operation.WasPersisted || _auditContext.IsSuppressed)
            return;

        var changedEntries = db.ChangeTracker.Entries()
            .Where(x => x.Entity is not AdminAuditLog &&
                        x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
        if (changedEntries.Count == 0) return;

        var changes = _sanitizer.CaptureChanges(changedEntries);
        if (changes.Count == 0) return;

        var source = operation.Event;
        if (source.Outcome != AdminAuditOutcome.Queued)
            source.Outcome = AdminAuditOutcome.Succeeded;
        source.StatusCode ??= 200;
        source.DurationMs ??= Math.Max(
            0,
            (long)(DateTime.UtcNow - source.OccurredAtUtc.ToUniversalTime()).TotalMilliseconds);
        source.Changes = changes;

        var first = changedEntries.FirstOrDefault(x =>
            string.Equals(x.Metadata.ClrType.Name, source.EntityType, StringComparison.OrdinalIgnoreCase))
            ?? changedEntries[0];
        source.EntityType ??= first.Metadata.ClrType.Name;
        source.EntityId ??= _sanitizer.GetEntityId(first);
        source.EntityDisplayName ??= _sanitizer.GetEntityDisplayName(first);

        db.Set<AdminAuditLog>().Add(AdminAuditMapper.ToEntity(source, _sanitizer));
        operation.WasPersisted = true;
    }
}
