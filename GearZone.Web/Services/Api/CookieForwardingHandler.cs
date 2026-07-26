using Microsoft.AspNetCore.Http;

namespace GearZone.Web.Services.Api;

/// <summary>
/// Forwards the incoming browser request's cookies onto the outgoing call to
/// GearZone.Api. This is a server-to-server call, so no browser SameSite/CORS
/// rules apply — the API validates the same Identity auth cookie via the shared
/// Data Protection key ring.
/// </summary>
public class CookieForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        var cookie = context?.Request.Headers.Cookie.ToString();
        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.Remove("Cookie");
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        var clientIp = context?.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrWhiteSpace(clientIp))
        {
            request.Headers.Remove("X-Forwarded-For");
            request.Headers.TryAddWithoutValidation("X-Forwarded-For", clientIp);
        }

        var userAgent = context?.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrWhiteSpace(userAgent))
        {
            request.Headers.Remove("User-Agent");
            request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
        }

        var correlationId = context?.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId) || correlationId.Length > 128 ||
            !correlationId.All(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.'))
        {
            correlationId = context?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
        }
        request.Headers.Remove("X-Correlation-ID");
        request.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId);

        return base.SendAsync(request, cancellationToken);
    }
}
