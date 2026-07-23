using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>
/// ربط المستخدم بالأدوار (User_Roles). يسمح بتعدد الأدوار مع فترة سريان
/// بدون حذف السجل التاريخي.
/// </summary>
[Table("user_roles")]
public class UserRole
{
    /// <summary>المعرف الداخلي للربط.</summary>
    [Key, Column("User_Role_ID")]
    public long User_Role_ID { get; set; }

    /// <summary>مستخدم النظام User_ID صاحب الدور.</summary>
    [Column("User_ID")]
    public int User_ID { get; set; }

    /// <summary>الدور Role_ID الممنوح للمستخدم.</summary>
    [Column("Role_ID")]
    public int Role_ID { get; set; }

    /// <summary>تاريخ بدء سريان الدور Effective_From.</summary>
    [Column("Effective_From")]
    public DateTime Effective_From { get; set; } = DateTime.UtcNow;

    /// <summary>تاريخ انتهاء السريان، وNull يعني مستمراً.</summary>
    [Column("Effective_To")]
    public DateTime? Effective_To { get; set; }

    /// <summary>هل الربط مفعل حالياً Is_Active.</summary>
    [Column("Is_Active")]
    public bool Is_Active { get; set; } = true;

    /// <summary>من منح الدور Granted_By.</summary>
    [Column("Granted_By")]
    public int? Granted_By { get; set; }

    /// <summary>سبب المنح أو الإيقاف Reason.</summary>
    [Column("Reason")]
    [MaxLength(500)]
    public string? Reason { get; set; }
}