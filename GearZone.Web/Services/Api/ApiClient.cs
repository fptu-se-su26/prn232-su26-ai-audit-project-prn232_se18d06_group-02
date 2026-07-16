using System.Net.Http.Json;
using GearZone.Application.Common;

namespace GearZone.Web.Services.Api;

/// <summary>
/// Thin HTTP client the Razor pages use to consume GearZone.Api. It unwraps the
/// standard <see cref="ApiResponse{T}"/> envelope so callers get the payload
/// directly, mirroring what the in-process service returned before the split.
/// </summary>
public interface IApiClient
{
    Task<T?> GetAsync<T>(string path, CancellationToken ct = default);
    Task<byte[]> GetBytesAsync(string path, CancellationToken ct = default);
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
    {
        var response = await _http.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();

        var wrapped = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct);
        return wrapped is null ? default : wrapped.Data;
    }

    public async Task<byte[]> GetBytesAsync(string path, CancellationToken ct = default)
    {
        var response = await _http.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(ct);
    }
}
