namespace AlTayerERP.API.DTOs;

/// <summary>
/// عقد خطأ متوافق: يبقى الحقل Message للعملاء الحاليين، وتضاف حقول ثابتة
/// ليتمكن العملاء الأحدث من معالجة الخطأ وإحالته للدعم بأمان.
/// </summary>
public sealed record ApiErrorResponse(string Code, string Message, string CorrelationId)
{
    public bool Success => false;
}
