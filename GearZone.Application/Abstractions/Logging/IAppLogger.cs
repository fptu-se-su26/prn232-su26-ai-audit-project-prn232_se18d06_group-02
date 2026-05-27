namespace GearZone.Application.Abstractions.Logging
{
    /// <summary>
    /// Domain-agnostic logger abstraction. Inject this instead of ILogger&lt;T&gt;
    /// so application layer stays free of infrastructure concerns.
    /// </summary>
    public interface IAppLogger<T>
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(Exception exception, string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogCritical(Exception exception, string message, params object[] args);
    }
}
