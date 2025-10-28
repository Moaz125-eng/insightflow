using InsightFlow.Core.Abstractions;
using InsightFlow.Core.Models;
using Microsoft.AspNetCore.Http;

namespace InsightFlow.Infrastructure.Audit;

public sealed class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditService _auditService;

    public AuditLoggingMiddleware(RequestDelegate next, IAuditService auditService)
    {
        _next = next;
        _auditService = auditService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (!ShouldAudit(context))
            return;

        var entry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            Actor = context.User.Identity?.Name ?? "anonymous",
            Action = ResolveAction(context.Request.Method),
            Resource = context.Request.Path.Value ?? "/",
            Method = context.Request.Method,
            StatusCode = context.Response.StatusCode,
            OccurredAt = DateTimeOffset.UtcNow
        };

        await _auditService.RecordAsync(entry, context.RequestAborted);
    }

    private static bool ShouldAudit(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
            return false;

        if (context.Request.Path.StartsWithSegments("/api/auth/login"))
            return false;

        return context.Request.Method is "POST" or "PUT" or "PATCH" or "DELETE";
    }

    private static string ResolveAction(string method) =>
        method switch
        {
            "POST" => "create",
            "PUT" => "update",
            "PATCH" => "update",
            "DELETE" => "delete",
            _ => "access"
        };
}
