using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.API.DTOs;

/// <summary>
/// بيانات إنشاء أو تعديل فرع. لا تتضمن حقول الحالة أو التدقيق.
/// الدولة والمحافظة لا تستقبلان من العميل؛ يستخرجهما الخادم من المدينة المختارة.
/// </summary>
public sealed class CreateBranchDto
{
    [Required(ErrorMessage = "الشركة مطلوبة.")]
    public string Company_ID { get; set; } = string.Empty;

    /// <summary>يمكن تركه فارغاً عند الإنشاء ليولده نظام الترقيم مركزياً.</summary>
    public string Branch_Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم الفرع بالعربية مطلوب.")]
    public string Branch_Name { get; set; } = string.Empty;

    public string Branch_Name_EN { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    /// <summary>كود أو اسم نوع فرع نشط قادم من جدول branch_types.</summary>
    [Required(ErrorMessage = "نوع الفرع مطلوب.")]
    public string Branch_Type { get; set; } = string.Empty;

    public int? Parent_Branch_ID { get; set; }

    /// <summary>
    /// المدينة المرجعية المختارة. يستخرج الخادم منها Country_ID وGovernorate_ID
    /// لضمان عدم إرسال تسلسل جغرافي متعارض من الواجهة.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "المدينة مطلوبة.")]
    public int? City_ID { get; set; }

    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Manager_Name { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool Allow_Credit { get; set; }
    public bool Allow_Percentage { get; set; }

    /// <summary>عملة نشطة تابعة للشركة المختارة؛ لا توجد قيمة ثابتة افتراضية.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "العملة الافتراضية للفرع مطلوبة.")]
    public int Currency_ID { get; set; }
}
