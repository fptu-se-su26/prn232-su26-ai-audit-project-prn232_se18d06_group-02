using System.Net.Http.Json;
using GearZone.Application.Common;

namespace GearZone.Web.Services.Api;

/// <summary>Outcome of a write call, mirroring the API's ApiResponse envelope.</summary>
public record ApiResult(bool Success, string? Message, IReadOnlyList<string> Errors)
{
    /// <summary>First error/message suitable for showing to the user.</summary>
    public string? FirstError => Errors.Count > 0 ? Errors[0] : Message;
}

/// <summary>
/// Thin HTTP client the Razor pages use to consume GearZone.Api. It unwraps the
/// standard <see cref="ApiResponse{T}"/> envelope so callers get the payload
/// directly, mirroring what the in-process service returned before the split.
/// </summary>
public interface IApiClient
{
    Task<T?> GetAsync<T>(string path, CancellationToken ct = default);
    Task<byte[]> GetBytesAsync(string path, CancellationToken ct = default);
    Task<ApiResult> PostAsync<TBody>(string path, TBody body, CancellationToken ct = default);
    Task<ApiResult> PostAsync(string path, CancellationToken ct = default);
    /// <summary>POSTs with no body and returns the unwrapped payload (null on failure).</summary>
    Task<TResponse?> PostAndReadAsync<TResponse>(string path, CancellationToken ct = default);
    Task<ApiResult> PutAsync<TBody>(string path, TBody body, CancellationToken ct = default);
    Task<ApiResult> PatchAsync(string path, CancellationToken ct = default);
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
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default; // "not found" → null, matching the in-process services' semantics.
        }
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

    public Task<ApiResult> PostAsync<TBody>(string path, TBody body, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Post, path, body, ct);

    public Task<ApiResult> PostAsync(string path, CancellationToken ct = default) =>
        SendAsync<object?>(HttpMethod.Post, path, null, ct);

    public async Task<TResponse?> PostAndReadAsync<TResponse>(string path, CancellationToken ct = default)
    {
        var response = await _http.PostAsync(path, content: null, ct);
        if (!response.IsSuccessStatusCode) return default;

        var wrapped = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(cancellationToken: ct);
        return wrapped is null ? default : wrapped.Data;
    }

    public Task<ApiResult> PutAsync<TBody>(string path, TBody body, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Put, path, body, ct);

    public Task<ApiResult> PatchAsync(string path, CancellationToken ct = default) =>
        SendAsync<object?>(HttpMethod.Patch, path, null, ct);

    // Write calls must read the body even on 4xx: the API reports failures as
    // ApiResponse.Fail (message + errors) with a non-success status code.
    private async Task<ApiResult> SendAsync<TBody>(HttpMethod method, string path, TBody? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        var response = await _http.SendAsync(request, ct);

        ApiResponse<object>? payload = null;
        try
        {
            payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(cancellationToken: ct);
        }
        catch
        {
            // Non-JSON body (e.g. an error page) — fall back to the status code below.
        }

        if (payload is not null)
        {
            return new ApiResult(payload.Success, payload.Message, payload.Errors?.ToList() ?? new List<string>());
        }

        return new ApiResult(
            response.IsSuccessStatusCode,
            response.IsSuccessStatusCode ? null : $"Request failed ({(int)response.StatusCode}).",
            new List<string>());
    }
}
