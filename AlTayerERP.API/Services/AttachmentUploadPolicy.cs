using Microsoft.AspNetCore.Http;

namespace AlTayerERP.API.Services;

/// <summary>
/// سياسة الرفع المعتمدة لمرفقات طلبات الصرف. لا تثق بنوع MIME الذي يرسله العميل؛
/// بل تقارن الامتداد مع البصمة الثنائية وتحدد نوع التنزيل من الخادم.
/// </summary>
public static class AttachmentUploadPolicy
{
    private const int SignatureLength = 8;

    private static readonly IReadOnlyDictionary<string, string> AllowedExtensions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg"
        };

    public static async Task<AttachmentUploadValidationResult> ValidateAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(Path.GetFileName(file.FileName));
        if (!AllowedExtensions.TryGetValue(extension, out var contentType))
            return AttachmentUploadValidationResult.Reject("يسمح فقط برفع ملفات PDF أو صور PNG وJPG/JPEG.");

        await using var stream = file.OpenReadStream();
        var header = new byte[SignatureLength];
        var read = await stream.ReadAsync(header.AsMemory(0, SignatureLength), cancellationToken);

        if (!MatchesExtensionSignature(extension, header.AsSpan(0, read)))
            return AttachmentUploadValidationResult.Reject("محتوى الملف لا يطابق نوعه المسموح.");

        return AttachmentUploadValidationResult.Accept(contentType);
    }

    private static bool MatchesExtensionSignature(string extension, ReadOnlySpan<byte> header) =>
        extension.ToLowerInvariant() switch
        {
            ".pdf" => header.Length >= 5 && header[..5].SequenceEqual("%PDF-"u8),
            ".png" => header.Length >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".jpg" or ".jpeg" => header.Length >= 3 && header[..3].SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }),
            _ => false
        };
}

public sealed record AttachmentUploadValidationResult(bool IsValid, string Message, string? ContentType)
{
    public static AttachmentUploadValidationResult Accept(string contentType) =>
        new(true, string.Empty, contentType);

    public static AttachmentUploadValidationResult Reject(string message) =>
        new(false, message, null);
}
