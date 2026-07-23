using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>
/// نطاق الشركات المسموح للمستخدم (Company_Access). لا يكفي الدور وحده
/// للوصول إلى بيانات شركة أخرى.
/// </summary>
[Table("company_access")]
public class UserCompanyAccess
{
    /// <summary>المعرف الداخلي لنطاق الشركة.</summary>
    [Key, Column("Company_Access_ID")]
    public long Company_Access_ID { get; set; }

    /// <summary>المستخدم User_ID.</summary>
    [Column("User_ID")]
    public int User_ID { get; set; }

    /// <summary>رمز الشركة Company_ID.</summary>
    [Column("Company_ID")]
    [MaxLength(50)]
    public string Company_ID { get; set; } = string.Empty;

    /// <summary>هل النطاق نشط Is_Active.</summary>
    [Column("Is_Active")]
    public bool Is_Active { get; set; } = true;

    /// <summary>منح النطاق يبدأ من Effective_From.</summary>
    [Column("Effective_From")]
    public DateTime Effective_From { get; set; } = DateTime.UtcNow;

    /// <summary>نهاية النطاق Effective_To.</summary>
    [Column("Effective_To")]
    public DateTime? Effective_To { get; set; }
}