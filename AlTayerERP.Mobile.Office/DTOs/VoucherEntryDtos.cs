namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class VoucherEntryReferencesDto
{
    public VoucherEntryTypeDto VoucherType { get; set; } = new();
    public VoucherEntryStatusDto DraftStatus { get; set; } = new();
    public List<VoucherEntrySourceDto> Sources { get; set; } = [];
    public int SourceCount { get; set; }
    public string? SourceMessage { get; set; }
    /// <summary>رسائل تشخيص مراحل الصلاحية والبيانات التي اجتازها تحميل القوائم.</summary>
    public List<string> PermissionDiagnostics { get; set; } = [];
    public List<VoucherEntryLookupDto> Accounts { get; set; } = [];
    public List<VoucherEntryLookupDto> CostCenters { get; set; } = [];
    public List<VoucherEntryCurrencyDto> Currencies { get; set; } = [];
    public List<VoucherEntryPartyDto> Parties { get; set; } = [];
    public List<VoucherEntryPaymentMethodDto> PaymentMethods { get; set; } = [];
    public List<VoucherEntryPeriodDto> OpenPeriods { get; set; } = [];
}

public sealed class VoucherEntryTypeDto { public int Id { get; set; } public string Code { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; }
public sealed class VoucherEntryStatusDto { public int Id { get; set; } public string Code { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; }
public sealed class VoucherEntrySourceDto { public string AccountId { get; set; } = string.Empty; public string SourceType { get; set; } = string.Empty; public string DisplayName { get; set; } = string.Empty; }
public sealed class VoucherEntryLookupDto { public string Id { get; set; } = string.Empty; public string DisplayName { get; set; } = string.Empty; }
public sealed class VoucherEntryCurrencyDto { public int Id { get; set; } public string DisplayName { get; set; } = string.Empty; public decimal ExchangeRate { get; set; } public bool IsLocal { get; set; } public bool IsDefault { get; set; } }
public sealed class VoucherEntryPartyDto { public string Id { get; set; } = string.Empty; public string DisplayName { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; }
public sealed class VoucherEntryPaymentMethodDto { public int Id { get; set; } public string DisplayName { get; set; } = string.Empty; }
public sealed class VoucherEntryPeriodDto { public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } }

public sealed class CreateMobileVoucherDto
{
    public int Voucher_Type_ID { get; set; }
    public int Voucher_Status_ID { get; set; }
    public string Branch_ID { get; set; } = string.Empty;
    public int Fiscal_Year_ID { get; set; }
    public DateTime Voucher_Date { get; set; }
    public DateTime Transaction_Date { get; set; }
    public string Cash_Account_ID { get; set; } = string.Empty;
    public string? Party_ID { get; set; }
    public string? Received_From_Name { get; set; }
    public int? Payment_Method_ID { get; set; }
    public int Currency_ID { get; set; }
    public decimal Exchange_Rate { get; set; }
    public decimal Amount { get; set; }
    public decimal Foreign_Total { get; set; }
    public decimal Local_Total { get; set; }
    public string? Reference_No { get; set; }
    public string? Against_Text { get; set; }
    public string? Description { get; set; }
    public bool Requires_Approval { get; set; }
    public List<CreateMobileVoucherLineDto> Details { get; set; } = [];
}

public sealed class CreateMobileVoucherLineDto
{
    public int Line_No { get; set; }
    public string Account_ID { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Cost_Center_ID { get; set; }
    public int Currency_ID { get; set; }
    public decimal Exchange_Rate { get; set; }
    public decimal Foreign_Amount { get; set; }
    public decimal Local_Amount { get; set; }
    public decimal Debit_Amount { get; set; }
    public decimal Credit_Amount { get; set; }
    public byte Line_Type { get; set; } = 2;
}

public sealed class UpdateMobileVoucherDto
{
    public long Voucher_ID { get; set; }
    public int Voucher_Type_ID { get; set; }
    public int Voucher_Status_ID { get; set; }
    public string Branch_ID { get; set; } = string.Empty;
    public int Fiscal_Year_ID { get; set; }
    public DateTime Voucher_Date { get; set; }
    public DateTime Transaction_Date { get; set; }
    public string Cash_Account_ID { get; set; } = string.Empty;
    public string? Party_ID { get; set; }
    public string Received_From_Name { get; set; } = string.Empty;
    public int? Payment_Method_ID { get; set; }
    public int Currency_ID { get; set; }
    public decimal Exchange_Rate { get; set; }
    public decimal Amount { get; set; }
    public decimal Foreign_Total { get; set; }
    public decimal Local_Total { get; set; }
    public string? Reference_No { get; set; }
    public string? Against_Text { get; set; }
    public string? Description { get; set; }
    public bool Requires_Approval { get; set; }
    public List<UpdateMobileVoucherLineDto> Details { get; set; } = [];
    public List<object> Allocations { get; set; } = [];
}

public sealed class UpdateMobileVoucherLineDto
{
    public long Voucher_Detail_ID { get; set; }
    public int Line_No { get; set; }
    public string Account_ID { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Cost_Center_ID { get; set; }
    public int Currency_ID { get; set; }
    public decimal Exchange_Rate { get; set; }
    public decimal Foreign_Amount { get; set; }
    public decimal Local_Amount { get; set; }
    public decimal Debit_Amount { get; set; }
    public decimal Credit_Amount { get; set; }
    public byte Line_Type { get; set; } = 2;
}

public sealed class CreateMobileVoucherResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public long Voucher_ID { get; set; }
    public string Voucher_No { get; set; } = string.Empty;
}
