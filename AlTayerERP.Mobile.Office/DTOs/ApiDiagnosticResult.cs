using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office.DTOs;

public enum ApiErrorType
{
    None, ConnectionRefused, Timeout, Dns, Unauthorized, Forbidden,
    NotFound, Conflict, ServerError, DeserializeFailure, Unknown
}

/// <summary>تشخيص محلي آمن؛ لا يمثل عقد استجابة API ولا يحمل أسراراً.</summary>
public sealed class ApiDiagnosticResult
{
    public string App { get; init; } = "Mobile";
    public string Endpoint { get; init; } = string.Empty;
    public string BaseAddress { get; init; } = string.Empty;
    public bool ServerReached { get; init; }
    public int? StatusCode { get; init; }
    public ApiErrorType ErrorType { get; init; }
    public string? CompanyId { get; init; }
    public int? BranchId { get; init; }
    public int? FiscalYearId { get; init; }
    public int AccountsCount { get; init; }
    public int CostCentersCount { get; init; }
    public int CurrenciesCount { get; init; }
    public int PaymentMethodsCount { get; init; }
    public int OpenPeriodsCount { get; init; }
    public ApiConnectionKind ConnectionKind { get; init; }
    public string? DatabaseName { get; init; }
    public string Message => GetMessage(ErrorType);

    public static string GetMessage(ApiErrorType errorType) => errorType switch
    {
        ApiErrorType.ConnectionRefused or ApiErrorType.Timeout or ApiErrorType.Dns => "تعذر الاتصال بخدمة النظام. تأكد من تشغيل الخادم واتصال الجهاز.",
        ApiErrorType.Unauthorized => "انتهت جلسة الدخول. يرجى تسجيل الدخول من جديد.",
        ApiErrorType.Forbidden => "ليس لديك صلاحية لتنفيذ هذه العملية.",
        ApiErrorType.NotFound => "البيانات المطلوبة غير موجودة أو لا يسمح لك بالوصول إليها.",
        ApiErrorType.Conflict => "تم تعديل البيانات من مستخدم آخر. سيتم تحميل أحدث نسخة.",
        ApiErrorType.ServerError or ApiErrorType.DeserializeFailure => "حدث خطأ في خدمة النظام. حاول مرة أخرى أو تواصل مع المسؤول.",
        _ => "حدث خطأ في خدمة النظام. حاول مرة أخرى أو تواصل مع المسؤول."
    };
}
