using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public interface IReceiptVoucherPrintService
{
    Task PrintAsync(ReceiptVoucherDetailsDto voucher, CancellationToken cancellationToken = default);
    Task<ReceiptVoucherPdfExportResult> ExportPdfAsync(ReceiptVoucherDetailsDto voucher, CancellationToken cancellationToken = default);
}

/// <summary>نتيجة حفظ سند القبض كملف PDF في ذاكرة الهاتف.</summary>
public sealed record ReceiptVoucherPdfExportResult(string FileName, string ContentUri);
