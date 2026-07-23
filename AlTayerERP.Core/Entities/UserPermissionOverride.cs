using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>
/// استثناء صلاحية لمستخدم (User_Permission_Overrides). تأثير المنع Deny
/// يتقدم على المنح Allow لمنع الامتيازات المتضاربة.
/// </summary>
[Table("user_permission_overrides")]
public class UserPermissionOverride
{
    /// <summary>المعرف الداخلي للاستثناء.</summary>
    [Key, Column("User_Permission_Override_ID")]
    public long User_Permission_Override_ID { get; set; }

    /// <summary>المستخدم User_ID الذي يخصه الاستثناء.</summary>
    [Column("User_ID")]
    public int User_ID { get; set; }

    /// <summary>الصلاحية القياسية System_Permission_ID.</summary>
    [Column("System_Permission_ID")]
    public int System_Permission_ID { get; set; }

    /// <summary>نوع الأثر Effect: Allow أو Deny.</summary>
    [Column("Effect")]
    [MaxLength(10)]
    public string Effect { get; set; } = "Deny";

    /// <summary>تاريخ بدء سريان الاستثناء Effective_From.</summary>
    [Column("Effective_From")]
    public DateTime Effective_From { get; set; } = DateTime.UtcNow;

    /// <summary>تاريخ انتهاء سريان الاستثناء Effective_To.</summary>
    [Column("Effective_To")]
    public DateTime? Effective_To { get; set; }

    /// <summary>الحالة التشغيلية Is_Active.</summary>
    [Column("Is_Active")]
    public bool Is_Active { get; set; } = true;

    /// <summary>سبب الاستثناء الإلزامي للمراجعة.</summary>
    [Column("Reason")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>من أنشأ الاستثناء Created_By.</summary>
    [Column("Created_By")]
    public int? Created_By { get; set; }

    /// <summary>وقت إنشاء الاستثناء Created_At.</summary>
    [Column("Created_At")]
    public DateTime Created_At { get; set; } = DateTime.UtcNow;
}