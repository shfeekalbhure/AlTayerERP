namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class VoucherJournalDto
{
    public string VoucherNo { get; set; } = string.Empty;
    public VoucherJournalHeaderDto Header { get; set; } = new();
    public List<VoucherJournalLineDto> Details { get; set; } = [];
}

public sealed class VoucherJournalHeaderDto
{
    public long JournalEntryId { get; set; }
    public string EntryNo { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string? Description { get; set; }
    public string? SourceDocumentNo { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public bool IsPosted { get; set; }
    public bool IsCancelled { get; set; }
}

public sealed class VoucherJournalLineDto
{
    public int LineNo { get; set; }
    public string AccountDisplay { get; set; } = string.Empty;
    public string? CostCenterDisplay { get; set; }
    public string CurrencyDisplay { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal ForeignAmount { get; set; }
    public decimal LocalAmount { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceNo { get; set; }
}
