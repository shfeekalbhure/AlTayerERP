namespace AlTayerERP.API.Contracts;

/// <summary>
/// عقد تشخيص عام وآمن يثبت نسخة API وقاعدة البيانات التي اتصل بها الخادم.
/// لا يحتوي على نص الاتصال أو بيانات اعتماد.
/// </summary>
public sealed record ApiHealthResponse(
    string Api,
    string Database,
    string Environment,
    string? DatabaseName)
{
    public bool IsReady =>
        string.Equals(Api, "ready", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(Database, "ready", StringComparison.OrdinalIgnoreCase);
}
