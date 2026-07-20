using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace AlTayerERP.API.Infrastructure;

/// <summary>
/// يحول الاستثناءات غير المعالجة إلى استجابة Problem Details موحدة،
/// ويمنع تسريب تفاصيل تقنية أو بيانات حساسة إلى العميل.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var error = MapException(exception);
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        if (error.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled API exception. TraceId: {TraceId}; Path: {Path}",
                traceId,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Rejected API request. Code: {ErrorCode}; TraceId: {TraceId}; Path: {Path}",
                error.Code,
                traceId,
                httpContext.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Type = $"https://httpstatuses.com/{error.StatusCode}",
            Instance = httpContext.Request.Path,
            Detail = BuildSafeDetail(exception, error.StatusCode)
        };

        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = traceId;

        if (environment.IsDevelopment())
        {
            problem.Extensions["exceptionType"] = exception.GetType().Name;
        }

        httpContext.Response.StatusCode = error.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private string BuildSafeDetail(Exception exception, int statusCode)
    {
        if (statusCode < StatusCodes.Status500InternalServerError)
        {
            return string.IsNullOrWhiteSpace(exception.Message)
                ? "تعذر تنفيذ الطلب بالقيم المرسلة."
                : exception.Message;
        }

        return environment.IsDevelopment()
            ? exception.Message
            : "حدث خطأ غير متوقع. استخدم رقم التتبع عند التواصل مع الدعم الفني.";
    }

    private static ApiError MapException(Exception exception) => exception switch
    {
        ValidationException => new(
            StatusCodes.Status400BadRequest,
            "بيانات الطلب غير صحيحة.",
            "validation_error"),

        ArgumentException => new(
            StatusCodes.Status400BadRequest,
            "بيانات الطلب غير صحيحة.",
            "invalid_argument"),

        KeyNotFoundException => new(
            StatusCodes.Status404NotFound,
            "العنصر المطلوب غير موجود.",
            "not_found"),

        UnauthorizedAccessException => new(
            StatusCodes.Status403Forbidden,
            "غير مسموح بتنفيذ هذه العملية.",
            "forbidden"),

        DbUpdateConcurrencyException => new(
            StatusCodes.Status409Conflict,
            "تم تعديل السجل بواسطة مستخدم آخر.",
            "concurrency_conflict"),

        DbUpdateException => new(
            StatusCodes.Status409Conflict,
            "تعذر حفظ التغييرات بسبب تعارض في البيانات.",
            "database_conflict"),

        InvalidOperationException => new(
            StatusCodes.Status409Conflict,
            "لا يمكن تنفيذ العملية في الحالة الحالية.",
            "invalid_operation"),

        _ => new(
            StatusCodes.Status500InternalServerError,
            "حدث خطأ داخلي في النظام.",
            "internal_error")
    };

    private sealed record ApiError(int StatusCode, string Title, string Code);
}
