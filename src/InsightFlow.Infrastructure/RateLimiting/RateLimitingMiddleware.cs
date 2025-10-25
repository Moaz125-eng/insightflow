using InsightFlow.Core.Configuration;
using Microsoft.AspNetCore.Http;

namespace InsightFlow.Infrastructure.RateLimiting;

public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitCounter _counter;
    private readonly RateLimitSettings _settings;

    public RateLimitingMiddleware(RequestDelegate next, RateLimitCounter counter, RateLimitSettings settings)
    {
        _next = next;
        _counter = counter;
        _settings = settings;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsExempt(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var key = BuildKey(context);
        var limit = context.Request.Path.StartsWithSegments("/api/documents") &&
                    string.Equals(context.Request.Method, HttpMethods.Post, StringComparison.OrdinalIgnoreCase)
            ? _settings.UploadRequestsPerMinute
            : _settings.RequestsPerMinute;

        if (!_counter.TryConsume(key, limit, TimeSpan.FromMinutes(1)))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = "60";
            await context.Response.WriteAsJsonAsync(new { error = "Rate limit exceeded. Try again shortly." });
            return;
        }

        await _next(context);
    }

    private static bool IsExempt(PathString path) =>
        path.StartsWithSegments("/health") || path.StartsWithSegments("/api/health");

    private static string BuildKey(HttpContext context)
    {
        var user = context.User.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(user))
            return $"user:{user}";

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }
}
