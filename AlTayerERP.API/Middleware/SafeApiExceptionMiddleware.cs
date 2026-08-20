using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AlTayerERP.API.Middleware;

/// <summary>
/// يحوّل الاستثناءات غير المتوقعة إلى استجابة API آمنة، مع إبقاء التفاصيل
/// التشخيصية في سجل الخادم فقط وربطها بمعرف التتبع.
/// </summary>
public sealed class SafeApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public SafeApiExceptionMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // إلغاء العميل ليس خطأ خادماً ولا يجوز تحويله إلى 500.
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled API exception. TraceIdentifier: {TraceIdentifier}",
                context.TraceIdentifier);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";

            await context.Response.WriteAsJsonAsync(
                new
                {
                    success = false,
                    message = "حدث خطأ غير متوقع. أعد المحاولة، وقدّم معرف التتبع للدعم عند استمرار المشكلة.",
                    correlationId = context.TraceIdentifier
                },
                context.RequestAborted);
        }
    }
}
