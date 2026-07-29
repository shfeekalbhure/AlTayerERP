using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>الفرع التشغيلي التابع لشركة، بمعرف رقمي وكود أعمال مستقل.</summary>
[Table("tenant_branches")]
public class TenantBranch
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Branch_ID { get; set; }

    [Required, MaxLength(50)] public string Company_ID { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Branch_Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Branch_Name { get; set; } = string.Empty;
    [MaxLength(150)] public string? Branch_Name_EN { get; set; }
    [MaxLength(500)] public string? Address { get; set; }

    /// <summary>كود نوع الفرع المرجعي؛ يبقى نصيًا ككود أعمال.</summary>
    [MaxLength(30)] public string? Branch_Type { get; set; }
    public int? Parent_Branch_ID { get; set; }

    public int? Country_ID { get; set; }
    public int? Governorate_ID { get; set; }
    public int? City_ID { get; set; }

    [MaxLength(50)] public string? Phone { get; set; }
    [MaxLength(50)] public string? Mobile { get; set; }
    [MaxLength(150)] public string? Email { get; set; }
    [MaxLength(250)] public string? Website { get; set; }
    [MaxLength(150)] public string? Manager_Name { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    public bool Allow_Credit { get; set; }
    public bool Allow_Percentage { get; set; }
    public bool Is_Active { get; set; } = true;
    public int Currency_ID { get; set; }

    public DateTime Created_Date { get; set; } = DateTime.UtcNow;
    public DateTime? Updated_Date { get; set; }
    public int? Created_By { get; set; }
    public int? Updated_By { get; set; }
    public int Edit_Count { get; set; }
    public int? Stopped_By { get; set; }
    public DateTime? Stopped_At { get; set; }
    [MaxLength(500)] public string? Stopped_Reason { get; set; }
    public int? Reactivated_By { get; set; }
    public DateTime? Reactivated_At { get; set; }
    [MaxLength(500)] public string? Reactivate_Reason { get; set; }

    public TenantBranch? ParentBranch { get; set; }
}
