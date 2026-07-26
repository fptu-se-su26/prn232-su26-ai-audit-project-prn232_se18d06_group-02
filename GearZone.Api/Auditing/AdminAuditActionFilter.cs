using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin;
using GearZone.Domain.Enums;
using GearZone.Infrastructure.Auditing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;

namespace GearZone.Api.Auditing;

public sealed class AdminAuditActionFilter : IAsyncActionFilter
{
    private readonly AdminAuditContext _auditContext;
    private readonly IAdminAuditRecorder _recorder;

    public AdminAuditActionFilter(AdminAuditContext auditContext, IAdminAuditRecorder recorder)
    {
        _auditContext = auditContext;
        _recorder = recorder;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var attribute = descriptor?.MethodInfo.GetCustomAttribute<AdminAuditActionAttribute>(inherit: true);
        if (attribute is null)
        {
            await next();
            return;
        }

        var http = context.HttpContext;
        var correlationId = ResolveCorrelationId(http);
        http.Response.Headers["X-Correlation-ID"] = correlationId;
        var auditEvent = new AdminAuditEvent
        {
            OccurredAtUtc = DateTime.UtcNow,
            ActorUserId = http.User.FindFirstValue(ClaimTypes.NameIdentifier),
            ActorDisplayName = http.User.FindFirstValue(ClaimTypes.Name) ?? http.User.Identity?.Name,
            ActorEmail = http.User.FindFirstValue(ClaimTypes.Email) ?? http.User.Identity?.Name,
            Action = attribute.Action,
            Module = attribute.Module,
            Outcome = attribute.SuccessOutcome,
            RiskLevel = attribute.RiskLevel,
            EntityType = attribute.EntityType,
            EntityId = context.RouteData.Values.TryGetValue(attribute.RouteIdName, out var routeId)
                ? Convert.ToString(routeId)
                : null,
            Description = attribute.Description ?? Humanize(attribute.Action),
            Reason = ResolveReason(context, attribute),
            HttpMethod = http.Request.Method,
            RequestPath = http.Request.Path.Value,
            IpAddress = http.Connection.RemoteIpAddress?.ToString(),
            UserAgent = http.Request.Headers.UserAgent.ToString(),
            CorrelationId = correlationId
        };

        foreach (var key in new[] { "reportType", "format", "forceRefresh" })
        {
            if (context.RouteData.Values.TryGetValue(key, out var routeValue))
                auditEvent.Metadata[key] = Convert.ToString(routeValue);
            else if (http.Request.Query.TryGetValue(key, out var queryValue))
                auditEvent.Metadata[key] = queryValue.ToString();
        }

        if (auditEvent.Action == AdminAuditActions.AiInsightGenerated &&
            http.Request.Query.TryGetValue("forceRefresh", out var refresh) &&
            bool.TryParse(refresh.ToString(), out var forceRefresh) && forceRefresh)
        {
            auditEvent.Action = AdminAuditActions.AiInsightRegenerated;
            auditEvent.Description = Humanize(auditEvent.Action);
        }

        var stopwatch = Stopwatch.StartNew();
        using var scope = _auditContext.Begin(auditEvent);
        ActionExecutedContext? executed = null;
        try
        {
            executed = await next();
        }
        catch (Exception ex)
        {
            auditEvent.Outcome = AdminAuditOutcome.Failed;
            auditEvent.StatusCode = StatusCodes.Status500InternalServerError;
            auditEvent.Description = $"{auditEvent.Description}: {ex.GetType().Name}";
            throw;
        }
        finally
        {
            stopwatch.Stop();
            auditEvent.DurationMs = stopwatch.ElapsedMilliseconds;
            if (executed is not null)
            {
                auditEvent.StatusCode = ResolveStatusCode(executed);
                if (executed.Exception is not null && !executed.ExceptionHandled || auditEvent.StatusCode >= 400)
                    auditEvent.Outcome = AdminAuditOutcome.Failed;
            }

            if (!(_auditContext.Current?.WasPersisted ?? false) || auditEvent.Outcome == AdminAuditOutcome.Failed)
                await _recorder.RecordAsync(auditEvent, CancellationToken.None);
        }
    }

    private static int ResolveStatusCode(ActionExecutedContext context) => context.Result switch
    {
        ObjectResult result => result.StatusCode ?? StatusCodes.Status200OK,
        StatusCodeResult result => result.StatusCode,
        _ => context.HttpContext.Response.StatusCode is >= 100 and <= 599
            ? context.HttpContext.Response.StatusCode
            : StatusCodes.Status200OK
    };

    private static string? ResolveReason(ActionExecutingContext context, AdminAuditActionAttribute attribute)
    {
        if (string.IsNullOrWhiteSpace(attribute.ReasonArgumentName) ||
            !context.ActionArguments.TryGetValue(attribute.ReasonArgumentName, out var argument) ||
            argument is null)
            return null;

        if (argument is string text) return text;
        return argument.GetType().GetProperty(attribute.ReasonPropertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(argument)?.ToString();
    }

    private static string ResolveCorrelationId(HttpContext http)
    {
        var candidate = http.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(candidate) && candidate.Length <= 128 &&
            candidate.All(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.'))
            return candidate;
        return http.TraceIdentifier;
    }

    private static string Humanize(string action) =>
        string.Join(' ', action.Split('_', StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
}
