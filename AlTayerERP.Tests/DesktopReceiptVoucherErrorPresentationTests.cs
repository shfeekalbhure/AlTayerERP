using Xunit;

namespace AlTayerERP.Tests;

/// <summary>
/// يتحقق سكونياً من عقد عرض أخطاء سند القبض لأن WinForms غير قابل للتشغيل في Linux.
/// </summary>
public sealed class DesktopReceiptVoucherErrorPresentationTests
{
    [Fact]
    public void Receipt_voucher_save_paths_do_not_preserve_raw_api_body_as_message()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "AlTayerERP.Desktop",
            "Accounting", "ReceiptVoucher", "FrmReceiptVoucher.Save.cs"));

        Assert.DoesNotContain(": rawMessage", source, StringComparison.Ordinal);
        Assert.Contains("ApiErrorMessageFormatter.FromPayload", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار رسالة خطأ سند القبض.");
    }
}
