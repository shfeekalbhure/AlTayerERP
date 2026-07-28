namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class PaymentRequestListItemDto
{
    public long Payment_Request_ID { get; set; }
    public string Request_No { get; set; } = string.Empty;
    public DateTime Request_Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Beneficiary_Name { get; set; } = string.Empty;
    public string? Party_ID { get; set; }
    public int? Payment_Method_ID { get; set; }
    public string? Header_Reference_No { get; set; }
    public string? Description { get; set; }
    public decimal Approved_Local_Total { get; set; }
    public long? Payment_Voucher_ID { get; set; }
    public string Created_By { get; set; } = string.Empty;
    public DateTime Created_At { get; set; }
    public string? Updated_By { get; set; }
    public DateTime? Updated_At { get; set; }
    public List<PaymentRequestLineItemDto> Details { get; set; } = [];

    public DateTime LastModifiedAt => Updated_At ?? Created_At;
    public decimal LocalTotal => Details.Sum(x => x.Local_Amount);
    public string StatusDisplay => Status switch
    {
        "DRAFT" => "مسودة",
        "PENDING_REVIEW" => "بانتظار المراجعة",
        "PENDING_APPROVAL" => "بانتظار الاعتماد",
        "APPROVED" => "معتمد",
        "REJECTED" => "مرفوض",
        "RETURNED" => "معاد",
        _ => Status
    };
}

public sealed class PaymentRequestLineItemDto
{
    public long Payment_Request_Line_ID { get; set; }
    public int Line_No { get; set; }
    public string Account_ID { get; set; } = string.Empty;
    public string? Cost_Center_ID { get; set; }
    public int Currency_ID { get; set; }
    public decimal Exchange_Rate { get; set; }
    public decimal Foreign_Amount { get; set; }
    public decimal Local_Amount { get; set; }
    public string? Reference_No { get; set; }
    public string? Description { get; set; }
}

public sealed class CreatePaymentRequestDto
{
    public DateTime Request_Date { get; set; } = DateTime.Today;
    public string Beneficiary_Name { get; set; } = string.Empty;
    public string? Party_ID { get; set; }
    public int? Payment_Method_ID { get; set; }
    public string? Header_Reference_No { get; set; }
    public string? Description { get; set; }
    public DateTime? Expected_Last_Modified_At { get; set; }
    public List<CreatePaymentRequestLineDto> Lines { get; set; } = [];
}

public sealed class CreatePaymentRequestLineDto
{
    public string Account_ID { get; set; } = string.Empty;
    public string? Cost_Center_ID { get; set; }
    public int Currency_ID { get; set; }
    public decimal Exchange_Rate { get; set; } = 1m;
    public decimal Foreign_Amount { get; set; }
    public decimal Local_Amount { get; set; }
    public string? Reference_No { get; set; }
    public string? Description { get; set; }
}

public sealed class PaymentVoucherSourceDto
{
    public string AccountId { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class CreatePaymentVoucherResponseDto
{
    public long VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
}
