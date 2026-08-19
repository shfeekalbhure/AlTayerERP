using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting;

[Table("payment_requests")]
public sealed class PaymentRequest
{
    [Key] public long Payment_Request_ID { get; set; }
    [Required] public string Company_ID { get; set; }=string.Empty;
    public int Branch_ID { get; set; }
    public int Fiscal_Year_ID { get; set; }
    [Required] public string Request_No { get; set; }=string.Empty;
    public DateTime Request_Date { get; set; }
    [Required] public string Status { get; set; }="DRAFT";
    [Required] public string Beneficiary_Name { get; set; }=string.Empty;
    public string? Party_ID { get; set; }
    public int? Payment_Method_ID { get; set; }
    public string? Header_Reference_No { get; set; }
    public string? Description { get; set; }
    [Column(TypeName="decimal(19,4)")] public decimal Approved_Local_Total { get; set; }
    public long? Payment_Voucher_ID { get; set; }
    public string? Review_Reason { get; set; }
    public string? Approval_Reason { get; set; }
    public string Created_By { get; set; }=string.Empty;
    public DateTime Created_At { get; set; }=DateTime.UtcNow;
    public string? Updated_By { get; set; }
    public DateTime? Updated_At { get; set; }
    public List<PaymentRequestLine> Details { get; set; }=new();
}
[Table("payment_request_lines")]
public sealed class PaymentRequestLine
{
    [Key] public long Payment_Request_Line_ID { get; set; }
    public long Payment_Request_ID { get; set; }
    public int Line_No { get; set; }
    [Required] public string Account_ID { get; set; }=string.Empty;
    public string? Cost_Center_ID { get; set; }
    public int Currency_ID { get; set; }
    [Column(TypeName="decimal(19,8)")] public decimal Exchange_Rate { get; set; }
    [Column(TypeName="decimal(19,4)")] public decimal Foreign_Amount { get; set; }
    [Column(TypeName="decimal(19,4)")] public decimal Local_Amount { get; set; }
    public string? Reference_No { get; set; }
    public string? Description { get; set; }
    public PaymentRequest PaymentRequest { get; set; }=null!;
}