namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class DocumentSearchResultDto
{
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentTypeDisplay { get; set; } = string.Empty;
    public long DocumentId { get; set; }
    public string DocumentNo { get; set; } = string.Empty;
    public DateTime DocumentDate { get; set; }
    public string? PartyName { get; set; }
    public string? Description { get; set; }
    public string? ReferenceNo { get; set; }
    public decimal LocalTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPosted { get; set; }

    public string StatusDisplay => DocumentType == "PAYMENT_REQUEST"
        ? Status switch
        {
            "DRAFT" => "مسودة",
            "PENDING_REVIEW" => "بانتظار المراجعة",
            "PENDING_APPROVAL" => "بانتظار الاعتماد",
            "APPROVED" => "معتمد",
            "REJECTED" => "مرفوض",
            "RETURNED" => "معاد",
            _ => Status
        }
        : IsPosted ? "مرحّل" : "غير مرحّل";
}
