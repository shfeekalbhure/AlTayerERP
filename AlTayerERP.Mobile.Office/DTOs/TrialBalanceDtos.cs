namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class TrialBalanceResponseDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<TrialBalanceRowDto> Rows { get; set; } = [];
    public TrialBalanceTotalsDto Totals { get; set; } = new();
}

public sealed class TrialBalanceRowDto
{
    public string AccountId { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}

public sealed class TrialBalanceTotalsDto
{
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}
