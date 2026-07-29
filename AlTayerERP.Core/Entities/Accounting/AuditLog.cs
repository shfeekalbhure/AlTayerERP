using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting;

/// <summary>سجل تدقيق مركزي يحفظ العملية دون أسرار حساسة.</summary>
[Table("audit_logs")]
public class AuditLog
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Audit_ID { get; set; }

    public int? Module_ID { get; set; }
    [Required, MaxLength(100)] public string Table_Name { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Record_ID { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string Action_Type { get; set; } = string.Empty;

    /// <summary>معرف المستخدم النصي التاريخي؛ يحسم تحويله إلى FK رقمي في جولة مستقلة.</summary>
    [MaxLength(50)] public string? User_ID { get; set; }

    /// <summary>معرف الفرع الفعلي إن توفر، موحد كرقم صحيح قابل للفراغ.</summary>
    public int? Branch_ID { get; set; }

    public DateTime Action_At { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "json")] public string? Old_Values { get; set; }
    [Column(TypeName = "json")] public string? New_Values { get; set; }
    [Required, MaxLength(20)] public string Action_Channel { get; set; } = "DESKTOP";
    [MaxLength(150)] public string? Device_Name { get; set; }
    [MaxLength(50)] public string? IP_Address { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}
