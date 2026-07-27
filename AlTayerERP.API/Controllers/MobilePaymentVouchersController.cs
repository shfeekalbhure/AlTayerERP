using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mobile/payment-vouchers")]
public sealed class MobilePaymentVouchersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _authorization;

    public MobilePaymentVouchersController(AppDbContext db, ScreenAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    private ServerSession Session() =>
        HttpContext.Items["ServerSession"] as ServerSession
        ?? throw new InvalidOperationException("جلسة الخادم غير متاحة.");

    private Task<bool> CanViewAsync(CancellationToken cancellationToken) =>
        _authorization.IsAllowedAsync(Session(), "PaymentVoucher", ScreenOperation.View, cancellationToken);

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? voucherNo, CancellationToken cancellationToken)
    {
        if (!await CanViewAsync(cancellationToken)) return Forbid();
        var session = Session();

        var paymentTypeId = await _db.Voucher_Types.AsNoTracking()
            .Where(x => x.Is_Active && x.Voucher_Type_Code == "PAYMENT")
            .Select(x => x.Voucher_Type_ID)
            .SingleOrDefaultAsync(cancellationToken);

        var query = _db.Financial_Voucher_Headers.AsNoTracking()
            .Where(x => x.Voucher_Type_ID == paymentTypeId &&
                        x.Branch_ID == session.Branch_ID.ToString() &&
                        x.Fiscal_Year_ID == session.Year_ID &&
                        x.Is_Active);

        if (!string.IsNullOrWhiteSpace(voucherNo))
            query = query.Where(x => x.Voucher_No.Contains(voucherNo.Trim()));

        var data = await query
            .OrderByDescending(x => x.Voucher_Date)
            .ThenByDescending(x => x.Voucher_ID)
            .Take(500)
            .Select(x => new
            {
                voucherId = x.Voucher_ID,
                voucherNo = x.Voucher_No,
                voucherDate = x.Voucher_Date,
                beneficiaryName = x.Received_From_Name,
                description = x.Description,
                localTotal = x.Local_Total,
                isPosted = x.Is_Posted,
                journalEntryId = x.Journal_Entry_ID,
                sourceDocumentNo = x.Source_Document_No,
                approvalStatus = x.Approval_Status
            })
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    [HttpGet("{voucherId:long}")]
    public async Task<IActionResult> Get(long voucherId, CancellationToken cancellationToken)
    {
        if (!await CanViewAsync(cancellationToken)) return Forbid();
        var session = Session();

        var paymentTypeId = await _db.Voucher_Types.AsNoTracking()
            .Where(x => x.Is_Active && x.Voucher_Type_Code == "PAYMENT")
            .Select(x => x.Voucher_Type_ID)
            .SingleOrDefaultAsync(cancellationToken);

        var header = await _db.Financial_Voucher_Headers.AsNoTracking()
            .Where(x => x.Voucher_ID == voucherId &&
                        x.Voucher_Type_ID == paymentTypeId &&
                        x.Branch_ID == session.Branch_ID.ToString() &&
                        x.Fiscal_Year_ID == session.Year_ID &&
                        x.Is_Active)
            .Select(x => new
            {
                voucherId = x.Voucher_ID,
                voucherNo = x.Voucher_No,
                voucherDate = x.Voucher_Date,
                beneficiaryName = x.Received_From_Name,
                description = x.Description,
                referenceNo = x.Reference_No,
                sourceDocumentNo = x.Source_Document_No,
                cashAccountId = x.Cash_Account_ID,
                currencyId = x.Currency_ID,
                exchangeRate = x.Exchange_Rate,
                amount = x.Amount,
                foreignTotal = x.Foreign_Total,
                localTotal = x.Local_Total,
                isPosted = x.Is_Posted,
                journalEntryId = x.Journal_Entry_ID,
                approvalStatus = x.Approval_Status
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (header == null) return NotFound(new { message = "سند الصرف غير موجود ضمن الفرع والسنة الحالية." });

        var details = await _db.Financial_Voucher_Details.AsNoTracking()
            .Where(x => x.Voucher_ID == voucherId)
            .OrderBy(x => x.Line_No)
            .Select(x => new
            {
                lineNo = x.Line_No,
                accountId = x.Account_ID,
                costCenterId = x.Cost_Center_ID,
                currencyId = x.Currency_ID,
                exchangeRate = x.Exchange_Rate,
                foreignAmount = x.Foreign_Amount,
                localAmount = x.Local_Amount,
                debitAmount = x.Debit_Amount,
                creditAmount = x.Credit_Amount,
                description = x.Description
            })
            .ToListAsync(cancellationToken);

        return Ok(new { header, details });
    }
}
