using Xunit;

namespace AlTayerERP.Tests;

/// <summary>
/// بيئة Linux لا تبني MAUI Android، لذا يتحقق هذا الاختبار من بقاء عقد الاستعادة
/// في مصدر العميل متصلاً بمسار Auth/Refresh الذي يتحقق منه الخادم أيضاً.
/// </summary>
public sealed class MobileAuthenticationRefreshContractTests
{
    [Fact]
    public void RestoreSession_uses_refresh_contract_before_discarding_expired_access_token()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(),
            "AlTayerERP.Mobile.Office", "Services", "AuthenticationService.cs"));

        Assert.Contains("api/Auth/Refresh", source, StringComparison.Ordinal);
        Assert.Contains("Refresh_Token", source, StringComparison.Ordinal);
        Assert.Contains("RefreshTokenExpiresAt", source, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "stored.AccessTokenExpiresAt <= DateTimeOffset.UtcNow) { sessionStorage.Clear(); return null; }",
            source,
            StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("لم يتعذر تحديد جذر مستودع AlTayerERP لاختبار عقد MAUI.");
    }
}
