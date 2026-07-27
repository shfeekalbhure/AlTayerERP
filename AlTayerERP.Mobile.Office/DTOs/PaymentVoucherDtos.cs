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

    public string PostingStatus => IsPosted ? "مرحّل" : "غير مرحّل";
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
    public int CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Amount { get; set; }
    public decimal ForeignTotal { get; set; }
    public decimal LocalTotal { get; set; }
    public bool IsPosted { get; set; }
    public long? JournalEntryId { get; set; }
    public byte ApprovalStatus { get; set; }
}

public sealed class PaymentVoucherLineDto
{
    public int LineNo { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string? CostCenterId { get; set; }
    public int CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal ForeignAmount { get; set; }
    public decimal LocalAmount { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Description { get; set; }
}
