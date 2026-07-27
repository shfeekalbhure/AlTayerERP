namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class ReceiptVoucherListItemDto
{
    public long VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? ReceivedFromName { get; set; }
    public string? Description { get; set; }
    public decimal LocalTotal { get; set; }
    public bool IsPosted { get; set; }
    public long? JournalEntryId { get; set; }
    public string? ReferenceNo { get; set; }
    public string PostingStatus => IsPosted ? "مرحّل" : "غير مرحّل";
}

public sealed class ReceiptVoucherDetailsDto
{
    public ReceiptVoucherHeaderDto Header { get; set; } = new();
    public List<ReceiptVoucherLineDto> Details { get; set; } = [];
}

public sealed class ReceiptVoucherHeaderDto
{
    public long VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? ReceivedFromName { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNo { get; set; }
    public string CashAccountId { get; set; } = string.Empty;
    public int CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Amount { get; set; }
    public decimal ForeignTotal { get; set; }
    public decimal LocalTotal { get; set; }
    public bool IsPosted { get; set; }
    public long? JournalEntryId { get; set; }
}

public sealed class ReceiptVoucherLineDto
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
