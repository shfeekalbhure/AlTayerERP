using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mobile/general-ledger")]
public sealed class MobileGeneralLedgerController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _authorization;

    public MobileGeneralLedgerController(AppDbContext db, ScreenAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    private ServerSession Session() => HttpContext.Items["ServerSession"] as ServerSession
        ?? throw new InvalidOperationException("جلسة الخادم غير متاحة.");

    [HttpGet("accounts")]
    public async Task<IActionResult> Accounts(CancellationToken cancellationToken)
    {
        var session = Session();
        if (!await _authorization.IsAllowedAsync(session, "GeneralLedger", ScreenOperation.View, cancellationToken))
            return Forbid();

        var accounts = await _db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID == session.Company_ID && x.Is_Active && x.Is_Postable && !x.Is_Summary_Account)
            .OrderBy(x => x.Account_Code)
            .Select(x => new
            {
                id = x.Account_ID,
                code = x.Account_Code,
                name = x.Account_Name_AR,
                displayName = x.Account_Code + " - " + x.Account_Name_AR
            })
            .ToListAsync(cancellationToken);

        return Ok(accounts);
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string accountId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken)
    {
        var session = Session();
        if (!await _authorization.IsAllowedAsync(session, "GeneralLedger", ScreenOperation.View, cancellationToken))
            return Forbid();

        if (string.IsNullOrWhiteSpace(accountId))
            return BadRequest(new { message = "الحساب مطلوب." });
        if (fromDate.Date > toDate.Date)
            return BadRequest(new { message = "تاريخ البداية يجب ألا يتجاوز تاريخ النهاية." });

        var account = await _db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID == session.Company_ID && x.Account_ID == accountId)
            .Select(x => new { x.Account_ID, x.Account_Code, x.Account_Name_AR })
            .SingleOrDefaultAsync(cancellationToken);

        if (account == null)
            return NotFound(new { message = "الحساب غير موجود ضمن الشركة الحالية." });

        var start = fromDate.Date;
        var endExclusive = toDate.Date.AddDays(1);
        var branchId = session.Branch_ID.ToString();

        var opening = await (
            from d in _db.Journal_Entry_Details.AsNoTracking()
            join h in _db.Journal_Entry_Headers.AsNoTracking() on d.Journal_Entry_ID equals h.Journal_Entry_ID
            where d.Account_ID == accountId && h.Branch_ID == branchId && h.Fiscal_Year_ID == session.Year_ID &&
                  h.Is_Active && h.Is_Posted && !h.Is_Cancelled && h.Entry_Date < start
            select d.Debit_Amount - d.Credit_Amount
        ).SumAsync(cancellationToken);

        var raw = await (
            from d in _db.Journal_Entry_Details.AsNoTracking()
            join h in _db.Journal_Entry_Headers.AsNoTracking() on d.Journal_Entry_ID equals h.Journal_Entry_ID
            where d.Account_ID == accountId && h.Branch_ID == branchId && h.Fiscal_Year_ID == session.Year_ID &&
                  h.Is_Active && h.Is_Posted && !h.Is_Cancelled &&
                  h.Entry_Date >= start && h.Entry_Date < endExclusive
            orderby h.Entry_Date, h.Journal_Entry_ID, d.Line_No
            select new
            {
                h.Journal_Entry_ID,
                h.Entry_No,
                h.Entry_Date,
                h.Source_Document_Type,
                h.Source_Document_No,
                HeaderDescription = h.Description,
                d.Line_No,
                d.Description,
                d.Reference_Type,
                d.Reference_No,
                d.Debit_Amount,
                d.Credit_Amount
            }
        ).ToListAsync(cancellationToken);

        var balance = opening;
        var rows = raw.Select(x =>
        {
            balance += x.Debit_Amount - x.Credit_Amount;
            return new
            {
                journalEntryId = x.Journal_Entry_ID,
                entryNo = x.Entry_No,
                entryDate = x.Entry_Date,
                sourceDocumentType = x.Source_Document_Type,
                sourceDocumentNo = x.Source_Document_No,
                lineNo = x.Line_No,
                description = string.IsNullOrWhiteSpace(x.Description) ? x.HeaderDescription : x.Description,
                referenceType = x.Reference_Type,
                referenceNo = x.Reference_No,
                debit = x.Debit_Amount,
                credit = x.Credit_Amount,
                runningDebit = balance > 0 ? balance : 0m,
                runningCredit = balance < 0 ? -balance : 0m
            };
        }).ToList();

        return Ok(new
        {
            accountId = account.Account_ID,
            accountCode = account.Account_Code,
            accountName = account.Account_Name_AR,
            fromDate = start,
            toDate = toDate.Date,
            openingDebit = opening > 0 ? opening : 0m,
            openingCredit = opening < 0 ? -opening : 0m,
            periodDebit = rows.Sum(x => x.debit),
            periodCredit = rows.Sum(x => x.credit),
            closingDebit = balance > 0 ? balance : 0m,
            closingCredit = balance < 0 ? -balance : 0m,
            rows
        });
    }
}
