using AlTayerERP.API.DTOs.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services.Accounting;

/// <summary>
/// خدمة التحقق من صحة السندات المالية قبل الحفظ أو الترحيل.
/// </summary>
public class VoucherValidationService
{
    private const string CanonicalMySqlCollation = "utf8mb4_unicode_ci";
    private readonly AppDbContext _context;

    public VoucherValidationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool IsValid, string ErrorMessage)> ValidateAsync(CreateFinancialVoucherDto voucher)
    {
        var result = ValidateDebitCredit(voucher);
        if (!result.IsValid)
            return result;

        if (string.IsNullOrWhiteSpace(voucher.Branch_ID) ||
            voucher.Fiscal_Year_ID <= 0 ||
            string.IsNullOrWhiteSpace(voucher.Against_Text))
            return (false, "الفرع والسنة والبيان المحاسبي حقول إلزامية.");

        if (voucher.Details.Select(x => x.Line_No).Distinct().Count() != voucher.Details.Count ||
            voucher.Details.Any(x => x.Line_No <= 0 || string.IsNullOrWhiteSpace(x.Account_ID)))
            return (false, "أرقام سطور السند أو حساباته غير صالحة.");

        var voucherTypeCode = await _context.Voucher_Types.AsNoTracking()
            .Where(x => x.Voucher_Type_ID == voucher.Voucher_Type_ID && x.Is_Active)
            .Select(x => x.Voucher_Type_Code)
            .SingleOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(voucherTypeCode))
            return (false, "نوع السند غير موجود أو غير فعال.");

        voucherTypeCode = voucherTypeCode.Trim().ToUpperInvariant();
        bool isJournal = voucherTypeCode == "JOURNAL";
        bool isReceipt = voucherTypeCode == "RECEIPT";
        bool isPayment = voucherTypeCode == "PAYMENT";

        if (!isJournal && !isReceipt && !isPayment)
            return (false, "نوع السند غير معتمد في المحرك المالي.");

        if (!isJournal && (string.IsNullOrWhiteSpace(voucher.Cash_Account_ID) ||
                           string.IsNullOrWhiteSpace(voucher.Received_From_Name)))
            return (false, "حساب الصندوق أو البنك واسم الطرف مطلوبان لسندي القبض والصرف.");

        if (!isJournal && !voucher.Payment_Method_ID.HasValue)
            return (false, "طريقة السداد مطلوبة لسندي القبض والصرف.");

        // القيد اليومي لا يحمل حساب نقدية في رأس السند؛ جميع الحسابات تأتي من السطور.
        if (isJournal && !string.IsNullOrWhiteSpace(voucher.Cash_Account_ID))
            return (false, "لا يجوز تحديد حساب صندوق أو بنك في رأس القيد اليومي.");

        if (voucher.Exchange_Rate <= 0 || voucher.Currency_ID <= 0)
            return (false, "العملة وسعر الصرف يجب أن يكونا صالحين.");

        if (!int.TryParse(voucher.Branch_ID, out int branchId))
            return (false, "معرف الفرع غير صالح.");

        var branch = await _context.Tenant_Branches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Branch_ID == branchId && x.Is_Active);
        if (branch == null)
            return (false, "الفرع المحدد غير موجود أو غير فعال.");

        bool fiscalYearIsValid = await _context.Fiscal_Years.AsNoTracking().AnyAsync(x =>
            x.Fiscal_Year_ID == voucher.Fiscal_Year_ID &&
            EF.Functions.Collate(x.Company_ID, CanonicalMySqlCollation) == branch.Company_ID &&
            x.Is_Active && !x.Is_Closed);
        if (!fiscalYearIsValid)
            return (false, "السنة المالية لا تتبع الفرع الحالي أو أنها مقفلة/غير فعالة.");

        bool voucherStatusIsActive = await _context.Voucher_Statuses.AsNoTracking()
            .AnyAsync(x => x.Voucher_Status_ID == voucher.Voucher_Status_ID && x.Is_Active);
        if (!voucherStatusIsActive)
            return (false, "حالة السند غير موجودة أو غير فعالة.");

        var currency = await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(x =>
            x.Currency_ID == voucher.Currency_ID &&
            EF.Functions.Collate(x.Company_ID, CanonicalMySqlCollation) == branch.Company_ID &&
            x.Is_Active);
        if (currency == null)
            return (false, "عملة السند غير موجودة أو غير فعالة ضمن الشركة.");

        if (currency.Is_Local_Currency && voucher.Exchange_Rate != 1m)
            return (false, "سعر صرف العملة المحلية يجب أن يساوي 1.");

        if (!currency.Is_Local_Currency &&
            ((currency.Min_Exchange_Rate.HasValue && voucher.Exchange_Rate < currency.Min_Exchange_Rate.Value) ||
             (currency.Max_Exchange_Rate.HasValue && voucher.Exchange_Rate > currency.Max_Exchange_Rate.Value)))
            return (false, "سعر الصرف خارج الحدود المسموح بها للعملة.");

        if (voucher.Payment_Method_ID.HasValue &&
            !await _context.Payment_Methods.AsNoTracking().AnyAsync(x =>
                x.Payment_Method_ID == voucher.Payment_Method_ID.Value && x.Is_Active))
            return (false, "طريقة السداد غير موجودة أو غير فعالة.");

        var accountIds = voucher.Details.Select(x => x.Account_ID.Trim())
            .Concat(isJournal ? Enumerable.Empty<string>() : new[] { voucher.Cash_Account_ID.Trim() })
            .Distinct()
            .ToList();

        var availableAccounts = await _context.Chart_Of_Accounts.AsNoTracking()
            .Where(x => accountIds.Contains(EF.Functions.Collate(x.Account_ID, CanonicalMySqlCollation)) &&
                        EF.Functions.Collate(x.Company_ID, CanonicalMySqlCollation) == branch.Company_ID)
            .Select(x => new
            {
                x.Account_ID,
                x.Account_Type,
                x.Account_Category,
                x.Normal_Balance,
                x.Currency_Code,
                x.Multi_Currency,
                x.Is_Active,
                x.Is_Postable,
                x.Is_Summary_Account,
                x.Allow_ManualEntry,
                x.Is_Control_Account
            })
            .ToListAsync();

        if (availableAccounts.Count != accountIds.Count)
            return (false, "يوجد حساب غير موجود ضمن الشركة الحالية.");

        var invalidAccount = availableAccounts.FirstOrDefault(x =>
            !x.Is_Active || !x.Is_Postable || x.Is_Summary_Account);
        if (invalidAccount != null)
            return (false, "جميع حسابات السند يجب أن تكون نشطة ونهائية وقابلة للترحيل.");

        var manualBlocked = availableAccounts.FirstOrDefault(x =>
            !x.Allow_ManualEntry || x.Is_Control_Account);
        if (manualBlocked != null)
            return (false, "لا يجوز استخدام حساب رقابي أو حساب يمنع الإدخال اليدوي في سند يدوي.");

        var currencyMismatch = availableAccounts.FirstOrDefault(x =>
            !x.Multi_Currency &&
            !string.IsNullOrWhiteSpace(x.Currency_Code) &&
            !string.Equals(x.Currency_Code, currency.Currency_Code, StringComparison.OrdinalIgnoreCase));
        if (currencyMismatch != null)
            return (false, "عملة أحد حسابات السند لا تطابق عملة السند.");

        var cashLines = voucher.Details.Where(x => x.Line_Type == 1).ToList();
        if (!isJournal && (cashLines.Count != 1 ||
                           !string.Equals(cashLines[0].Account_ID?.Trim(), voucher.Cash_Account_ID.Trim(), StringComparison.Ordinal)))
            return (false, "يجب وجود سطر صندوق/بنك واحد فقط ومطابق لحساب الصندوق في رأس السند.");

        if (isJournal && cashLines.Count != 0)
            return (false, "القيد اليومي لا يحتوي سطر صندوق/بنك؛ استخدم سطوراً محاسبية عادية.");

        if (!isJournal && voucher.Details.Any(x =>
                x.Line_Type != 1 &&
                string.Equals(x.Account_ID.Trim(), voucher.Cash_Account_ID.Trim(), StringComparison.Ordinal)))
            return (false, "لا يجوز استخدام حساب الصندوق أو البنك نفسه كحساب مقابل في سند القبض أو الصرف.");

        if (!isJournal)
        {
            string cashAccountId = voucher.Cash_Account_ID.Trim();
            var cashAccount = availableAccounts.Single(x => x.Account_ID == cashAccountId);

            bool validCashCategory =
                string.Equals(cashAccount.Account_Type, "Asset", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cashAccount.Normal_Balance, "Debit", StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(cashAccount.Account_Category, "Cash", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(cashAccount.Account_Category, "Bank", StringComparison.OrdinalIgnoreCase));

            if (!validCashCategory)
                return (false, "حساب التحصيل أو الدفع يجب أن يكون حساب نقدية أو بنك معتمداً من نوع الأصول وطبيعته مدينة.");

            bool linkedToActiveCashBox = await _context.Cash_Boxes.AsNoTracking().AnyAsync(x =>
                x.Company_ID == branch.Company_ID &&
                x.Branch_ID == branchId &&
                x.Account_ID == cashAccountId &&
                x.Is_Active);

            bool linkedToActiveBank = await _context.Bank_Accounts.AsNoTracking().AnyAsync(x =>
                x.Company_ID == branch.Company_ID &&
                x.GL_Account == cashAccountId &&
                x.Is_Active);

            if (!linkedToActiveCashBox && !linkedToActiveBank)
                return (false, "حساب التحصيل أو الدفع غير مرتبط بصندوق أو حساب بنكي نشط ضمن الشركة والفرع الحاليين.");

            if (isReceipt && (cashLines[0].Debit_Amount <= 0 || cashLines[0].Credit_Amount != 0))
                return (false, "سند القبض يجب أن يجعل حساب الصندوق أو البنك مديناً.");

            if (isPayment && (cashLines[0].Credit_Amount <= 0 || cashLines[0].Debit_Amount != 0))
                return (false, "سند الصرف يجب أن يجعل حساب الصندوق أو البنك دائناً.");
        }

        bool periodIsOpen = await _context.Fiscal_Periods.AsNoTracking().AnyAsync(x =>
            x.Branch_ID == branchId &&
            x.Fiscal_Year_ID == voucher.Fiscal_Year_ID &&
            x.Is_Active && !x.Is_Closed &&
            x.Start_Date.Date <= voucher.Transaction_Date.Date &&
            x.End_Date.Date >= voucher.Transaction_Date.Date);
        if (!periodIsOpen)
            return (false, "لا توجد فترة مالية مفتوحة لتاريخ الحركة ضمن الفرع والسنة الحالية.");

        return (true, string.Empty);
    }

    private static (bool IsValid, string ErrorMessage) ValidateDebitCredit(CreateFinancialVoucherDto voucher)
    {
        if (voucher.Details == null || voucher.Details.Count < 2)
            return (false, "يجب أن يحتوي السند على سطرين محاسبيين على الأقل.");

        foreach (var detail in voucher.Details)
        {
            if (detail.Debit_Amount < 0 || detail.Credit_Amount < 0)
                return (false, $"لا يسمح بمبلغ سالب في السطر رقم {detail.Line_No}.");

            if (detail.Debit_Amount > 0 && detail.Credit_Amount > 0)
                return (false, $"لا يمكن أن يكون السطر رقم {detail.Line_No} مدينًا ودائنًا معًا.");

            if (detail.Debit_Amount == 0 && detail.Credit_Amount == 0)
                return (false, $"يجب إدخال مبلغ مدين أو دائن في السطر رقم {detail.Line_No}.");

            if (detail.Currency_ID <= 0 || detail.Exchange_Rate <= 0)
                return (false, $"العملة أو سعر الصرف غير صالح في السطر رقم {detail.Line_No}.");

            if (detail.Foreign_Amount < 0 || detail.Local_Amount <= 0)
                return (false, $"المبلغ الأجنبي أو المحلي غير صالح في السطر رقم {detail.Line_No}.");

            decimal accountingAmount = detail.Debit_Amount > 0
                ? detail.Debit_Amount
                : detail.Credit_Amount;

            if (decimal.Round(detail.Local_Amount, 2) != decimal.Round(accountingAmount, 2))
                return (false,
                    $"المبلغ المحلي في السطر رقم {detail.Line_No} لا يطابق قيمة المدين أو الدائن.");
        }

        decimal totalDebit = voucher.Details.Sum(x => x.Debit_Amount);
        decimal totalCredit = voucher.Details.Sum(x => x.Credit_Amount);

        if (decimal.Round(totalDebit, 2) <= 0 || decimal.Round(totalCredit, 2) <= 0)
            return (false, "إجمالي المدين والدائن يجب أن يكون أكبر من صفر.");

        if (decimal.Round(totalDebit, 2) != decimal.Round(totalCredit, 2))
        {
            decimal difference = decimal.Round(totalDebit - totalCredit, 2);
            return (false,
                $"لا يمكن حفظ السند لأن إجمالي المدين ({totalDebit:N2}) لا يساوي إجمالي الدائن ({totalCredit:N2}). الفرق: {difference:N2}");
        }

        return (true, string.Empty);
    }
}
