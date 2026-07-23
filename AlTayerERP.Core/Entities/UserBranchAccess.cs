using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>
/// نطاق الفروع المسموح للمستخدم (Branch_Access) داخل الشركة.
/// </summary>
[Table("branch_access")]
public class UserBranchAccess
{
    /// <summary>المعرف الداخلي لنطاق الفرع.</summary>
    [Key, Column("Branch_Access_ID")]
    public long Branch_Access_ID { get; set; }

    /// <summary>المستخدم User_ID.</summary>
    [Column("User_ID")]
    public int User_ID { get; set; }

    /// <summary>رمز الشركة Company_ID لعزل الفرع مؤسسياً.</summary>
    [Column("Company_ID")]
    [MaxLength(50)]
    public string Company_ID { get; set; } = string.Empty;

    /// <summary>الفرع Branch_ID المسموح.</summary>
    [Column("Branch_ID")]
    public int Branch_ID { get; set; }

    /// <summary>الحالة التشغيلية Is_Active.</summary>
    [Column("Is_Active")]
    public bool Is_Active { get; set; } = true;

    /// <summary>بداية السريان Effective_From.</summary>
    [Column("Effective_From")]
    public DateTime Effective_From { get; set; } = DateTime.UtcNow;

    /// <summary>نهاية السريان Effective_To.</summary>
    [Column("Effective_To")]
    public DateTime? Effective_To { get; set; }
}