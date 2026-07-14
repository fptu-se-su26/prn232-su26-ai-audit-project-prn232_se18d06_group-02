namespace GearZone.Application.Common;

/// <summary>
/// Standard envelope wrapping every API response as { success, data, message, errors }.
/// Lives in the Application layer so both the API host and any HTTP client
/// (e.g. the Razor Pages client) can (de)serialize against the same type.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public IEnumerable<string>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

public static class ApiResponse
{
    public static ApiResponse<object> Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public static ApiResponse<object> Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
