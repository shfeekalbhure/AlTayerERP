namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class PaymentRequestReferencesDto
{
    public List<PaymentRequestReferenceItemDto> Accounts { get; set; } = [];
    public List<PaymentRequestReferenceItemDto> CostCenters { get; set; } = [];
    public List<PaymentRequestCurrencyDto> Currencies { get; set; } = [];
    public List<PaymentRequestOpenPeriodDto> OpenPeriods { get; set; } = [];
}

public sealed class PaymentRequestReferenceItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
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
