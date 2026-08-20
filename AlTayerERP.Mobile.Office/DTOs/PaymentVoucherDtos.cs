namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class PaymentVoucherListItemDto
{
    public long VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? Description { get; set; }
    public decimal LocalTotal { get; set; }
    public bool IsPosted { get; set; }
    public long? JournalEntryId { get; set; }
    public string? SourceDocumentNo { get; set; }
    public byte ApprovalStatus { get; set; }
    public byte ReviewStatus { get; set; }
    public int VoucherStatusId { get; set; }
    public bool RequiresApproval { get; set; }

    public string PostingStatus => IsPosted ? "مرحّل" : ApprovalStatus == 2 ? "معتمد" : ReviewStatus == 2 ? "تمت المراجعة" : ReviewStatus == 3 ? "معاد للتصحيح" : "مسودة";
}

public sealed class PaymentVoucherDetailsDto
{
    public PaymentVoucherHeaderDto Header { get; set; } = new();
    public List<PaymentVoucherLineDto> Details { get; set; } = [];
}

public sealed class PaymentVoucherHeaderDto
{
    public long VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNo { get; set; }
    public string? SourceDocumentNo { get; set; }
    public string CashAccountId { get; set; } = string.Empty;
    public string CashAccountDisplay { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoDataUri { get; set; }
    public string? PartyId { get; set; }
    public int? PaymentMethodId { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyDisplay { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal Amount { get; set; }
    public decimal ForeignTotal { get; set; }
    public decimal LocalTotal { get; set; }
    public bool IsPosted { get; set; }
    public long? JournalEntryId { get; set; }
    public byte ApprovalStatus { get; set; }
    public byte ReviewStatus { get; set; }
    public string? ReviewNotes { get; set; }
    public int VoucherTypeId { get; set; }
    public int VoucherStatusId { get; set; }
    public bool RequiresApproval { get; set; }
    public int EditCount { get; set; }
    public int PrintCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    /// <summary>رمز التزامن المستلم مع نسخة السند المحمّلة للتعديل.</summary>
    public Guid? RowVersion { get; set; }

    public string WorkflowStatus => IsPosted ? "مرحّل" : ApprovalStatus == 2 ? "معتمد" : ReviewStatus == 2 ? "تمت المراجعة" : ReviewStatus == 3 ? "معاد للتصحيح" : "مسودة";
}

public sealed class PaymentVoucherLineDto
{
    public long VoucherDetailId { get; set; }
    public int LineNo { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string AccountDisplay { get; set; } = string.Empty;
    public string? CostCenterId { get; set; }
    public string? CostCenterDisplay { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyDisplay { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal ForeignAmount { get; set; }
    public decimal LocalAmount { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public byte LineType { get; set; }
    public string? Description { get; set; }
}
