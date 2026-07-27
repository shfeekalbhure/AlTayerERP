using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mobile/payment-voucher-sources")]
public sealed class MobilePaymentVoucherSourcesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _authorization;

    public MobilePaymentVoucherSourcesController(AppDbContext db, ScreenAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

        if (!await _authorization.IsExplicitlyAllowedAsync(session, "PaymentRequest", ScreenOperation.Add, cancellationToken))
            return Forbid();

        var cashBoxes = await _db.Cash_Boxes.AsNoTracking()
            .Where(x => x.Company_ID == session.Company_ID && x.Branch_ID == session.Branch_ID && x.Is_Active)
            .OrderBy(x => x.CashBox_Code)
            .Select(x => new
            {
                accountId = x.Account_ID,
                sourceType = "CASH",
                displayName = "صندوق: " + x.CashBox_Code + " - " + x.Box_Name_AR
            })
            .ToListAsync(cancellationToken);

        var bankAccounts = await _db.Bank_Accounts.AsNoTracking()
            .Where(x => x.Company_ID == session.Company_ID && x.Is_Active && x.GL_Account != null && x.GL_Account != "")
            .OrderBy(x => x.Bank_Name_AR).ThenBy(x => x.Account_No)
            .Select(x => new
            {
                accountId = x.GL_Account!,
                sourceType = "BANK",
                displayName = "بنك: " + x.Bank_Name_AR + " - " + x.Account_No
            })
            .ToListAsync(cancellationToken);

        return Ok(cashBoxes.Concat(bankAccounts).ToList());
    }
}
