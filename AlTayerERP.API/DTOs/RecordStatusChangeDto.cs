namespace AlTayerERP.API.DTOs;

/// <summary>
/// طلب إيقاف أو إعادة تفعيل سجل مرجعي.
/// Reason إلزامي حتى تكون العملية قابلة للمراجعة في Audit_Logs.
/// </summary>
public class RecordStatusChangeDto
{
    /// <summary>سبب الإيقاف أو إعادة التفعيل، ولا يملؤه النظام تلقائياً.</summary>
    public string Reason { get; set; } = string.Empty;
}