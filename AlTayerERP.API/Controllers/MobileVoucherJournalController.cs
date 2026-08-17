using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mobile/voucher-journal")]
public sealed class MobileVoucherJournalController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _authorization;

    public MobileVoucherJournalController(AppDbContext db, ScreenAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    [HttpGet("{voucherId:long}")]
    public async Task<IActionResult> Get(long voucherId, CancellationToken cancellationToken)
    {
        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

        var voucher = await (
            from h in _db.Financial_Voucher_Headers.AsNoTracking()
            join t in _db.Voucher_Types.AsNoTracking() on h.Voucher_Type_ID equals t.Voucher_Type_ID
            where h.Voucher_ID == voucherId && h.Branch_ID == session.Branch_ID &&
                  h.Fiscal_Year_ID == session.Year_ID && h.Is_Active
            select new
            {
                h.Voucher_ID,
                h.Voucher_No,
                h.Journal_Entry_ID,
                t.Voucher_Type_Code
            }).SingleOrDefaultAsync(cancellationToken);

        if (voucher == null)
            return NotFound(new { message = "السند غير موجود ضمن الفرع والسنة الحالية." });

        var screenCode = voucher.Voucher_Type_Code switch
        {
            "RECEIPT" => "ReceiptVoucher",
            "PAYMENT" => "PaymentVoucher",
            "JOURNAL" => "JournalVoucher",
            _ => null
        };

        if (screenCode == null ||
            !await _authorization.IsAllowedAsync(session, screenCode, ScreenOperation.View, cancellationToken))
            return Forbid();

        if (!voucher.Journal_Entry_ID.HasValue)
            return Conflict(new { message = "لم يتم إنشاء قيد محاسبي لهذا السند بعد. يجب ترحيل السند أولاً." });

        var header = await _db.Journal_Entry_Headers.AsNoTracking()
            .Where(x => x.Journal_Entry_ID == voucher.Journal_Entry_ID.Value &&
                        x.Branch_ID == session.Branch_ID && x.Fiscal_Year_ID == session.Year_ID)
            .Select(x => new
            {
                journalEntryId = x.Journal_Entry_ID,
                entryNo = x.Entry_No,
                entryDate = x.Entry_Date,
                description = x.Description,
                sourceDocumentNo = x.Source_Document_No,
                totalDebit = x.Total_Debit,
                totalCredit = x.Total_Credit,
                isPosted = x.Is_Posted,
                isCancelled = x.Is_Cancelled
            }).SingleOrDefaultAsync(cancellationToken);

        if (header == null)
            return NotFound(new { message = "القيد المحاسبي المرتبط بالسند غير موجود." });

        var details = await (
            from d in _db.Journal_Entry_Details.AsNoTracking()
            join a in _db.Chart_Of_Accounts.AsNoTracking() on d.Account_ID equals a.Account_ID into accountGroup
            from a in accountGroup.DefaultIfEmpty()
            join cc in _db.Cost_Centers.AsNoTracking() on d.Cost_Center_ID equals cc.Cost_Center_ID into centerGroup
            from cc in centerGroup.DefaultIfEmpty()
            join c in _db.Currencies.AsNoTracking() on d.Currency_ID equals c.Currency_ID into currencyGroup
            from c in currencyGroup.DefaultIfEmpty()
            where d.Journal_Entry_ID == voucher.Journal_Entry_ID.Value
            orderby d.Line_No
            select new
            {
                lineNo = d.Line_No,
                accountDisplay = a == null ? d.Account_ID : a.Account_Code + " - " + a.Account_Name_AR,
                costCenterDisplay = d.Cost_Center_ID == null ? null :
                    cc == null ? d.Cost_Center_ID : cc.Center_Code + " - " + cc.Center_Name_AR,
                currencyDisplay = c == null ? d.Currency_ID.ToString() : c.Currency_Code + " - " + c.Currency_Name_AR,
                exchangeRate = d.Exchange_Rate,
                foreignAmount = d.Foreign_Amount,
                localAmount = d.Local_Amount,
                debit = d.Debit_Amount,
                credit = d.Credit_Amount,
                description = d.Description,
                referenceType = d.Reference_Type,
                referenceNo = d.Reference_No
            }).ToListAsync(cancellationToken);

        return Ok(new { voucherNo = voucher.Voucher_No, header, details });
    }
}
