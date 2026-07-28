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
    public async Task<IActionResult> Get([FromQuery] string? type, CancellationToken cancellationToken)
    {
        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

        var normalized = type?.Trim().ToUpperInvariant();
        var isReceipt = normalized == "RECEIPT";
        var isPayment = normalized == "PAYMENT";
        var isVoucher = isReceipt || isPayment;

        bool allowed;
        if (isReceipt)
            allowed = await _authorization.IsAllowedAsync(session, "ReceiptVoucher", ScreenOperation.Add, cancellationToken);
        else if (isPayment)
            allowed = await _authorization.IsAllowedAsync(session, "PaymentVoucher", ScreenOperation.Add, cancellationToken);
        else
            allowed = await _authorization.IsExplicitlyAllowedAsync(session, "PaymentRequest", ScreenOperation.View, cancellationToken);

        if (!allowed)
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

        if (!isVoucher)
            return Ok(new { accounts, costCenters, currencies, openPeriods });

        var cashBoxes = await _db.Cash_Boxes.AsNoTracking()
            .Where(x => x.Company_ID == session.Company_ID &&
                        x.Branch_ID == session.Branch_ID &&
                        x.Is_Active &&
                        x.Account_ID != null && x.Account_ID != "")
            .OrderBy(x => x.Box_Name_AR)
            .Select(x => new
            {
                accountId = x.Account_ID,
                sourceType = "CASH",
                displayName = x.CashBox_Code + " - " + x.Box_Name_AR
            })
            .ToListAsync(cancellationToken);

        var banks = await _db.Bank_Accounts.AsNoTracking()
            .Where(x => x.Company_ID == session.Company_ID &&
                        x.Is_Active &&
                        x.GL_Account != null && x.GL_Account != "")
            .OrderBy(x => x.Bank_Name_AR)
            .Select(x => new
            {
                accountId = x.GL_Account!,
                sourceType = "BANK",
                displayName = x.Bank_Name_AR + " - " + x.Account_No
            })
            .ToListAsync(cancellationToken);

        var sources = cashBoxes.Concat(banks)
            .GroupBy(x => new { x.accountId, x.sourceType, x.displayName })
            .Select(x => x.Key)
            .OrderBy(x => x.displayName)
            .ToList();

        var parties = await _db.Parties.AsNoTracking()
            .Where(x => x.Company_ID == session.Company_ID && x.Is_Active)
            .OrderBy(x => x.Party_Name_AR)
            .Select(x => new
            {
                id = x.Party_ID,
                displayName = x.Party_Code + " - " + x.Party_Name_AR,
                name = x.Party_Name_AR
            })
            .ToListAsync(cancellationToken);

        var paymentMethods = await _db.Payment_Methods.AsNoTracking()
            .Where(x => x.Is_Active)
            .OrderBy(x => x.Payment_Method_Name_AR)
            .Select(x => new
            {
                id = x.Payment_Method_ID,
                displayName = x.Payment_Method_Name_AR
            })
            .ToListAsync(cancellationToken);

        var voucherTypeCode = isReceipt ? "RECEIPT" : "PAYMENT";
        var voucherType = await _db.Voucher_Types.AsNoTracking()
            .Where(x => x.Is_Active && x.Voucher_Type_Code == voucherTypeCode)
            .Select(x => new { id = x.Voucher_Type_ID, code = x.Voucher_Type_Code, name = x.Voucher_Type_Name_AR })
            .SingleOrDefaultAsync(cancellationToken);

        var draftStatus = await _db.Voucher_Statuses.AsNoTracking()
            .Where(x => x.Is_Active && x.Voucher_Status_Code == "DRAFT")
            .Select(x => new { id = x.Voucher_Status_ID, code = x.Voucher_Status_Code, name = x.Voucher_Status_Name_AR })
            .SingleOrDefaultAsync(cancellationToken);

        if (voucherType == null || draftStatus == null)
            return Conflict(new { message = $"نوع سند {(isReceipt ? "القبض" : "الصرف")} أو حالة المسودة غير مهيأة." });

        return Ok(new
        {
            voucherType,
            draftStatus,
            sources,
            sourceCount = sources.Count,
            sourceMessage = sources.Count == 0 ? "لا توجد صناديق أو بنوك متاحة للفرع الحالي." : null,
            accounts,
            costCenters,
            currencies,
            parties,
            paymentMethods,
            openPeriods
        });
    }
}
