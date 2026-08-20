using Xunit;

namespace AlTayerERP.Tests;

/// <summary>
/// يتحقق من أن الخدمات التي تقرأ الجلسة لا تستعمل رمز وصول منتهياً خلال عمل التطبيق.
/// الاختبار سكوني لأن MAUI Android لا يُبنى في Linux.
/// </summary>
public sealed class MobileActiveSessionRefreshContractTests
{
    [Fact]
    public void Session_storage_refreshes_expired_access_token_once_for_concurrent_callers()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(root,
            "AlTayerERP.Mobile.Office", "Services", "SessionStorageService.cs"));

        Assert.Contains("api/Auth/Refresh", source, StringComparison.Ordinal);
        Assert.Contains("SemaphoreSlim", source, StringComparison.Ordinal);
        Assert.Contains("Refresh_Token = stored.RefreshToken", source, StringComparison.Ordinal);
        Assert.Contains("Device_ID = await deviceIdentity.GetAsync()", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار تجديد جلسة MAUI النشطة.");
    }
}
