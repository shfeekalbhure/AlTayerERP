using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting;

/// <summary>رأس السند المالي الموحد للقبض والصرف والقيود والتسويات.</summary>
[Table("financial_voucher_headers")]
public class FinancialVoucherHeader
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Voucher_ID { get; set; }

    [Required, MaxLength(50)] public string Voucher_No { get; set; } = string.Empty;
    public int Voucher_Type_ID { get; set; }
    public int Voucher_Status_ID { get; set; }

    /// <summary>معرف الفرع الداخلي المعتمد رقميًا في جميع الجداول التشغيلية.</summary>
    public int Branch_ID { get; set; }
    public int? Fiscal_Year_ID { get; set; }
    public DateTime Voucher_Date { get; set; }
    public DateTime Transaction_Date { get; set; }

    [Required, MaxLength(50)] public string Cash_Account_ID { get; set; } = string.Empty;
    [MaxLength(50)] public string? Party_ID { get; set; }
    [MaxLength(200)] public string? Received_From_Name { get; set; }
    public int? Payment_Method_ID { get; set; }
    public int Currency_ID { get; set; }
    [Column(TypeName = "decimal(18,6)")] public decimal Exchange_Rate { get; set; } = 1m;
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Foreign_Total { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Local_Total { get; set; }
    [MaxLength(100)] public string? Reference_No { get; set; }
    public DateTime? Reference_Date { get; set; }
    [MaxLength(500)] public string? Against_Text { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    public int? Module_ID { get; set; }
    public int? Document_Type_ID { get; set; }
    public long? Document_ID { get; set; }
    [MaxLength(100)] public string? Source_Document_No { get; set; }

    public bool Requires_Approval { get; set; }
    public byte Approval_Status { get; set; }
    [MaxLength(50)] public string? Approval_Requested_By_User_ID { get; set; }
    public DateTime? Approval_Requested_At { get; set; }
    [MaxLength(50)] public string? Approved_By_User_ID { get; set; }
    public DateTime? Approved_At { get; set; }
    [MaxLength(50)] public string? Rejected_By_User_ID { get; set; }
    public DateTime? Rejected_At { get; set; }
    [MaxLength(500)] public string? Rejection_Reason { get; set; }
    public byte Review_Status { get; set; }
    [MaxLength(50)] public string? Reviewed_By_User_ID { get; set; }
    public DateTime? Reviewed_At { get; set; }
    [MaxLength(500)] public string? Review_Notes { get; set; }

    public int Edit_Count { get; set; }
    public int Print_Count { get; set; }
    [MaxLength(50)] public string? Last_Printed_By { get; set; }
    public DateTime? Last_Print_Date { get; set; }
    public int Undo_Count { get; set; }
    [MaxLength(50)] public string? Last_Undo_By { get; set; }
    public DateTime? Last_Undo_At { get; set; }

    public bool Is_Posted { get; set; }
    public long? Journal_Entry_ID { get; set; }
    [MaxLength(50)] public string? Posted_By_User_ID { get; set; }
    public DateTime? Posted_At { get; set; }
    [MaxLength(50)] public string? Unposted_By { get; set; }
    public DateTime? Unposted_At { get; set; }
    [MaxLength(500)] public string? Unpost_Reason { get; set; }

    public bool Is_Active { get; set; } = true;
    [MaxLength(50)] public string? Created_By { get; set; }
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
    [MaxLength(50)] public string? Updated_By { get; set; }
    public DateTime? Updated_At { get; set; }

    public ICollection<FinancialVoucherDetail> Details { get; set; } = new List<FinancialVoucherDetail>();
    public ICollection<DocumentAllocation> DocumentAllocations { get; set; } = new List<DocumentAllocation>();
    public ICollection<VoucherActionLog> VoucherActionLogs { get; set; } = new List<VoucherActionLog>();
}
