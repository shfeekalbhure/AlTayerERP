using System.Collections.Generic;

namespace AlTayerERP.API.Infrastructure.Observability;

/// <summary>
/// يضمن وجود معرف ارتباط ثابت لكل طلب حتى يمكن تتبع الخطأ من الواجهة
/// إلى سجل الـ API وقاعدة البيانات دون الاعتماد على رسائل مبهمة.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private const int MaxCorrelationIdLength = 128;

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = ResolveCorrelationId(context);

        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using IDisposable? scope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });

        await _next(context);
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            string requestedValue = values.ToString().Trim();

            if (IsValid(requestedValue))
            {
                return requestedValue;
            }
        }

        return Guid.NewGuid().ToString("N");
    }

    private static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > MaxCorrelationIdLength)
        {
            return false;
        }

        foreach (char character in value)
        {
            bool allowed = char.IsLetterOrDigit(character) ||
                           character is '-' or '_' or '.' or ':';

            if (!allowed)
            {
                return false;
            }
        }

        return true;
    }
}
