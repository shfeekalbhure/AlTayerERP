using Xunit;

namespace AlTayerERP.Tests;

public sealed class DatabaseMigrationSafetyContractTests
{
    [Fact]
    public void RowVersion_scripts_are_limited_to_a_test_database_and_never_instruct_direct_production_execution()
    {
        var root = FindRepositoryRoot();
        var scriptsDirectory = Path.Combine(root, "Database", "Scripts");
        var paymentRequestScript = File.ReadAllText(Path.Combine(scriptsDirectory, "20260821_add_payment_request_row_version.sql"));
        var voucherScript = File.ReadAllText(Path.Combine(scriptsDirectory, "20260820_add_financial_voucher_row_version.sql"));

        Assert.Contains("نسخة اختبار", paymentRequestScript, StringComparison.Ordinal);
        Assert.Contains("تجريبية", voucherScript, StringComparison.Ordinal);
        Assert.DoesNotContain("ثم في نافذة صيانة الإنتاج", paymentRequestScript, StringComparison.Ordinal);
        Assert.Contains("لا يطبق مباشرة على الإنتاج", voucherScript, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار سلامة ترحيلات قاعدة البيانات.");
    }
}
