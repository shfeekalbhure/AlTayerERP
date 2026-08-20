using System;

namespace AlTayerERP.API.DTOs.Accounting;

/// <summary>
/// عرض تشغيلي مختصر لسند مالي. يُستخدم للعملاء الذين يحتاجون حالة السند
/// وبياناته المالية الأساسية فقط ولا يحتاجون سجل العمليات أو تفاصيل القيود.
/// </summary>
public sealed class FinancialVoucherSummaryDto
{
    public long Voucher_ID { get; init; }
    public string Voucher_No { get; init; } = string.Empty;
    public int Voucher_Type_ID { get; init; }
    public string Voucher_Type_Name { get; init; } = string.Empty;
    public int Voucher_Status_ID { get; init; }
    public string Voucher_Status_Name { get; init; } = string.Empty;
    public DateTime Voucher_Date { get; init; }
    public DateTime Transaction_Date { get; init; }
    public string Cash_Account_Name { get; init; } = string.Empty;
    public string? Party_Name { get; init; }
    public string? Received_From_Name { get; init; }
    public string? Payment_Method_Name { get; init; }
    public string Currency_Name { get; init; } = string.Empty;
    public decimal Exchange_Rate { get; init; }
    public decimal Amount { get; init; }
    public decimal Foreign_Total { get; init; }
    public decimal Local_Total { get; init; }
    public string? Reference_No { get; init; }
    public DateTime? Reference_Date { get; init; }
    public string? Description { get; init; }
    public bool Requires_Approval { get; init; }
    public byte Approval_Status { get; init; }
    public byte Review_Status { get; init; }
    public bool Is_Posted { get; init; }
    public string? Journal_Entry_No { get; init; }
}
