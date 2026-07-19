using GearZone.Application.Abstractions.Services;

namespace GearZone.Infrastructure.Auditing;

public sealed class AdminAuditContext
{
    public AdminAuditOperation? Current { get; private set; }
    public bool IsSuppressed { get; private set; }

    public IDisposable Begin(AdminAuditEvent auditEvent)
    {
        var previous = Current;
        Current = new AdminAuditOperation(auditEvent);
        return new CallbackScope(() => Current = previous);
    }

    public IDisposable Suppress()
    {
        var previous = IsSuppressed;
        IsSuppressed = true;
        return new CallbackScope(() => IsSuppressed = previous);
    }

    private sealed class CallbackScope(Action onDispose) : IDisposable
    {
        private Action? _onDispose = onDispose;
        public void Dispose() => Interlocked.Exchange(ref _onDispose, null)?.Invoke();
    }
}

public sealed class AdminAuditOperation(AdminAuditEvent auditEvent)
{
    public AdminAuditEvent Event { get; } = auditEvent;
    public bool WasPersisted { get; set; }
}
