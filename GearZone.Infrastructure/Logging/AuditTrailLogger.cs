using Microsoft.Extensions.Logging;

namespace GearZone.Infrastructure.Logging
{
    /// <summary>
    /// Records who did what to which entity and when.
    /// Emits structured log entries consumable by Serilog sinks (file, Seq, etc.).
    /// Register as scoped so UserId context stays per-request.
    /// </summary>
    public interface IAuditTrailLogger
    {
        void LogCreate(string entityType, string entityId, string? userId, string? details = null);
        void LogUpdate(string entityType, string entityId, string? userId, string? details = null);
        void LogDelete(string entityType, string entityId, string? userId, string? details = null);
        void LogAction(string action, string entityType, string entityId, string? userId, string? details = null);
    }

    public sealed class AuditTrailLogger : IAuditTrailLogger
    {
        private readonly ILogger<AuditTrailLogger> _logger;

        public AuditTrailLogger(ILogger<AuditTrailLogger> logger)
        {
            _logger = logger;
        }

        public void LogCreate(string entityType, string entityId, string? userId, string? details = null)
            => LogAction("CREATE", entityType, entityId, userId, details);

        public void LogUpdate(string entityType, string entityId, string? userId, string? details = null)
            => LogAction("UPDATE", entityType, entityId, userId, details);

        public void LogDelete(string entityType, string entityId, string? userId, string? details = null)
            => LogAction("DELETE", entityType, entityId, userId, details);

        public void LogAction(string action, string entityType, string entityId, string? userId, string? details = null)
        {
            _logger.LogInformation(
                "[AUDIT] {Action} | {EntityType} #{EntityId} | User: {UserId} | {OccurredAt:u} | {Details}",
                action,
                entityType,
                entityId,
                userId ?? "SYSTEM",
                DateTime.UtcNow,
                details ?? string.Empty);
        }
    }
}
