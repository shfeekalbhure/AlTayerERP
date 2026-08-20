using AlTayerERP.API.DTOs;
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
    private readonly ILogger<MobilePaymentRequestReferencesController> _logger;

    public MobilePaymentRequestReferencesController(
        AppDbContext db,
        ScreenAuthorizationService authorization,
        ILogger<MobilePaymentRequestReferencesController> logger)
    {
        _db = db;
        _authorization = authorization;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? type,
        [FromQuery] bool includeLookupData = true,
        CancellationToken cancellationToken = default)
    {
        var stage = "التحقق من جلسة المستخدم";

        try
        {
            if (HttpContext.Items["ServerSession"] is not ServerSession session)
                return Unauthorized(new { message = "توقف تحميل المنسدلات عند مرحلة التحقق من الجلسة: انتهت الجلسة أو أنها غير صالحة.", stage });

            var normalized = type?.Trim().ToUpperInvariant();
            var isReceipt = normalized == "RECEIPT";
            var isPayment = normalized == "PAYMENT";
            var isVoucher = isReceipt || isPayment;

            stage = "التحقق من صلاحية فتح السند";
            bool allowed;
            if (isReceipt)
                allowed = await _authorization.IsAllowedAsync(session, "ReceiptVoucher", ScreenOperation.Add, cancellationToken);
            else if (isPayment)
                allowed = await _authorization.IsAllowedAsync(session, "PaymentVoucher", ScreenOperation.Add, cancellationToken);
            else
                allowed = await _authorization.IsExplicitlyAllowedAsync(session, "PaymentRequest", ScreenOperation.View, cancellationToken);

            if (!allowed)
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = $"توقف تحميل المنسدلات عند مرحلة {stage}: لا توجد الصلاحية المطلوبة.",
                    stage
                });

            var accounts = new List<MobileReferenceLookupItemDto>();
            var costCenters = new List<MobileReferenceLookupItemDto>();
            if (includeLookupData)
            {
                stage = "تحميل الحسابات المحاسبية";
                accounts = await _db.Chart_Of_Accounts.AsNoTracking()
                    .Where(x => x.Company_ID == session.Company_ID && x.Is_Active && x.Is_Postable && !x.Is_Summary_Account)
                    .OrderBy(x => x.Account_Code)
                    .Select(x => new MobileReferenceLookupItemDto(
                        x.Account_ID,
                        x.Account_Code,
                        x.Account_Name_AR,
                        x.Account_Code + " - " + x.Account_Name_AR))
                    .ToListAsync(cancellationToken);

                stage = "تحميل مراكز التكلفة";
                costCenters = await _db.Cost_Centers.AsNoTracking()
                    .Where(x => x.Company_ID == session.Company_ID && x.Is_Active && x.Is_Postable)
                    .OrderBy(x => x.Center_Code)
                    .Select(x => new MobileReferenceLookupItemDto(
                        x.Cost_Center_ID,
                        x.Center_Code,
                        x.Center_Name_AR,
                        x.Center_Code + " - " + x.Center_Name_AR))
                    .ToListAsync(cancellationToken);
            }

            stage = "تحميل العملات";
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

            stage = "تحميل طرق السداد";
            var paymentMethods = await _db.Payment_Methods.AsNoTracking()
                .Where(x => x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .ThenBy(x => x.Payment_Method_Name_AR)
                .Select(x => new
                {
                    id = x.Payment_Method_ID,
                    displayName = x.Payment_Method_Name_AR
                })
                .ToListAsync(cancellationToken);

            stage = "تحميل الفترات المالية المفتوحة";
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
                return Ok(new { accounts, costCenters, currencies, paymentMethods, openPeriods, diagnosticStage = "اكتمل تحميل قوائم طلب الصرف" });

            stage = "تحميل الصناديق المربوطة بحساب";
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

            stage = "تحميل البنوك المربوطة بحساب";
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

            stage = "دمج الصناديق والبنوك";
            var sources = cashBoxes.Concat(banks)
                .GroupBy(x => new { x.accountId, x.sourceType, x.displayName })
                .Select(x => x.Key)
                .OrderBy(x => x.displayName)
                .ToList();

            stage = "تحميل الأطراف";
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

            stage = "تحميل نوع السند";
            var voucherTypeCode = isReceipt ? "RECEIPT" : "PAYMENT";
            var voucherType = await _db.Voucher_Types.AsNoTracking()
                .Where(x => x.Is_Active && x.Voucher_Type_Code == voucherTypeCode)
                .Select(x => new { id = x.Voucher_Type_ID, code = x.Voucher_Type_Code, name = x.Voucher_Type_Name_AR })
                .SingleOrDefaultAsync(cancellationToken);

            stage = "تحميل حالة المسودة";
            var draftStatus = await _db.Voucher_Statuses.AsNoTracking()
                .Where(x => x.Is_Active && x.Voucher_Status_Code == "DRAFT")
                .Select(x => new { id = x.Voucher_Status_ID, code = x.Voucher_Status_Code, name = x.Voucher_Status_Name_AR })
                .SingleOrDefaultAsync(cancellationToken);

            if (voucherType == null || draftStatus == null)
                return Conflict(new
                {
                    message = $"توقف تحميل المنسدلات عند مرحلة {stage}: نوع سند {(isReceipt ? "القبض" : "الصرف")} أو حالة المسودة غير مهيأة.",
                    stage
                });

            stage = "اكتمل تحميل جميع المنسدلات";
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
                openPeriods,
                diagnosticStage = stage
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "توقف تحميل منسدلات السند عند المرحلة {Stage}.", stage);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = $"توقف تحميل المنسدلات عند مرحلة: {stage}.",
                stage
            });
        }
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(
        [FromQuery] string? resource,
        [FromQuery] string? search,
        [FromQuery] int limit = MobileReferenceLookupPolicy.DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized(new ApiErrorResponse(
                "INVALID_SESSION",
                "انتهت الجلسة أو أنها غير صالحة.",
                HttpContext.TraceIdentifier));

        var allowed = await _authorization.IsExplicitlyAllowedAsync(
            session,
            "PaymentRequest",
            ScreenOperation.View,
            cancellationToken);
        if (!allowed)
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(
                "ACCESS_DENIED",
                "لا توجد الصلاحية المطلوبة لتحميل البيانات المرجعية.",
                HttpContext.TraceIdentifier));

        var normalizedResource = resource?.Trim().ToUpperInvariant();
        var normalizedSearch = MobileReferenceLookupPolicy.NormalizeSearch(search);
        var normalizedLimit = MobileReferenceLookupPolicy.NormalizeLimit(limit);

        return normalizedResource switch
        {
            "ACCOUNT" or "ACCOUNTS" => Ok(await LookupAccountsAsync(session.Company_ID, normalizedSearch, normalizedLimit, cancellationToken)),
            "COST_CENTER" or "COST_CENTERS" => Ok(await LookupCostCentersAsync(session.Company_ID, normalizedSearch, normalizedLimit, cancellationToken)),
            "PARTY" or "PARTIES" => Ok(await LookupPartiesAsync(session.Company_ID, normalizedSearch, normalizedLimit, cancellationToken)),
            _ => BadRequest(new ApiErrorResponse(
                "INVALID_LOOKUP_RESOURCE",
                "نوع البيانات المرجعية المطلوب غير مدعوم.",
                HttpContext.TraceIdentifier))
        };
    }

    private async Task<MobileReferenceLookupResponseDto> LookupAccountsAsync(
        string companyId,
        string search,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = _db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID == companyId && x.Is_Active && x.Is_Postable && !x.Is_Summary_Account);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Account_Code.Contains(search) || x.Account_Name_AR.Contains(search));

        var items = await query.OrderBy(x => x.Account_Code)
            .Select(x => new MobileReferenceLookupItemDto(x.Account_ID, x.Account_Code, x.Account_Name_AR, x.Account_Code + " - " + x.Account_Name_AR))
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        return BuildLookupResponse(items, limit, search);
    }

    private async Task<MobileReferenceLookupResponseDto> LookupCostCentersAsync(
        string companyId,
        string search,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = _db.Cost_Centers.AsNoTracking()
            .Where(x => x.Company_ID == companyId && x.Is_Active && x.Is_Postable);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Center_Code.Contains(search) || x.Center_Name_AR.Contains(search));

        var items = await query.OrderBy(x => x.Center_Code)
            .Select(x => new MobileReferenceLookupItemDto(x.Cost_Center_ID, x.Center_Code, x.Center_Name_AR, x.Center_Code + " - " + x.Center_Name_AR))
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        return BuildLookupResponse(items, limit, search);
    }

    private async Task<MobileReferenceLookupResponseDto> LookupPartiesAsync(
        string companyId,
        string search,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = _db.Parties.AsNoTracking()
            .Where(x => x.Company_ID == companyId && x.Is_Active);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Party_Code.Contains(search) || x.Party_Name_AR.Contains(search));

        var items = await query.OrderBy(x => x.Party_Name_AR)
            .Select(x => new MobileReferenceLookupItemDto(x.Party_ID, x.Party_Code, x.Party_Name_AR, x.Party_Code + " - " + x.Party_Name_AR))
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        return BuildLookupResponse(items, limit, search);
    }

    private static MobileReferenceLookupResponseDto BuildLookupResponse(
        List<MobileReferenceLookupItemDto> items,
        int limit,
        string search)
    {
        var hasMore = items.Count > limit;
        return new MobileReferenceLookupResponseDto(items.Take(limit).ToList(), limit, search, hasMore);
    }
}
