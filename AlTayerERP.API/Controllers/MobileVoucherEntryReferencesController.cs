using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/mobile/voucher-entry-references")]
public sealed class MobileVoucherEntryReferencesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _authorization;

    public MobileVoucherEntryReferencesController(AppDbContext db, ScreenAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string type, CancellationToken cancellationToken)
    {
        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

        var normalized = type?.Trim().ToUpperInvariant();
        var screenCode = normalized == "RECEIPT" ? "ReceiptVoucher" : normalized == "PAYMENT" ? "PaymentVoucher" : null;
        if (screenCode == null) return BadRequest(new { message = "نوع السند غير صحيح." });

        if (!await _authorization.IsAllowedAsync(session, screenCode, ScreenOperation.Add, cancellationToken))
            return Forbid();

        // نحدد الشركة من الفرع الموثوق في قاعدة البيانات، لأن قيمة Company_ID في
        // الجلسة القديمة قد تحتوي اختلاف تنسيق أو مسافات فتؤدي إلى قوائم فارغة كلها.
        var trustedCompanyId = await _db.Tenant_Branches.AsNoTracking()
            .Where(x => x.Branch_ID == session.Branch_ID && x.Is_Active)
            .Select(x => x.Company_ID)
            .SingleOrDefaultAsync(cancellationToken);

        trustedCompanyId = string.IsNullOrWhiteSpace(trustedCompanyId)
            ? session.Company_ID.Trim()
            : trustedCompanyId.Trim();

        var voucherType = await _db.Voucher_Types.AsNoTracking()
            .Where(x => x.Is_Active && x.Voucher_Type_Code == normalized)
            .Select(x => new { id = x.Voucher_Type_ID, code = x.Voucher_Type_Code, name = x.Voucher_Type_Name_AR })
            .SingleOrDefaultAsync(cancellationToken);

        var draftStatus = await _db.Voucher_Statuses.AsNoTracking()
            .Where(x => x.Is_Active && x.Voucher_Status_Code == "DRAFT")
            .Select(x => new { id = x.Voucher_Status_ID, code = x.Voucher_Status_Code, name = x.Voucher_Status_Name_AR })
            .SingleOrDefaultAsync(cancellationToken);

        if (voucherType == null || draftStatus == null)
            return Conflict(new { message = "نوع السند أو حالة المسودة غير مهيأة." });

        var activeCashBoxes = await _db.Cash_Boxes.AsNoTracking()
            .Where(x => x.Company_ID.Trim() == trustedCompanyId &&
                        x.Branch_ID == session.Branch_ID &&
                        x.Is_Active &&
                        x.Account_ID != null && x.Account_ID != "")
            .OrderBy(x => x.Box_Name_AR)
            .Select(x => new { x.Account_ID, x.CashBox_Code, x.Box_Name_AR })
            .ToListAsync(cancellationToken);

        var cashAccountIds = activeCashBoxes.Select(x => x.Account_ID).Distinct().ToList();
        var cashAccounts = await _db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID.Trim() == trustedCompanyId &&
                        cashAccountIds.Contains(x.Account_ID) &&
                        x.Is_Active && x.Is_Postable && !x.Is_Summary_Account)
            .Select(x => new { x.Account_ID, x.Account_Code, x.Account_Name_AR })
            .ToDictionaryAsync(x => x.Account_ID, cancellationToken);

        // لا نعرض صندوقاً لا يملك حساباً نشطاً قابلاً للحركة؛ اختياره سيفشل عند الحفظ.
        var cashBoxes = activeCashBoxes
            .Where(box => cashAccounts.ContainsKey(box.Account_ID))
            .Select(box =>
            {
                var account = cashAccounts[box.Account_ID];
                return new
                {
                    accountId = box.Account_ID,
                    sourceType = "CASH",
                    displayName = box.CashBox_Code + " - " + box.Box_Name_AR + " | " + account.Account_Code + " - " + account.Account_Name_AR
                };
            }).ToList();

        var activeBanks = await _db.Bank_Accounts.AsNoTracking()
            .Where(x => x.Company_ID.Trim() == trustedCompanyId &&
                        x.Is_Active &&
                        x.GL_Account != null && x.GL_Account != "")
            .OrderBy(x => x.Bank_Name_AR)
            .Select(x => new { AccountId = x.GL_Account!, x.Bank_Name_AR, x.Account_No })
            .ToListAsync(cancellationToken);

        var bankAccountIds = activeBanks.Select(x => x.AccountId).Distinct().ToList();
        var bankAccounts = await _db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID.Trim() == trustedCompanyId &&
                        bankAccountIds.Contains(x.Account_ID) &&
                        x.Is_Active && x.Is_Postable && !x.Is_Summary_Account)
            .Select(x => new { x.Account_ID, x.Account_Code, x.Account_Name_AR })
            .ToDictionaryAsync(x => x.Account_ID, cancellationToken);

        // ونطبق القاعدة نفسها على الحسابات البنكية.
        var banks = activeBanks
            .Where(bank => bankAccounts.ContainsKey(bank.AccountId))
            .Select(bank =>
            {
                var account = bankAccounts[bank.AccountId];
                return new
                {
                    accountId = bank.AccountId,
                    sourceType = "BANK",
                    displayName = bank.Bank_Name_AR + " - " + bank.Account_No + " | " + account.Account_Code + " - " + account.Account_Name_AR
                };
            }).ToList();

        var sources = cashBoxes.Concat(banks)
            .GroupBy(x => new { x.accountId, x.sourceType, x.displayName })
            .Select(x => x.Key)
            .OrderBy(x => x.displayName)
            .ToList();

        var accounts = await _db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID.Trim() == trustedCompanyId && x.Is_Active && x.Is_Postable && !x.Is_Summary_Account)
            .OrderBy(x => x.Account_Code)
            .Select(x => new { id = x.Account_ID, displayName = x.Account_Code + " - " + x.Account_Name_AR })
            .ToListAsync(cancellationToken);

        var costCenters = await _db.Cost_Centers.AsNoTracking()
            .Where(x => x.Company_ID.Trim() == trustedCompanyId && x.Is_Active && x.Is_Postable)
            .OrderBy(x => x.Center_Code)
            .Select(x => new { id = x.Cost_Center_ID, displayName = x.Center_Code + " - " + x.Center_Name_AR })
            .ToListAsync(cancellationToken);

        var currencies = await _db.Currencies.AsNoTracking()
            .Where(x => x.Company_ID.Trim() == trustedCompanyId && x.Is_Active)
            .OrderByDescending(x => x.Is_Local_Currency).ThenBy(x => x.Currency_Code)
            .Select(x => new
            {
                id = x.Currency_ID,
                displayName = x.Currency_Code + " - " + x.Currency_Name_AR,
                exchangeRate = x.Is_Local_Currency ? 1m : x.Exchange_Rate,
                isLocal = x.Is_Local_Currency,
                isDefault = x.Is_Default
            }).ToListAsync(cancellationToken);

        var parties = await _db.Parties.AsNoTracking()
            .Where(x => x.Company_ID.Trim() == trustedCompanyId && x.Is_Active)
            .OrderBy(x => x.Party_Name_AR)
            .Select(x => new { id = x.Party_ID, displayName = x.Party_Code + " - " + x.Party_Name_AR, name = x.Party_Name_AR })
            .ToListAsync(cancellationToken);

        var paymentMethods = await _db.Payment_Methods.AsNoTracking()
            .Where(x => x.Is_Active)
            .OrderBy(x => x.Payment_Method_Name_AR)
            .Select(x => new { id = x.Payment_Method_ID, displayName = x.Payment_Method_Name_AR })
            .ToListAsync(cancellationToken);

        var openPeriods = await _db.Fiscal_Periods.AsNoTracking()
            .Where(x => x.Branch_ID == session.Branch_ID && x.Fiscal_Year_ID == session.Year_ID && x.Is_Active && !x.Is_Closed)
            .OrderBy(x => x.Start_Date)
            .Select(x => new { startDate = x.Start_Date.Date, endDate = x.End_Date.Date })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            voucherType,
            draftStatus,
            sources,
            sourceCount = sources.Count,
            sourceMessage = sources.Count == 0
                ? "لا توجد صناديق أو بنوك فعالة لها حساب مرتبط ضمن الشركة والفرع الحاليين."
                : null,
            accounts,
            accountCount = accounts.Count,
            costCenters,
            costCenterCount = costCenters.Count,
            currencies,
            currencyCount = currencies.Count,
            parties,
            partyCount = parties.Count,
            paymentMethods,
            paymentMethodCount = paymentMethods.Count,
            openPeriods,
            openPeriodCount = openPeriods.Count
        });
    }
}
