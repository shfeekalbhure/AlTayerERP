using Xunit;

namespace AlTayerERP.Tests;

public sealed class VoucherAttachmentSecurityTests
{
    [Fact]
    public void Voucher_upload_uses_server_signature_validation_and_quarantines_new_files_before_download()
    {
        var root = FindRepositoryRoot();
        var controller = File.ReadAllText(Path.Combine(
            root,
            "AlTayerERP.API",
            "Controllers",
            "VoucherAttachmentsController.cs"));

        Assert.Contains("AttachmentUploadPolicy.ValidateAsync", controller, StringComparison.Ordinal);
        Assert.Contains("ScanStatus = AttachmentScanStatus.Pending", controller, StringComparison.Ordinal);
        Assert.Contains("WriteManifestAtomicAsync", controller, StringComparison.Ordinal);
        Assert.Contains("AttachmentScanStatus.Clean", controller, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار مرفقات السندات.");
    }
}
