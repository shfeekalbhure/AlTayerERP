using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities;

/// <summary>حالة اعتماد مرجعية ثابتة تستخدم في دورات الموافقة.</summary>
[Table("approval_statuses")]
public sealed class ApprovalStatusReference
{
    [Key] public int Approval_Status_ID { get; set; }
    [Required, MaxLength(30)] public string Approval_Status_Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Approval_Status_Name_AR { get; set; } = string.Empty;
    [MaxLength(100)] public string? Approval_Status_Name_EN { get; set; }
    public int Sort_Order { get; set; }
    public bool Is_Active { get; set; } = true;
}

/// <summary>نوع مستند معتمد للترقيم المركزي.</summary>
[Table("numbering_document_types")]
public sealed class NumberingDocumentType
{
    [Key] public int Numbering_Document_Type_ID { get; set; }
    [Required, MaxLength(50)] public string Document_Type_Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Document_Type_Name_AR { get; set; } = string.Empty;
    [MaxLength(150)] public string? Document_Type_Name_EN { get; set; }
    public bool Is_Active { get; set; } = true;
    public int Sort_Order { get; set; }
}
