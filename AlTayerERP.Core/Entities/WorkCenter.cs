using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>مركز عمل تشغيلي/إداري ضمن شركة وفرع اختياري.</summary>
[Table("work_centers")]
public class WorkCenter
{
    [Key]
    [Column("Work_Center_ID")]
    public long Work_Center_ID { get; set; }

    [Required, MaxLength(50)]
    [Column("Company_ID")]
    public string Company_ID { get; set; } = string.Empty;

    [Column("Branch_ID")]
    public int? Branch_ID { get; set; }

    [Required, MaxLength(30)]
    [Column("Work_Center_Code")]
    public string Work_Center_Code { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Column("Work_Center_Name_AR")]
    public string Work_Center_Name_AR { get; set; } = string.Empty;

    [MaxLength(150)]
    [Column("Work_Center_Name_EN")]
    public string? Work_Center_Name_EN { get; set; }

    [Required, MaxLength(30)]
    [Column("Work_Center_Type")]
    public string Work_Center_Type { get; set; } = string.Empty;

    [Column("Parent_Work_Center_ID")]
    public long? Parent_Work_Center_ID { get; set; }

    [Column("Is_Active")]
    public bool Is_Active { get; set; } = true;

    [Column("Sort_Order")]
    public int Sort_Order { get; set; }

    [Column("Created_At")]
    public DateTime Created_At { get; set; } = DateTime.UtcNow;

    [Column("Created_By")]
    public int? Created_By { get; set; }

    [Column("Updated_At")]
    public DateTime? Updated_At { get; set; }

    [Column("Updated_By")]
    public int? Updated_By { get; set; }
}