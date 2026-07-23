namespace AlTayerERP.API.DTOs;

/// <summary>
/// طلب تقييم صلاحية من الخادم. لا يتضمن User_ID لأن هوية المستخدم تؤخذ
/// من جلسة الخادم فقط.
/// </summary>
public class PermissionEvaluationRequestDto
{
    /// <summary>رمز الصلاحية، مثال ReceiptVoucher.Approve.</summary>
    public string Permission_Code { get; set; } = string.Empty;

    /// <summary>رمز الشركة المطلوب الوصول إليها؛ اختياري عند فحص صلاحية عامة.</summary>
    public string? Company_ID { get; set; }

    /// <summary>الفرع المطلوب؛ اختياري عند فحص صلاحية عامة.</summary>
    public int? Branch_ID { get; set; }

    /// <summary>المبلغ المحلي لتطبيق حد الاعتماد عند الحاجة.</summary>
    public decimal? Amount_Local { get; set; }

    /// <summary>نوع المستند لتخصيص حد الاعتماد.</summary>
    public string? Voucher_Type_Code { get; set; }
}