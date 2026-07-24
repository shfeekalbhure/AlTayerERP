namespace AlTayerERP.API.DTOs;

/// <summary>بيانات إنشاء أو تعديل فرع. لا تتضمن حقول التدقيق أو الحالة التشغيلية.</summary>
public sealed class CreateBranchDto
{
    public string Company_ID { get; set; } = string.Empty;
    public string Branch_Code { get; set; } = string.Empty;
    public string Branch_Name { get; set; } = string.Empty;
    public string Branch_Name_EN { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Branch_Type { get; set; } = string.Empty;
    public int? Parent_Branch_ID { get; set; }

    public int? Country_ID { get; set; }
    public int? Governorate_ID { get; set; }
    public int? City_ID { get; set; }

    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Manager_Name { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool Allow_Credit { get; set; }
    public bool Allow_Percentage { get; set; }

    /// <summary>يجب اختياره صراحة من عملات الشركة النشطة؛ لا توجد قيمة افتراضية رقمية.</summary>
    public int Currency_ID { get; set; }
}
