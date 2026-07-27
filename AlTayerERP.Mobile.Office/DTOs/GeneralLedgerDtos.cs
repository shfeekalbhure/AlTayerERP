namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class GeneralLedgerAccountDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class GeneralLedgerResponseDto
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
    public List<GeneralLedgerRowDto> Rows { get; set; } = [];
}

public sealed class GeneralLedgerRowDto
{
    public long JournalEntryId { get; set; }
    public string EntryNo { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string? SourceDocumentType { get; set; }
    public string? SourceDocumentNo { get; set; }
    public int LineNo { get; set; }
    public string? Description { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceNo { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningDebit { get; set; }
    public decimal RunningCredit { get; set; }
}
