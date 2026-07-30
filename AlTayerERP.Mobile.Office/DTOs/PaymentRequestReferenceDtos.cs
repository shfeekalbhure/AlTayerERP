namespace AlTayerERP.Mobile.Office.DTOs;

/// <summary>
/// نتيجة تحميل مراجع طلب الصرف. لا تتضمن رمز الدخول أو نص الاستجابة الخام.
/// </summary>
public sealed class PaymentRequestReferenceResult
{
    public bool IsSuccess { get; init; }
    public int? HttpStatusCode { get; init; }
    public bool ServerReached { get; init; }
    public ApiErrorType ErrorType { get; init; }
    public PaymentRequestReferencesDto? References { get; init; }
    public int AccountsCount { get; init; }
    public int CostCentersCount { get; init; }
    public int CurrenciesCount { get; init; }
    public int PaymentMethodsCount { get; init; }
    public int OpenPeriodsCount { get; init; }

    public string UserMessage => ErrorType switch
    {
        _ => ApiDiagnosticResult.GetMessage(ErrorType);

    public static PaymentRequestReferenceResult Success(PaymentRequestReferencesDto references, int statusCode) => new()
    {
        IsSuccess = true,
        HttpStatusCode = statusCode,
        ServerReached = true,
        ErrorType = ApiErrorType.None,
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
