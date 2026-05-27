using GearZone.Application.Abstractions.Logging;
using Microsoft.Extensions.Logging;

namespace GearZone.Infrastructure.Logging
{
    /// <summary>
    /// Bridges IAppLogger&lt;T&gt; to the built-in ILogger&lt;T&gt; / Serilog sink.
    /// Register via DI: services.AddScoped(typeof(IAppLogger&lt;&gt;), typeof(SerilogAppLogger&lt;&gt;))
    /// </summary>
    public sealed class SerilogAppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;

        public SerilogAppLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message, params object[] args)
            => _logger.LogInformation(message, args);

        public void LogWarning(string message, params object[] args)
            => _logger.LogWarning(message, args);

        public void LogError(Exception exception, string message, params object[] args)
            => _logger.LogError(exception, message, args);

        public void LogDebug(string message, params object[] args)
            => _logger.LogDebug(message, args);

        public void LogCritical(Exception exception, string message, params object[] args)
            => _logger.LogCritical(exception, message, args);
    }
}
