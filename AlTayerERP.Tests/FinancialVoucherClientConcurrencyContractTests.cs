using Xunit;

namespace AlTayerERP.Tests;

/// <summary>
/// يثبت أن عقد السند المالي يعيد رمز النسخة الذي يحتاجه العميل لإجراء تحديث متزامن آمن.
/// </summary>
public sealed class FinancialVoucherClientConcurrencyContractTests
{
    [Fact]
    public void Voucher_response_contract_exposes_row_version_for_a_follow_up_update()
    {
        var root = FindRepositoryRoot();
        var dtoPath = Path.Combine(
            root,
            "AlTayerERP.API",
            "DTOs",
            "Accounting",
            "FinancialVoucherResponseDto.cs");

        var source = File.ReadAllText(dtoPath);

        Assert.Contains("public Guid RowVersion", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار عقد تزامن السند المالي.");
    }
}
