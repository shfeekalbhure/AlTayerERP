using System.Diagnostics;
using System.Net;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office.Services;

/// <summary>
/// يحول أخطاء الاتصال وHTTP إلى رسائل عربية آمنة قبل عرضها للمستخدم.
/// لا يمرر نص الخادم أو الاستثناء أو أي أسرار إلى الواجهة.
/// </summary>
public sealed class MobileApiErrorHandler(SessionStorageService sessionStorage)
{
    private bool _redirectingToLogin;

    public static string GetUserMessage(Exception exception) =>
        (IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("خدمات التطبيق غير مهيأة.")).GetRequiredService<MobileApiErrorHandler>()
            .GetUserMessageCore(exception);

    public static bool IsConflict(Exception exception) =>
        ResolveErrorType(exception) == ApiErrorType.Conflict;

    public static void EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        throw new MobileApiException(response.StatusCode, response.RequestMessage?.RequestUri?.ToString());
    }

    /// <summary>
    /// يقبل الرسائل المعرفة صراحة داخل JSON فقط، ويعيد الرسالة البديلة عند وجود HTML
    /// أو نص وسيط أو تفاصيل تشغيلية. لا يعرض جسم HTTP الخام أبداً.
    /// </summary>
    public static string FromPayload(string? payload, string fallback)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return fallback;

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return fallback;

            foreach (var name in new[] { "message", "detail", "title" })
            {
                if (document.RootElement.TryGetProperty(name, out var value) &&
                    value.ValueKind == JsonValueKind.String &&
                    IsSafeServerMessage(value.GetString()))
                {
                    return value.GetString()!.Trim();
                }
            }
        }
        catch (JsonException)
        {
            // لا تعرض صفحات البروكسي أو نصوص الاستضافة غير المنظمة للمستخدم.
        }

        return fallback;
    }

    private string GetUserMessageCore(Exception exception)
    {
        var errorType = ResolveErrorType(exception);
        WriteDevelopment(exception, errorType);

        if (errorType == ApiErrorType.Unauthorized)
        {
            sessionStorage.Clear();
            RedirectToLogin();
        }

        return errorType == ApiErrorType.Unknown && IsSafeLocalMessage(exception.Message)
            ? exception.Message
            : GetMessage(errorType);
    }

    private void RedirectToLogin()
    {
        if (_redirectingToLogin)
            return;

        _redirectingToLogin = true;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (Application.Current?.Windows.FirstOrDefault() is not Window window)
                    return;

                var loginPage = (IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("خدمات التطبيق غير مهيأة.")).GetRequiredService<MainPage>();
                window.Page = new NavigationPage(loginPage)
                {
                    FlowDirection = FlowDirection.RightToLeft,
                    BarBackgroundColor = Color.FromArgb("#17324D"),
                    BarTextColor = Colors.White
                };
            }
            finally
            {
                _redirectingToLogin = false;
            }
        });
    }

    private static ApiErrorType ResolveErrorType(Exception exception) => exception switch
    {
        ApiDiagnosticException diagnostic => diagnostic.Diagnostic.ErrorType,
        MobileApiException api => FromStatusCode((int)api.StatusCode),
        HttpRequestException request when request.StatusCode is not null => FromStatusCode((int)request.StatusCode),
        _ => ApiClientConfiguration.Classify(exception)
    };

    private static ApiErrorType FromStatusCode(int statusCode) => statusCode switch
    {
        401 => ApiErrorType.Unauthorized,
        403 => ApiErrorType.Forbidden,
        404 => ApiErrorType.NotFound,
        409 => ApiErrorType.Conflict,
        >= 500 => ApiErrorType.ServerError,
        _ => ApiErrorType.Unknown
    };

    private static string GetMessage(ApiErrorType errorType) => errorType switch
    {
        ApiErrorType.ConnectionRefused or ApiErrorType.Timeout or ApiErrorType.Dns =>
            "تعذر الاتصال بخدمة النظام. تأكد من تشغيل الخادم واتصال الجهاز.",
        ApiErrorType.Unauthorized => "انتهت جلسة الدخول. يرجى تسجيل الدخول من جديد.",
        ApiErrorType.Forbidden => "ليس لديك صلاحية لتنفيذ هذه العملية.",
        ApiErrorType.NotFound => "البيانات المطلوبة غير موجودة أو لا يسمح لك بالوصول إليها.",
        ApiErrorType.Conflict => "تم تعديل البيانات من مستخدم آخر. سيتم تحميل أحدث نسخة.",
        ApiErrorType.ServerError or ApiErrorType.DeserializeFailure or ApiErrorType.Unknown =>
            "حدث خطأ في خدمة النظام. حاول مرة أخرى أو تواصل مع المسؤول.",
        _ => "حدث خطأ في خدمة النظام. حاول مرة أخرى أو تواصل مع المسؤول."
    };

    private static bool IsSafeLocalMessage(string message) =>
        !string.IsNullOrWhiteSpace(message) &&
        message.Length <= 300 &&
        message.Any(character => character is >= '\u0600' and <= '\u06FF') &&
        !message.Contains("http", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("json", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("token", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("stack", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("connection string", StringComparison.OrdinalIgnoreCase);

    private static bool IsSafeServerMessage(string? message) =>
        !string.IsNullOrWhiteSpace(message) &&
        message.Length <= 300 &&
        !message.Contains('<') &&
        !message.Contains("http", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("json", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("token", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("stack", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("System.", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains("MySql", StringComparison.OrdinalIgnoreCase) &&
        !message.Contains(" at ", StringComparison.OrdinalIgnoreCase);

    [Conditional("DEBUG")]
    private static void WriteDevelopment(Exception exception, ApiErrorType errorType)
    {
        var endpoint = exception is MobileApiException api ? api.Endpoint : string.Empty;
        var statusCode = exception switch
        {
            MobileApiException response => (int?)response.StatusCode,
            HttpRequestException request => (int?)request.StatusCode,
            _ => null
        };
        var safeEndpoint = string.Empty;
        var baseAddress = string.Empty;
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            baseAddress = uri.GetLeftPart(UriPartial.Authority);
            safeEndpoint = uri.AbsolutePath;
        }

        Debug.WriteLine($"[MobileApiError] BaseAddress={baseAddress} Endpoint={safeEndpoint} StatusCode={statusCode?.ToString() ?? "none"} ErrorType={errorType} ServerReached={statusCode is not null}");
    }
}

public sealed class MobileApiException(HttpStatusCode statusCode, string? endpoint) : Exception()
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string? Endpoint { get; } = endpoint;
}
