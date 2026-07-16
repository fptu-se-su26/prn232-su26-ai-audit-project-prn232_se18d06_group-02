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
        var cookie = _httpContextAccessor.HttpContext?.Request.Headers.Cookie.ToString();
        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.Remove("Cookie");
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
