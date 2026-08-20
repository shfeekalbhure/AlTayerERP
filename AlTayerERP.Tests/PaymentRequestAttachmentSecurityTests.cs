using AlTayerERP.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class PaymentRequestAttachmentSecurityTests
{
    [Fact]
    public void Upload_does_not_trust_the_client_declared_content_type_without_server_validation()
    {
        var root = FindRepositoryRoot();
        var controller = File.ReadAllText(Path.Combine(
            root,
            "AlTayerERP.API",
            "Controllers",
            "PaymentRequestAttachmentsController.cs"));

        Assert.Contains("AttachmentUploadPolicy.ValidateAsync", controller, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Content_Type = string.IsNullOrWhiteSpace(file.ContentType) ? \"application/octet-stream\" : file.ContentType",
            controller,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("invoice.pdf", "image/png", "%PDF-1.7", true, "application/pdf")]
    [InlineData("proof.PNG", "application/octet-stream", "\u0089PNG\r\n\u001a\n", true, "image/png")]
    [InlineData("photo.jpg", "application/pdf", "\u00ff\u00d8\u00ff\u00e0", true, "image/jpeg")]
    [InlineData("spoofed.pdf", "application/pdf", "not-a-pdf", false, null)]
    [InlineData("script.exe", "image/jpeg", "MZ", false, null)]
    public async Task Upload_policy_accepts_only_supported_extensions_with_matching_binary_signatures(
        string fileName,
        string clientContentType,
        string content,
        bool expectedValid,
        string? expectedContentType)
    {
        var bytes = content.Select(c => (byte)c).ToArray();
        await using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary { ["Content-Type"] = new StringValues(clientContentType) }
        };

        var result = await AttachmentUploadPolicy.ValidateAsync(file);

        Assert.Equal(expectedValid, result.IsValid);
        Assert.Equal(expectedContentType, result.ContentType);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار سياسة مرفقات طلبات الصرف.");
    }
}
