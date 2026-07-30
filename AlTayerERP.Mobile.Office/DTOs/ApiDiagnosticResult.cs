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
        ApiErrorType.Unauthorized => "انتهت الجلسة. يرجى تسجيل الدخول مرة أخرى.",
        ApiErrorType.Forbidden => "ليس لديك صلاحية لتنفيذ هذه العملية.",
        ApiErrorType.NotFound => "الخدمة المطلوبة غير متاحة في إصدار الخادم الحالي.",
        ApiErrorType.Conflict => "تعذر إكمال العملية بسبب تعارض في البيانات. أعد تحميل الحالة ثم حاول مجددًا.",
        ApiErrorType.ServerError => "تعذر إكمال العملية بسبب خطأ في الخادم.",
        ApiErrorType.DeserializeFailure => "تعذر قراءة البيانات المستلمة من الخادم.",
        _ => "تعذر إكمال العملية."
    };
}
