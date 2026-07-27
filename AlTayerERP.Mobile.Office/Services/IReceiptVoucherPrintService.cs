using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public interface IReceiptVoucherPrintService
{
    Task PrintAsync(ReceiptVoucherDetailsDto voucher, CancellationToken cancellationToken = default);
}
