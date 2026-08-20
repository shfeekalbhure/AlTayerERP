using Xunit;

namespace AlTayerERP.Tests;

/// <summary>
/// يتحقق على Linux من عقد العرض في WinForms الذي لا يمكن تشغيله في بيئة المراجعة.
/// </summary>
public sealed class DesktopApprovalErrorPresentationTests
{
    [Fact]
    public void ApprovalDecision_does_not_show_raw_api_error_body()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(),
            "AlTayerERP.Desktop", "FrmApprovalRequests.cs"));

        Assert.DoesNotContain(
            "MessageBox.Show(await response.Content.ReadAsStringAsync()",
            source,
            StringComparison.Ordinal);
        Assert.Contains("ApiErrorMessageFormatter", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار عرض أخطاء سطح المكتب.");
    }
}
