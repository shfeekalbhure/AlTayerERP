namespace AlTayerERP.Mobile.Office.DTOs;

public enum PaymentRequestReferenceErrorType
{
    None,
    ConnectionRefused,
    Timeout,
    Dns,
    Unauthorized,
    Forbidden,
    NotFound,
    ServerError,
    DeserializeFailure,
    Unknown
}

/// <summary>
/// نتيجة تحميل مراجع طلب الصرف. لا تتضمن رمز الدخول أو نص الاستجابة الخام.
/// </summary>
public sealed class PaymentRequestReferenceResult
{
    public bool IsSuccess { get; init; }
    public int? HttpStatusCode { get; init; }
    public bool ServerReached { get; init; }
    public PaymentRequestReferenceErrorType ErrorType { get; init; }
    public PaymentRequestReferencesDto? References { get; init; }
    public int AccountsCount { get; init; }
    public int CostCentersCount { get; init; }
    public int CurrenciesCount { get; init; }
    public int PaymentMethodsCount { get; init; }
    public int OpenPeriodsCount { get; init; }

    public string UserMessage => ErrorType switch
    {
        PaymentRequestReferenceErrorType.ConnectionRefused or
        PaymentRequestReferenceErrorType.Timeout or
        PaymentRequestReferenceErrorType.Dns => "تعذر الاتصال بخدمة النظام. تأكد من تشغيل الخادم واتصال الجهاز.",
        PaymentRequestReferenceErrorType.Unauthorized => "انتهت الجلسة. يرجى تسجيل الدخول مرة أخرى.",
        PaymentRequestReferenceErrorType.Forbidden => "ليس لديك صلاحية لتحميل بيانات طلب الصرف.",
        PaymentRequestReferenceErrorType.NotFound => "خدمة بيانات طلب الصرف غير متاحة في إصدار الخادم الحالي.",
        PaymentRequestReferenceErrorType.ServerError => "تعذر تحميل بيانات طلب الصرف بسبب خطأ في الخادم.",
        PaymentRequestReferenceErrorType.DeserializeFailure => "تعذر قراءة بيانات طلب الصرف المستلمة من الخادم.",
        _ => "تعذر تحميل بيانات طلب الصرف."
    };

    public static PaymentRequestReferenceResult Success(PaymentRequestReferencesDto references, int statusCode) => new()
    {
        IsSuccess = true,
        HttpStatusCode = statusCode,
        ServerReached = true,
        ErrorType = PaymentRequestReferenceErrorType.None,
        References = references,
        AccountsCount = references.Accounts.Count,
        CostCentersCount = references.CostCenters.Count,
        CurrenciesCount = references.Currencies.Count,
        PaymentMethodsCount = references.PaymentMethods.Count,
        OpenPeriodsCount = references.OpenPeriods.Count
    };
}

public sealed class PaymentRequestReferencesDto
{
    public List<PaymentRequestReferenceItemDto> Accounts { get; set; } = [];
    public List<PaymentRequestReferenceItemDto> CostCenters { get; set; } = [];
    public List<PaymentRequestCurrencyDto> Currencies { get; set; } = [];
    public List<PaymentRequestMethodDto> PaymentMethods { get; set; } = [];
    public List<PaymentRequestOpenPeriodDto> OpenPeriods { get; set; } = [];
}

public sealed class PaymentRequestReferenceItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class PaymentRequestMethodDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class PaymentRequestCurrencyDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public bool IsLocal { get; set; }
    public bool IsDefault { get; set; }
}

public sealed class PaymentRequestOpenPeriodDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public bool Contains(DateTime date) =>
        date.Date >= StartDate.Date && date.Date <= EndDate.Date;
}
