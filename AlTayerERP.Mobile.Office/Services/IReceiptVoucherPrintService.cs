using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public interface IReceiptVoucherPrintService
{
    Task PrintAsync(
        ReceiptVoucherDetailsDto voucher,
        VoucherPdfExportOptions? options = null,
        CancellationToken cancellationToken = default);
    Task<ReceiptVoucherPdfExportResult> ExportPdfAsync(
        ReceiptVoucherDetailsDto voucher,
        VoucherPdfExportOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>نتيجة حفظ سند القبض كملف PDF في ذاكرة الهاتف.</summary>
public sealed record ReceiptVoucherPdfExportResult(string FileName, string ContentUri);

/// <summary>
/// إعدادات الطباعة الخاصة بنوع السند. تظل القيم الافتراضية لسند القبض،
/// بينما يمرر سند الصرف مجلده وتسمياته المستقلة.
/// </summary>
public sealed record VoucherPdfExportOptions(
    string DownloadsFolder,
    string PartyLabel,
    string RecipientSignatureLabel,
    string AccountantSignatureLabel = "المحاسب",
    string ApproverSignatureLabel = "المعتمد")
{
    public static VoucherPdfExportOptions ReceiptVoucher { get; } = new(
        "ReceiptVouchers", "استلمنا من", "المستلم منه");

    public static VoucherPdfExportOptions PaymentVoucher { get; } = new(
        "PaymentVouchers", "المستفيد", "المستلم");
}
