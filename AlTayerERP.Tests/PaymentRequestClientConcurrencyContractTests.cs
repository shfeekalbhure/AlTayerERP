using Xunit;

namespace AlTayerERP.Tests;

public sealed class PaymentRequestClientConcurrencyContractTests
{
    [Fact]
    public void PaymentRequest_Clients_MustRetainAndResend_RowVersion()
    {
        var root = FindSolutionRoot();
        var mobileDtos = File.ReadAllText(Path.Combine(root,
            "AlTayerERP.Mobile.Office", "DTOs", "PaymentRequestDtos.cs"));
        var mobileEditor = File.ReadAllText(Path.Combine(root,
            "AlTayerERP.Mobile.Office", "NewPaymentRequestPage.xaml.cs"));
        var desktopEditor = File.ReadAllText(Path.Combine(root,
            "AlTayerERP.Desktop", "FrmPaymentRequest.cs"));

        Assert.True(
            Count(mobileDtos, "public Guid? RowVersion { get; set; }") >= 2,
            "يجب أن يستقبل نموذج العرض ونموذج الحفظ في MAUI رمز النسخة الاختياري.");
        Assert.Contains("RowVersion = _editingRequest.RowVersion", mobileEditor);
        Assert.Contains("_rowVersion", desktopEditor);
        Assert.Contains("RowVersion = _rowVersion", desktopEditor);
    }

    private static int Count(string value, string fragment)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(fragment, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += fragment.Length;
        }

        return count;
    }

    private static string FindSolutionRoot()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current != null; current = current.Parent)
        {
            if (File.Exists(Path.Combine(current.FullName, "AlTayerERP.sln")))
                return current.FullName;
        }

        throw new DirectoryNotFoundException("تعذر العثور على جذر حل AlTayerERP.");
    }
}
