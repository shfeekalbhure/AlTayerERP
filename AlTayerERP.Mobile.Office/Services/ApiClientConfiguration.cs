namespace AlTayerERP.Mobile.Office.Services;

/// <summary>مصدر العنوان الوحيد لتطبيق الجوال؛ لا يغيّر عنوان الإنتاج الحالي.</summary>
public static class ApiClientConfiguration
{
    private const string DevelopmentBaseAddress = "http://127.0.0.1:5021/";
    private const string ProductionBaseAddress = "http://127.0.0.1:5021/";

    public static Uri BaseAddress => new(
#if DEBUG
        DevelopmentBaseAddress
#else
        ProductionBaseAddress
#endif
    );
}
