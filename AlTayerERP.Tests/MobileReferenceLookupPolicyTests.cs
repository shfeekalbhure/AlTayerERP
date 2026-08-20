using AlTayerERP.API.Services;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class MobileReferenceLookupPolicyTests
{
    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 1)]
    [InlineData(25, 25)]
    [InlineData(50, 50)]
    [InlineData(500, 50)]
    public void NormalizeLimit_ClampsLookupResultsToSafeRange(int requested, int expected)
    {
        Assert.Equal(expected, MobileReferenceLookupPolicy.NormalizeLimit(requested));
    }

    [Fact]
    public void NormalizeSearch_TrimsAndLimitsInputLength()
    {
        var normalized = MobileReferenceLookupPolicy.NormalizeSearch("  حساب " + new string('أ', 100) + "  ");

        Assert.Equal(80, normalized.Length);
        Assert.StartsWith("حساب", normalized, StringComparison.Ordinal);
    }

    [Fact]
    public void LookupController_ProvidesSeparateBoundedEndpoint()
    {
        var sourcePath = Path.Combine(FindRepositoryRoot(), "AlTayerERP.API", "Controllers", "MobilePaymentRequestReferencesController.cs");
        var source = File.ReadAllText(sourcePath);

        Assert.Contains("[HttpGet(\"lookup\")]", source, StringComparison.Ordinal);
        Assert.Contains("MobileReferenceLookupPolicy.NormalizeLimit", source, StringComparison.Ordinal);
        Assert.Contains(".Take(limit + 1)", source, StringComparison.Ordinal);
    }

    [Fact]
    public void NewPaymentRequestPage_UsesBoundedLookupForAccountsAndCostCenters()
    {
        var root = FindRepositoryRoot();
        var pageSource = File.ReadAllText(Path.Combine(root, "AlTayerERP.Mobile.Office", "NewPaymentRequestPage.xaml.cs"));
        var xamlSource = File.ReadAllText(Path.Combine(root, "AlTayerERP.Mobile.Office", "NewPaymentRequestPage.xaml"));

        Assert.Contains("includeLookupData: false", pageSource, StringComparison.Ordinal);
        Assert.Contains("SelectLookupReferenceAsync", pageSource, StringComparison.Ordinal);
        Assert.Contains("\"accounts\"", pageSource, StringComparison.Ordinal);
        Assert.Contains("\"cost-centers\"", pageSource, StringComparison.Ordinal);
        Assert.Contains("OnSelectAccountClicked", xamlSource, StringComparison.Ordinal);
        Assert.Contains("OnSelectCostCenterClicked", xamlSource, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار عقد lookup.");
    }
}
