using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>
/// حد اعتماد مالي (Approval_Limits) حسب المستخدم أو الدور ونطاق الشركة/الفرع.
/// </summary>
[Table("approval_limits")]
public class ApprovalLimit
{
    /// <summary>المعرف الداخلي للحد.</summary>
    [Key, Column("Approval_Limit_ID")]
    public long Approval_Limit_ID { get; set; }

    /// <summary>المستخدم User_ID، فارغ عند اعتماد الحد على الدور.</summary>
    [Column("User_ID")]
    public int? User_ID { get; set; }

    /// <summary>الدور Role_ID، فارغ عند تخصيص الحد لمستخدم.</summary>
    [Column("Role_ID")]
    public int? Role_ID { get; set; }

    /// <summary>رمز الشركة Company_ID، فارغ يعني كل الشركات المسموح بها.</summary>
    [Column("Company_ID")]
    [MaxLength(50)]
    public string? Company_ID { get; set; }

    /// <summary>الفرع Branch_ID، فارغ يعني كل الفروع المسموح بها.</summary>
    [Column("Branch_ID")]
    public int? Branch_ID { get; set; }

    /// <summary>نوع المستند Voucher_Type_Code مثل ReceiptVoucher.</summary>
    [Column("Voucher_Type_Code")]
    [MaxLength(50)]
    public string? Voucher_Type_Code { get; set; }

    /// <summary>العملة Currency_ID للحد.</summary>
    [Column("Currency_ID")]
    public int? Currency_ID { get; set; }

    /// <summary>أقصى مبلغ محلي يمكن اعتماده Max_Amount_Local.</summary>
    [Column("Max_Amount_Local")]
    public decimal Max_Amount_Local { get; set; }

    /// <summary>الحالة التشغيلية Is_Active.</summary>
    [Column("Is_Active")]
    public bool Is_Active { get; set; } = true;

    /// <summary>بداية سريان الحد Effective_From.</summary>
    [Column("Effective_From")]
    public DateTime Effective_From { get; set; } = DateTime.UtcNow;

    /// <summary>نهاية سريان الحد Effective_To.</summary>
    [Column("Effective_To")]
    public DateTime? Effective_To { get; set; }
}