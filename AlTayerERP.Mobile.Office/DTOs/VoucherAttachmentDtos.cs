namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class VoucherAttachmentDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Bytes { get; set; }
    public string? Notes { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public bool Active { get; set; }

    public string SizeDisplay => Bytes < 1024 * 1024
        ? $"{Bytes / 1024d:N1} KB"
        : $"{Bytes / 1024d / 1024d:N1} MB";
}
