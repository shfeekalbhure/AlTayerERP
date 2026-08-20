using System.Net;
using System.Text.Json;

namespace AlTayerERP.Desktop.Services;

/// <summary>
/// يحول أخطاء API إلى نص مناسب للمستخدم ولا يعرض جسم الاستجابة أو أي تفاصيل تشغيلية خام.
/// </summary>
public static class ApiErrorMessageFormatter
{
    public static async Task<string> FromResponseAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return FromPayload(response.StatusCode, body);
    }

    internal static string FromPayload(HttpStatusCode statusCode, string? body)
    {
        string? message = null;
        string? correlationId = null;

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    message = ReadString(document.RootElement, "message")
                              ?? ReadString(document.RootElement, "title");
                    correlationId = ReadString(document.RootElement, "correlationId");
                }
                else if (document.RootElement.ValueKind == JsonValueKind.String)
                {
                    message = document.RootElement.GetString();
                }
            }
            catch (JsonException)
            {
                // لا نعرض النص الخام؛ قد يكون صفحة وسيطة أو تفاصيل خادم غير صالحة للواجهة.
            }
        }

        var safeMessage = IsSafeForDisplay(message) ? message! : GetFallbackMessage(statusCode);
        return IsSafeForDisplay(correlationId)
            ? $"{safeMessage}\nرقم التتبع: {correlationId}"
            : safeMessage;
    }

    private static string? ReadString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool IsSafeForDisplay(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= 500 &&
        !value.Contains('\r') &&
        !value.Contains('\n');

    private static string GetFallbackMessage(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "تعذر تنفيذ الطلب. تحقق من البيانات المدخلة ثم أعد المحاولة.",
        HttpStatusCode.Unauthorized => "انتهت جلسة الدخول. سجّل الدخول من جديد.",
        HttpStatusCode.Forbidden => "لا تملك صلاحية لتنفيذ هذا الإجراء.",
        HttpStatusCode.NotFound => "طلب الاعتماد غير موجود أو لم يعد متاحاً.",
        HttpStatusCode.Conflict => "تم تعديل الطلب من مستخدم آخر. أعد تحميل القائمة ثم حاول من جديد.",
        _ when (int)statusCode >= 500 => "تعذر تنفيذ الإجراء حالياً. أعد المحاولة أو زوّد الدعم برقم التتبع إن ظهر.",
        _ => "تعذر تنفيذ الإجراء. أعد تحميل القائمة ثم حاول من جديد."
    };
}
