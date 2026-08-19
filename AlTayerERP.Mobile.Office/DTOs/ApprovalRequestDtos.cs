namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class ApprovalRequestListItemDto
{
    public int Approval_ID { get; set; }
    public string Request_Type { get; set; } = string.Empty;
    public string? Reference_Type { get; set; }
    public string? Reference_ID { get; set; }
    public string? Entity_Type { get; set; }
    public string? Entity_ID { get; set; }
    public string? Currency_Code { get; set; }
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Requested_By { get; set; }
    public DateTime Requested_At { get; set; }
    public string? Approved_By { get; set; }
    public DateTime? Approved_At { get; set; }
    public string? Approval_Notes { get; set; }
    public int Edit_Count { get; set; }
    public int Print_Count { get; set; }

    public string StatusDisplay => Status switch
    {
        "Pending" => "بانتظار الاعتماد",
        "UnderReview" => "تحت المراجعة",
        "Approved" => "معتمد",
        "Rejected" => "مرفوض",
        "Returned" => "معاد للتعديل",
        "Canceled" => "ملغي",
        _ => Status
    };

    public string ReferenceDisplay => string.IsNullOrWhiteSpace(Reference_ID)
        ? (Reference_Type ?? Request_Type)
        : $"{Reference_Type ?? Request_Type} / {Reference_ID}";

    public string AmountDisplay => Amount.HasValue
        ? $"{Amount.Value:N2} {Currency_Code}".Trim()
        : "—";
}

public sealed class ApprovalDecisionDto
{
    public string? Reason { get; set; }
}
