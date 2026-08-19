namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class JournalVoucherListItemDto
{
    public long VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? Description { get; set; }
    public decimal DebitTotal { get; set; }
    public decimal CreditTotal { get; set; }
    public bool IsPosted { get; set; }
    public long? JournalEntryId { get; set; }
    public string PostingStatus => IsPosted ? "مرحّل" : "غير مرحّل";
    public string BalanceStatus => DebitTotal == CreditTotal ? "متوازن" : "غير متوازن";
}

public sealed class JournalVoucherDetailsDto
{
    public JournalVoucherHeaderDto Header { get; set; } = new();
    public List<JournalVoucherLineDto> Details { get; set; } = [];
}

public sealed class JournalVoucherHeaderDto
{
    public long VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNo { get; set; }
    public bool IsPosted { get; set; }
    public long? JournalEntryId { get; set; }
    public decimal DebitTotal { get; set; }
    public decimal CreditTotal { get; set; }
}

public sealed class JournalVoucherLineDto
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
    public string? ReferenceType { get; set; }
    public string? ReferenceNo { get; set; }
    public string? ReferenceName { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public string? Description { get; set; }
}
