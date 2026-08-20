using Xunit;

namespace AlTayerERP.Tests;

/// <summary>
/// يحمي عقد العرض المختصر من إعادة الحقول التشغيلية الحساسة إلى عميل لا يحتاجها.
/// </summary>
public sealed class FinancialVoucherSummaryContractTests
{
    [Fact]
    public void Summary_endpoint_uses_a_dedicated_contract_and_excludes_internal_audit_fields()
    {
        var root = FindRepositoryRoot();
        var controllerPath = Path.Combine(root, "AlTayerERP.API", "Controllers", "FinancialVoucherController.cs");
        var dtoPath = Path.Combine(root, "AlTayerERP.API", "DTOs", "Accounting", "FinancialVoucherSummaryDto.cs");

        var controllerSource = File.ReadAllText(controllerPath);
        Assert.True(File.Exists(dtoPath), "يجب تعريف FinancialVoucherSummaryDto لعقد العرض المختصر.");
        Assert.Contains("[HttpGet(\"{voucherId:long}/summary\")]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("FinancialVoucherSummaryDto", controllerSource, StringComparison.Ordinal);

        var dtoSource = File.ReadAllText(dtoPath);
        foreach (var forbidden in new[]
                 {
                     "Created_By", "Created_At", "Updated_By", "Updated_At",
                     "Reviewed_By_User_ID", "Review_Notes", "Last_Printed_By",
                     "Last_Undo_By", "Edit_Count", "Print_Count", "Undo_Count"
                 })
        {
            Assert.DoesNotContain(forbidden, dtoSource, StringComparison.Ordinal);
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار عقد العرض المختصر.");
    }
}
