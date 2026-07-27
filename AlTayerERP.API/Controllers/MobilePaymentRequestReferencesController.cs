using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mobile/payment-request-references")]
public sealed class MobilePaymentRequestReferencesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _authorization;

    public MobilePaymentRequestReferencesController(AppDbContext db, ScreenAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

        var canView = await _authorization.IsExplicitlyAllowedAsync(
            session, "PaymentRequest", ScreenOperation.View, cancellationToken);
        if (!canView)
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

        var costCenters = await _db.Cost_Centers.AsNoTracking()
            .Where(x => x.Company_ID == session.Company_ID && x.Is_Active && x.Is_Postable)
            .OrderBy(x => x.Center_Code)
            .Select(x => new
            {
                id = x.Cost_Center_ID,
                code = x.Center_Code,
                name = x.Center_Name_AR,
                displayName = x.Center_Code + " - " + x.Center_Name_AR
            })
            .ToListAsync(cancellationToken);

        var currencies = await _db.Currencies.AsNoTracking()
            .Where(x => x.Company_ID == session.Company_ID && x.Is_Active)
            .OrderByDescending(x => x.Is_Local_Currency).ThenBy(x => x.Currency_Code)
            .Select(x => new
            {
                id = x.Currency_ID,
                code = x.Currency_Code,
                name = x.Currency_Name_AR,
                displayName = x.Currency_Code + " - " + x.Currency_Name_AR,
                exchangeRate = x.Is_Local_Currency ? 1m : x.Exchange_Rate,
                isLocal = x.Is_Local_Currency,
                isDefault = x.Is_Default
            })
            .ToListAsync(cancellationToken);

        var openPeriods = await _db.Fiscal_Periods.AsNoTracking()
            .Where(x => x.Branch_ID == session.Branch_ID &&
                        x.Fiscal_Year_ID == session.Year_ID &&
                        x.Is_Active && !x.Is_Closed)
            .OrderBy(x => x.Start_Date)
            .Select(x => new
            {
                startDate = x.Start_Date.Date,
                endDate = x.End_Date.Date,
                displayName = x.Start_Date.ToString("yyyy/MM/dd") + " - " + x.End_Date.ToString("yyyy/MM/dd")
            })
            .ToListAsync(cancellationToken);

        return Ok(new { accounts, costCenters, currencies, openPeriods });
    }
}
