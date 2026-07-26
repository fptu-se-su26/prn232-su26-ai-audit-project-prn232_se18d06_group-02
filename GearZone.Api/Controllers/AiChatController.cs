using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common;
using GearZone.Application.Features.AiChat.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GearZone.Api.Controllers;

[ApiController]
[Route("api/ai-chat")]
[AllowAnonymous]
public sealed class AiChatController : ControllerBase
{
    public const string GuestCookieName = "GearZone.AiGuest";
    private const string RequestedWithHeader = "X-GearZone-AI";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAiChatService _chat;

    public AiChatController(IAiChatService chat)
    {
        _chat = chat;
    }

    [HttpGet("status")]
    public IActionResult Status() => Ok(ApiResponse<object>.Ok(new
    {
        enabled = _chat.IsEnabled,
        authenticated = User.Identity?.IsAuthenticated == true &&
                        User.IsInRole("Customer")
    }));

    [HttpPost("conversations")]
    [EnableRateLimiting("ai-chat")]
    public async Task<IActionResult> Create(
        [FromBody] CreateAiConversationDto request,
        CancellationToken ct)
    {
        if (!IsAllowedBrowserRequest()) return Forbid();
        var actor = ResolveActor(createGuest: true);
        if (actor is null) return Forbid();

        try
        {
            var conversation = await _chat.CreateConversationAsync(actor, ct);
            return Created(
                $"/api/ai-chat/conversations/{conversation.Id}",
                ApiResponse<AiConversationDto>.Ok(conversation));
        }
        catch (AiChatUnavailableException ex)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var actor = ResolveActor(createGuest: false);
        if (actor is null) return Forbid();
        if (!actor.IsCustomer)
        {
            return Unauthorized(ApiResponse.Fail(
                "Sign in as a customer to view saved AI conversations."));
        }

        return Ok(ApiResponse<object>.Ok(
            await _chat.GetConversationsAsync(
                actor.CustomerUserId!,
                pageNumber,
                pageSize,
                ct)));
    }

    [HttpGet("conversations/{id:guid}/messages")]
    public async Task<IActionResult> Messages(
        Guid id,
        [FromQuery] DateTime? beforeUtc = null,
        [FromQuery] int pageSize = 40,
        CancellationToken ct = default)
    {
        var actor = ResolveActor(createGuest: false);
        if (actor is null) return Forbid();

        var result = await _chat.GetMessagesAsync(id, actor, beforeUtc, pageSize, ct);
        return result is null
            ? NotFound(ApiResponse.Fail("AI conversation was not found."))
            : Ok(ApiResponse<AiConversationMessagesDto>.Ok(result));
    }

    [HttpDelete("conversations/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!IsAllowedBrowserRequest()) return Forbid();
        var actor = ResolveActor(createGuest: false);
        if (actor is null) return Forbid();

        return await _chat.DeleteConversationAsync(id, actor, ct)
            ? Ok(ApiResponse.Ok("AI conversation deleted."))
            : NotFound(ApiResponse.Fail("AI conversation was not found."));
    }

    [HttpPost("conversations/{id:guid}/messages/stream")]
    [EnableRateLimiting("ai-chat")]
    public async Task Stream(
        Guid id,
        [FromBody] SendAiMessageDto request,
        CancellationToken ct)
    {
        if (!IsAllowedBrowserRequest())
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var actor = ResolveActor(createGuest: false);
        if (actor is null)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers.CacheControl = "no-cache, no-store";
        Response.Headers["X-Accel-Buffering"] = "no";

        try
        {
            var result = await _chat.SendMessageAsync(
                id,
                actor,
                request,
                (progress, token) => WriteEventAsync(progress.Type, progress.Data, token),
                ct);

            if (!result.WasDuplicate)
            {
                foreach (var chunk in ChunkText(result.AssistantMessage.Content, 32))
                {
                    await WriteEventAsync("response.delta", new { delta = chunk }, ct);
                }
            }

            await WriteEventAsync("response.completed", result, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // The browser closed the stream.
        }
        catch (KeyNotFoundException ex)
        {
            await WriteEventAsync("response.error", new { message = ex.Message, code = "not_found" }, ct);
        }
        catch (ArgumentException ex)
        {
            await WriteEventAsync("response.error", new { message = ex.Message, code = "invalid_request" }, ct);
        }
        catch (AiChatUnavailableException)
        {
            await WriteEventAsync(
                "response.error",
                new
                {
                    message = "The AI assistant is temporarily unavailable. Please try again later.",
                    code = "temporarily_unavailable"
                },
                ct);
        }
    }

    private AiChatActor? ResolveActor(bool createGuest)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (!User.IsInRole("Customer")) return null;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(userId)
                ? null
                : new AiChatActor(userId, null);
        }

        var token = Request.Cookies[GuestCookieName];
        if (string.IsNullOrWhiteSpace(token) && createGuest)
        {
            token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            Response.Cookies.Append(GuestCookieName, token, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps,
                MaxAge = TimeSpan.FromHours(24),
                Path = "/"
            });
        }

        return string.IsNullOrWhiteSpace(token)
            ? null
            : new AiChatActor(null, HashToken(token));
    }

    private bool IsAllowedBrowserRequest()
    {
        if (!string.Equals(
                Request.Headers[RequestedWithHeader].ToString(),
                "1",
                StringComparison.Ordinal))
        {
            return false;
        }

        if (!Request.Headers.TryGetValue("Origin", out var originValue) ||
            string.IsNullOrWhiteSpace(originValue))
        {
            return true;
        }

        if (!Uri.TryCreate(originValue.ToString(), UriKind.Absolute, out var origin))
        {
            return false;
        }

        if (string.Equals(origin.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // YARP normally rewrites Host to the API destination and carries the
        // browser-facing host in X-Forwarded-Host.
        var forwardedHost = Request.Headers["X-Forwarded-Host"]
            .FirstOrDefault()?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
        return !string.IsNullOrWhiteSpace(forwardedHost) &&
               HostString.FromUriComponent(forwardedHost).Host is { Length: > 0 } originalHost &&
               string.Equals(origin.Host, originalHost, StringComparison.OrdinalIgnoreCase);
    }

    private async Task WriteEventAsync(string eventName, object data, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await Response.WriteAsync($"event: {eventName}\n", ct);
        await Response.WriteAsync($"data: {json}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static IEnumerable<string> ChunkText(string value, int maxTextElements)
    {
        var enumerator = StringInfo.GetTextElementEnumerator(value);
        var builder = new StringBuilder();
        var count = 0;
        while (enumerator.MoveNext())
        {
            builder.Append(enumerator.GetTextElement());
            count++;
            if (count < maxTextElements) continue;

            yield return builder.ToString();
            builder.Clear();
            count = 0;
        }

        if (builder.Length > 0) yield return builder.ToString();
    }
}
