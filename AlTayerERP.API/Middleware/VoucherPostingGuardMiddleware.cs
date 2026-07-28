using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Middleware;

/// <summary>
/// يعيد التحقق من سلامة السند المحاسبية مباشرة قبل الترحيل.
/// وجود هذا الحارس يمنع ترحيل سند قديم أصبحت حساباته أو صندوقه أو بنكه موقوفة
/// بعد الحفظ وقبل تنفيذ عملية الترحيل.
/// </summary>
public sealed class VoucherPostingGuardMiddleware
{
    private readonly RequestDelegate _next;

    public VoucherPostingGuardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (!HttpMethods.IsPost(context.Request.Method) ||
            !TryGetVoucherId(context.Request.Path, out long voucherId))
        {
            await _next(context);
            return;
        }

        var voucher = await db.Financial_Voucher_Headers.AsNoTracking()
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Voucher_ID == voucherId, context.RequestAborted);

        if (voucher == null)
        {
            await RejectAsync(context, "السند المالي غير موجود.", StatusCodes.Status404NotFound);
            return;
        }

        var voucherTypeCode = await db.Voucher_Types.AsNoTracking()
            .Where(x => x.Voucher_Type_ID == voucher.Voucher_Type_ID && x.Is_Active)
            .Select(x => x.Voucher_Type_Code)
            .SingleOrDefaultAsync(context.RequestAborted);

        if (string.IsNullOrWhiteSpace(voucherTypeCode))
        {
            await RejectAsync(context, "نوع السند موقوف أو غير موجود.");
            return;
        }

        voucherTypeCode = voucherTypeCode.Trim().ToUpperInvariant();
        bool isReceipt = voucherTypeCode == "RECEIPT";
        bool isPayment = voucherTypeCode == "PAYMENT";
        bool isJournal = voucherTypeCode == "JOURNAL";

        if (!isReceipt && !isPayment && !isJournal)
        {
            await RejectAsync(context, "نوع السند غير مدعوم في الترحيل المحاسبي.");
            return;
        }

        if (!int.TryParse(voucher.Branch_ID, out int branchId))
        {
            await RejectAsync(context, "معرف فرع السند غير صالح.");
            return;
        }

        var branch = await db.Tenant_Branches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Branch_ID == branchId && x.Is_Active, context.RequestAborted);
        if (branch == null)
        {
            await RejectAsync(context, "فرع السند موقوف أو غير موجود.");
            return;
        }

        var accountIds = voucher.Details
            .Select(x => x.Account_ID?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Cast<string>()
            .ToList();

        if (!isJournal && !string.IsNullOrWhiteSpace(voucher.Cash_Account_ID))
            accountIds.Add(voucher.Cash_Account_ID.Trim());

        accountIds = accountIds.Distinct().ToList();

        var accounts = await db.Chart_Of_Accounts.AsNoTracking()
            .Where(x => x.Company_ID == branch.Company_ID && accountIds.Contains(x.Account_ID))
            .Select(x => new
            {
                x.Account_ID,
                x.Account_Type,
                x.Account_Category,
                x.Normal_Balance,
                x.Is_Active,
                x.Is_Postable,
                x.Is_Summary_Account,
                x.Allow_ManualEntry,
                x.Is_Control_Account
            })
            .ToListAsync(context.RequestAborted);

        if (accounts.Count != accountIds.Count)
        {
            await RejectAsync(context, "لا يمكن الترحيل: يوجد حساب لا يتبع شركة الفرع أو لم يعد موجودًا.");
            return;
        }

        if (accounts.Any(x => !x.Is_Active || !x.Is_Postable || x.Is_Summary_Account))
        {
            await RejectAsync(context, "لا يمكن الترحيل: جميع الحسابات يجب أن تكون نشطة ونهائية وقابلة للترحيل.");
            return;
        }

        if (accounts.Any(x => !x.Allow_ManualEntry || x.Is_Control_Account))
        {
            await RejectAsync(context, "لا يمكن ترحيل سند يدوي يحتوي على حساب رقابي أو حساب يمنع الإدخال اليدوي.");
            return;
        }

        if (!isJournal)
        {
            if (string.IsNullOrWhiteSpace(voucher.Cash_Account_ID))
            {
                await RejectAsync(context, "حساب الصندوق أو البنك غير محدد في السند.");
                return;
            }

            string cashAccountId = voucher.Cash_Account_ID.Trim();
            var cashAccount = accounts.SingleOrDefault(x => x.Account_ID == cashAccountId);
            if (cashAccount == null ||
                !string.Equals(cashAccount.Account_Type, "Asset", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(cashAccount.Normal_Balance, "Debit", StringComparison.OrdinalIgnoreCase) ||
                !(string.Equals(cashAccount.Account_Category, "Cash", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(cashAccount.Account_Category, "Bank", StringComparison.OrdinalIgnoreCase)))
            {
                await RejectAsync(context, "حساب الصندوق أو البنك لا يطابق تصنيف النقدية المعتمد.");
                return;
            }

            bool activeCashBox = await db.Cash_Boxes.AsNoTracking().AnyAsync(x =>
                x.Company_ID == branch.Company_ID &&
                x.Branch_ID == branchId &&
                x.Account_ID == cashAccountId &&
                x.Is_Active, context.RequestAborted);

            bool activeBank = await db.Bank_Accounts.AsNoTracking().AnyAsync(x =>
                x.Company_ID == branch.Company_ID &&
                x.GL_Account == cashAccountId &&
                x.Is_Active, context.RequestAborted);

            if (!activeCashBox && !activeBank)
            {
                await RejectAsync(context, "لا يمكن الترحيل: حساب النقدية غير مرتبط بصندوق أو حساب بنكي نشط.");
                return;
            }

            var cashLines = voucher.Details.Where(x => x.Line_Type == 1).ToList();
            if (cashLines.Count != 1 || cashLines[0].Account_ID?.Trim() != cashAccountId)
            {
                await RejectAsync(context, "يجب وجود سطر نقدية واحد مطابق لحساب رأس السند.");
                return;
            }

            if (isReceipt && (cashLines[0].Debit_Amount <= 0 || cashLines[0].Credit_Amount != 0))
            {
                await RejectAsync(context, "اتجاه حساب النقدية في سند القبض غير صحيح؛ يجب أن يكون مدينًا.");
                return;
            }

            if (isPayment && (cashLines[0].Credit_Amount <= 0 || cashLines[0].Debit_Amount != 0))
            {
                await RejectAsync(context, "اتجاه حساب النقدية في سند الصرف غير صحيح؛ يجب أن يكون دائنًا.");
                return;
            }
        }

        await _next(context);
    }

    private static bool TryGetVoucherId(PathString path, out long voucherId)
    {
        voucherId = 0;
        string[] parts = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        return parts.Length == 4 &&
               parts[0].Equals("api", StringComparison.OrdinalIgnoreCase) &&
               parts[1].Equals("FinancialVoucher", StringComparison.OrdinalIgnoreCase) &&
               parts[3].Equals("post", StringComparison.OrdinalIgnoreCase) &&
               long.TryParse(parts[2], out voucherId) &&
               voucherId > 0;
    }

    private static async Task RejectAsync(HttpContext context, string message, int statusCode = StatusCodes.Status400BadRequest)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { success = false, message }, context.RequestAborted);
    }
}
