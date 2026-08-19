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
                approvalStatus = x.Approval_Status,
                reviewStatus = x.Review_Status,
                voucherStatusId = x.Voucher_Status_ID,
                requiresApproval = x.Requires_Approval
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

        var voucher = await _db.Financial_Voucher_Headers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Voucher_ID == voucherId &&
                        x.Voucher_Type_ID == paymentTypeId &&
                        x.Branch_ID == session.Branch_ID.ToString() &&
                        x.Fiscal_Year_ID == session.Year_ID &&
                        x.Is_Active,
                cancellationToken);

        if (voucher == null) return NotFound(new { message = "سند الصرف غير موجود ضمن الفرع والسنة الحالية." });

        var cashAccount = await _db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Account_ID == voucher.Cash_Account_ID && x.Company_ID == session.Company_ID)
            .Select(x => new { x.Account_Code, x.Account_Name_AR })
            .SingleOrDefaultAsync(cancellationToken);
        var currency = await _db.Currencies.AsNoTracking()
            .Where(x => x.Currency_ID == voucher.Currency_ID && x.Company_ID == session.Company_ID)
            .Select(x => new { x.Currency_Code, x.Currency_Name_AR })
            .SingleOrDefaultAsync(cancellationToken);
        var partyName = string.IsNullOrWhiteSpace(voucher.Party_ID) ? null : await _db.Parties.AsNoTracking()
            .Where(x => x.Party_ID == voucher.Party_ID && x.Company_ID == session.Company_ID)
            .Select(x => x.Party_Name_AR)
            .SingleOrDefaultAsync(cancellationToken);
        var branchName = await _db.Tenant_Branches.AsNoTracking()
            .Where(x => x.Branch_ID == session.Branch_ID)
            .Select(x => x.Branch_Name)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;
        var company = await _db.Companies.AsNoTracking()
            .Where(x => x.Company_ID.Trim() == session.Company_ID.Trim())
            .Select(x => new { x.Company_Name_AR, x.Company_Logo })
            .SingleOrDefaultAsync(cancellationToken);

        var header = new
        {
            voucherId = voucher.Voucher_ID, voucherNo = voucher.Voucher_No, voucherDate = voucher.Voucher_Date,
            beneficiaryName = string.IsNullOrWhiteSpace(voucher.Received_From_Name) ? partyName : voucher.Received_From_Name,
            description = voucher.Description, referenceNo = voucher.Reference_No, sourceDocumentNo = voucher.Source_Document_No,
            cashAccountId = voucher.Cash_Account_ID,
            cashAccountDisplay = cashAccount == null ? voucher.Cash_Account_ID : cashAccount.Account_Code + " - " + cashAccount.Account_Name_AR,
            branchName, companyName = company?.Company_Name_AR ?? string.Empty, companyLogoDataUri = ToImageDataUri(company?.Company_Logo),
            partyId = voucher.Party_ID, paymentMethodId = voucher.Payment_Method_ID, currencyId = voucher.Currency_ID,
            currencyDisplay = currency == null ? voucher.Currency_ID.ToString() : currency.Currency_Code + " - " + currency.Currency_Name_AR,
            exchangeRate = voucher.Exchange_Rate, amount = voucher.Amount, foreignTotal = voucher.Foreign_Total, localTotal = voucher.Local_Total,
            isPosted = voucher.Is_Posted, journalEntryId = voucher.Journal_Entry_ID, approvalStatus = voucher.Approval_Status,
            reviewStatus = voucher.Review_Status, reviewNotes = voucher.Review_Notes, voucherTypeId = voucher.Voucher_Type_ID,
            voucherStatusId = voucher.Voucher_Status_ID, requiresApproval = voucher.Requires_Approval, editCount = voucher.Edit_Count,
            printCount = voucher.Print_Count, createdAt = voucher.Created_At, updatedAt = voucher.Updated_At
        };

        var details = await (from d in _db.Financial_Voucher_Details.AsNoTracking()
            join a in _db.Chart_Of_Accounts.AsNoTracking() on d.Account_ID equals a.Account_ID into accounts
            from a in accounts.DefaultIfEmpty()
            join c in _db.Currencies.AsNoTracking() on d.Currency_ID equals c.Currency_ID into currencies
            from c in currencies.DefaultIfEmpty()
            join cc in _db.Cost_Centers.AsNoTracking() on d.Cost_Center_ID equals cc.Cost_Center_ID into centers
            from cc in centers.DefaultIfEmpty()
            where d.Voucher_ID == voucherId
            orderby d.Line_No
            select new
            {
                voucherDetailId = d.Voucher_Detail_ID, lineNo = d.Line_No, accountId = d.Account_ID,
                accountDisplay = a == null ? d.Account_ID : a.Account_Code + " - " + a.Account_Name_AR,
                costCenterId = d.Cost_Center_ID,
                costCenterDisplay = d.Cost_Center_ID == null ? null : cc == null ? d.Cost_Center_ID : cc.Center_Code + " - " + cc.Center_Name_AR,
                currencyId = d.Currency_ID, currencyDisplay = c == null ? d.Currency_ID.ToString() : c.Currency_Code + " - " + c.Currency_Name_AR,
                exchangeRate = d.Exchange_Rate, foreignAmount = d.Foreign_Amount, localAmount = d.Local_Amount,
                debitAmount = d.Debit_Amount, creditAmount = d.Credit_Amount, lineType = d.Line_Type, description = d.Description
            }).ToListAsync(cancellationToken);

        return Ok(new { header, details });
    }

    /// <summary>تحويل شعار الشركة المخزن في قاعدة البيانات لصيغة قابلة للعرض والطباعة.</summary>
    private static string? ToImageDataUri(byte[]? image)
    {
        if (image is not { Length: > 0 }) return null;
        var mime = image.Length >= 8 && image[0] == 0x89 && image[1] == 0x50 ? "image/png"
            : image.Length >= 3 && image[0] == 0xFF && image[1] == 0xD8 ? "image/jpeg" : "image/png";
        return $"data:{mime};base64,{Convert.ToBase64String(image)}";
    }
}
