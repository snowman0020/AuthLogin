using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AuthApi.API.Middleware;

public class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    // Fields whose values must be redacted from request bodies
    private static readonly HashSet<string> _sensitiveFields =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "passwordHash", "token", "refreshToken",
            "accessToken", "secret", "apiKey", "key"
        };

    public async Task InvokeAsync(HttpContext ctx)
    {
        // Skip swagger / health-check endpoints
        if (ShouldSkip(ctx.Request.Path))
        {
            await next(ctx);
            return;
        }

        var sw = Stopwatch.StartNew();

        // Buffer the request body so we can read it
        ctx.Request.EnableBuffering();
        var requestBody = await ReadBodyAsync(ctx.Request);

        await next(ctx);   // ← execute the rest of the pipeline

        sw.Stop();

        // Fire-and-forget: do not block the response
        _ = WriteLogAsync(ctx, requestBody, sw.ElapsedMilliseconds);
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private static async Task WriteLogAsync(
        HttpContext ctx,
        string? requestBody,
        long durationMs)
    {
        try
        {
            var repo = ctx.RequestServices.GetService<IAuditLogRepository>();
            if (repo is null) return;

            var userId    = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = ctx.User.FindFirstValue(ClaimTypes.Email);
            var authMethod = ResolveAuthMethod(ctx);

            var log = new AuditLog
            {
                Method      = ctx.Request.Method,
                Path        = ctx.Request.Path.ToString(),
                StatusCode  = ctx.Response.StatusCode,
                DurationMs  = durationMs,
                UserId      = userId,
                UserEmail   = userEmail,
                IpAddress   = GetIpAddress(ctx),
                UserAgent   = ctx.Request.Headers.UserAgent.ToString(),
                RequestBody = SanitizeBody(requestBody),
                AuthMethod  = authMethod,
                CreatedAt   = DateTime.UtcNow
            };

            await repo.CreateAsync(log);
        }
        catch (Exception ex)
        {
            // Never crash the app because of logging
            var loggerFactory = ctx.RequestServices.GetService<ILoggerFactory>();
            var fallback = loggerFactory?.CreateLogger<RequestLoggingMiddleware>();
            fallback?.LogError(ex, "Failed to write audit log to MongoDB");
        }
    }

    private static async Task<string?> ReadBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0) return null;
        if (!request.ContentType?.Contains("application/json") ?? true) return null;

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static string? SanitizeBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;

        // Truncate large bodies
        if (body.Length > 2000) body = body[..2000] + "...[truncated]";

        try
        {
            using var doc = JsonDocument.Parse(body);
            var sanitized = RedactSensitiveFields(doc.RootElement);
            return JsonSerializer.Serialize(sanitized);
        }
        catch
        {
            return "[non-JSON body]";
        }
    }

    private static object RedactSensitiveFields(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return element.ToString();

        var dict = new Dictionary<string, object?>();
        foreach (var prop in element.EnumerateObject())
        {
            dict[prop.Name] = _sensitiveFields.Contains(prop.Name)
                ? "***REDACTED***"
                : prop.Value.ValueKind == JsonValueKind.Object
                    ? RedactSensitiveFields(prop.Value)
                    : prop.Value.ToString();
        }
        return dict;
    }

    private static string ResolveAuthMethod(HttpContext ctx)
    {
        if (ctx.Request.Headers.ContainsKey("X-API-Key")) return "ApiKey";
        if (ctx.Request.Headers.Authorization.ToString().StartsWith("Bearer ")) return "Bearer";
        return "Anonymous";
    }

    private static string GetIpAddress(HttpContext ctx)
    {
        // Respect X-Forwarded-For (reverse proxy / load balancer)
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static bool ShouldSkip(PathString path)
    {
        var p = path.Value ?? string.Empty;
        return p.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || p.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase)
            || p.Equals("/health", StringComparison.OrdinalIgnoreCase);
    }
}

// Extension method for clean registration
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<RequestLoggingMiddleware>();
}
