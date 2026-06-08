using System.Diagnostics;

namespace GearZone.Web.Middleware
{
    /// <summary>
    /// Logs every HTTP request: method, path, response code, and elapsed time.
    /// Catches unhandled exceptions before they bubble out of the middleware pipeline.
    /// Register with app.UseRequestLogging() before UseRouting().
    /// </summary>
    public sealed class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await _next(context);
                sw.Stop();

                var level = context.Response.StatusCode >= 500
                    ? LogLevel.Error
                    : context.Response.StatusCode >= 400
                        ? LogLevel.Warning
                        : LogLevel.Information;

                _logger.Log(level,
                    "[HTTP] {Method} {Path} => {StatusCode} ({ElapsedMs}ms)",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex,
                    "[HTTP] {Method} {Path} => UNHANDLED EXCEPTION after {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds);
                throw;
            }
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
            => app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
