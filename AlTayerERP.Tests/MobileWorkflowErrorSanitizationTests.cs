using Xunit;

namespace AlTayerERP.Tests;

/// <summary>
/// يتحقق من أن مسارات MAUI الحرجة لا تعرض نص استجابة HTTP غير المنظم للمستخدم.
/// </summary>
public sealed class MobileWorkflowErrorSanitizationTests
{
    [Fact]
    public void Financial_workflow_services_use_the_shared_safe_api_message_formatter()
    {
        var root = FindRepositoryRoot();
        var services = new[]
        {
            "ApprovalRequestsService.cs",
            "PaymentRequestService.cs",
            "VoucherWorkflowService.cs"
        };

        foreach (var service in services)
        {
            var source = File.ReadAllText(Path.Combine(root, "AlTayerERP.Mobile.Office", "Services", service));
            Assert.Contains("MobileApiErrorHandler.FromPayload", source, StringComparison.Ordinal);
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار رسائل MAUI الآمنة.");
    }
}
