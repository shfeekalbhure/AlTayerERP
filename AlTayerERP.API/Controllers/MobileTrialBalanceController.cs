using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mobile/trial-balance")]
public sealed class MobileTrialBalanceController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _authorization;

    public MobileTrialBalanceController(AppDbContext db, ScreenAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken cancellationToken)
    {
        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

        if (!await _authorization.IsAllowedAsync(session, "TrialBalance", ScreenOperation.View, cancellationToken))
            return Forbid();

        if (fromDate.Date > toDate.Date)
            return BadRequest(new { message = "تاريخ البداية يجب ألا يتجاوز تاريخ النهاية." });

        var start = fromDate.Date;
        var endExclusive = toDate.Date.AddDays(1);
        int branchId = session.Branch_ID;

        var opening = await (
            from d in _db.Journal_Entry_Details.AsNoTracking()
            join h in _db.Journal_Entry_Headers.AsNoTracking() on d.Journal_Entry_ID equals h.Journal_Entry_ID
            where h.Branch_ID == branchId && h.Fiscal_Year_ID == session.Year_ID && h.Is_Active &&
                  h.Is_Posted && !h.Is_Cancelled && h.Entry_Date < start
            group d by d.Account_ID into g
            select new { AccountId = g.Key, Debit = g.Sum(x => x.Debit_Amount), Credit = g.Sum(x => x.Credit_Amount) }
        ).ToDictionaryAsync(x => x.AccountId, cancellationToken);

        var movement = await (
            from d in _db.Journal_Entry_Details.AsNoTracking()
            join h in _db.Journal_Entry_Headers.AsNoTracking() on d.Journal_Entry_ID equals h.Journal_Entry_ID
            where h.Branch_ID == branchId && h.Fiscal_Year_ID == session.Year_ID && h.Is_Active &&
                  h.Is_Posted && !h.Is_Cancelled && h.Entry_Date >= start && h.Entry_Date < endExclusive
            group d by d.Account_ID into g
            select new { AccountId = g.Key, Debit = g.Sum(x => x.Debit_Amount), Credit = g.Sum(x => x.Credit_Amount) }
        ).ToDictionaryAsync(x => x.AccountId, cancellationToken);

        var accountIds = opening.Keys.Union(movement.Keys).ToList();
        var accounts = await _db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID == session.Company_ID && accountIds.Contains(x.Account_ID))
            .Select(x => new { x.Account_ID, x.Account_Code, x.Account_Name_AR })
            .ToDictionaryAsync(x => x.Account_ID, cancellationToken);

        var rows = accountIds.Select(id =>
        {
            opening.TryGetValue(id, out var o);
            movement.TryGetValue(id, out var m);
            accounts.TryGetValue(id, out var account);
            var openingBalance = (o?.Debit ?? 0m) - (o?.Credit ?? 0m);
            var debit = m?.Debit ?? 0m;
            var credit = m?.Credit ?? 0m;
            var closing = openingBalance + debit - credit;
            return new
            {
                accountId = id,
                accountCode = account?.Account_Code ?? id,
                accountName = account?.Account_Name_AR ?? id,
                openingDebit = openingBalance > 0 ? openingBalance : 0m,
                openingCredit = openingBalance < 0 ? -openingBalance : 0m,
                periodDebit = debit,
                periodCredit = credit,
                closingDebit = closing > 0 ? closing : 0m,
                closingCredit = closing < 0 ? -closing : 0m
            };
        }).OrderBy(x => x.accountCode).ToList();

        return Ok(new
        {
            fromDate = start,
            toDate = toDate.Date,
            rows,
            totals = new
            {
                openingDebit = rows.Sum(x => x.openingDebit),
                openingCredit = rows.Sum(x => x.openingCredit),
                periodDebit = rows.Sum(x => x.periodDebit),
                periodCredit = rows.Sum(x => x.periodCredit),
                closingDebit = rows.Sum(x => x.closingDebit),
                closingCredit = rows.Sum(x => x.closingCredit)
            }
        });
    }
}
