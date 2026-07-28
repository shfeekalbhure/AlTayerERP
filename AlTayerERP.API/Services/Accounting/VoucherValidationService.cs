using AlTayerERP.API.DTOs.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services.Accounting;

/// <summary>
/// خدمة التحقق من صحة السندات المالية قبل الحفظ أو الترحيل.
/// </summary>
public class VoucherValidationService
{
    // توحيد المقارنات النصية مع الجداول القديمة التي قد تستخدم Collation مختلفاً.
    // وضع COLLATE على العمود يجعل MySQL يقارن قيمة المعامل بنفس القاعدة دون تعديل البيانات.
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
        {
            return (false, "الفرع والسنة والبيان المحاسبي حقول إلزامية.");
        }

        var voucherTypeCode = await _context.Voucher_Types.AsNoTracking()
            .Where(x => x.Voucher_Type_ID == voucher.Voucher_Type_ID && x.Is_Active)
            .Select(x => x.Voucher_Type_Code)
            .SingleOrDefaultAsync();

        var isJournal = string.Equals(voucherTypeCode, "JOURNAL", StringComparison.OrdinalIgnoreCase);
        if (!isJournal && (string.IsNullOrWhiteSpace(voucher.Cash_Account_ID) ||
                           string.IsNullOrWhiteSpace(voucher.Received_From_Name)))
        {
            return (false, "حساب الصندوق أو البنك واسم الطرف مطلوبان لسندي القبض والصرف.");
        }

        if (!isJournal && !voucher.Payment_Method_ID.HasValue)
            return (false, "طريقة السداد مطلوبة لسندي القبض والصرف.");

        if (voucher.Exchange_Rate <= 0 || voucher.Currency_ID <= 0)
            return (false, "العملة وسعر الصرف يجب أن يكونا صالحين.");

        if (voucher.Details.Select(x => x.Line_No).Distinct().Count() != voucher.Details.Count ||
            voucher.Details.Any(x => x.Line_No <= 0 || string.IsNullOrWhiteSpace(x.Account_ID)))
        {
            return (false, "أرقام سطور السند أو حساباته غير صالحة.");
        }

        if (!int.TryParse(voucher.Branch_ID, out var branchId))
            return (false, "معرف الفرع غير صالح.");

        var branch = await _context.Tenant_Branches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Branch_ID == branchId && x.Is_Active);
        if (branch == null)
            return (false, "الفرع المحدد غير موجود أو غير فعال.");

        var fiscalYearIsValid = await _context.Fiscal_Years.AsNoTracking().AnyAsync(x =>
            x.Fiscal_Year_ID == voucher.Fiscal_Year_ID &&
            EF.Functions.Collate(x.Company_ID, CanonicalMySqlCollation) == branch.Company_ID &&
            x.Is_Active && !x.Is_Closed);
        if (!fiscalYearIsValid)
            return (false, "السنة المالية لا تتبع الفرع الحالي أو أنها مقفلة/غير فعالة.");

        if (string.IsNullOrWhiteSpace(voucherTypeCode))
            return (false, "نوع السند غير موجود أو غير فعال.");

        var voucherStatusIsActive = await _context.Voucher_Statuses.AsNoTracking()
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
        {
            return (false, "سعر الصرف خارج الحدود المسموح بها للعملة.");
        }

        if (voucher.Payment_Method_ID.HasValue &&
            !await _context.Payment_Methods.AsNoTracking().AnyAsync(x =>
                x.Payment_Method_ID == voucher.Payment_Method_ID.Value && x.Is_Active))
        {
            return (false, "طريقة السداد غير موجودة أو غير فعالة.");
        }

        var accountIds = voucher.Details.Select(x => x.Account_ID.Trim())
            .Concat(isJournal ? Enumerable.Empty<string>() : new[] { voucher.Cash_Account_ID.Trim() })
            .Distinct()
            .ToList();

        var availableAccounts = await _context.Chart_Of_Accounts.AsNoTracking()
            // يفرض الترميز الموحد على رقم الحساب والشركة؛ هذا يمنع خطأ
            // Illegal mix of collations عند مقارنة قواعد البيانات القديمة والجديدة.
            .Where(x => accountIds.Contains(EF.Functions.Collate(x.Account_ID, CanonicalMySqlCollation)) &&
                        EF.Functions.Collate(x.Company_ID, CanonicalMySqlCollation) == branch.Company_ID &&
                        x.Is_Active && x.Is_Postable)
            .Select(x => x.Account_ID)
            .ToListAsync();

        if (availableAccounts.Count != accountIds.Count)
            return (false, "يوجد حساب غير موجود أو غير نشط أو غير قابل للترحيل ضمن السند.");

        var cashLines = voucher.Details.Where(x => x.Line_Type == 1).ToList();
        if (!isJournal && (cashLines.Count != 1 ||
                           !string.Equals(cashLines[0].Account_ID?.Trim(), voucher.Cash_Account_ID.Trim(), StringComparison.Ordinal)))
        {
            return (false, "يجب وجود سطر صندوق/بنك واحد فقط ومطابق لحساب الصندوق في رأس السند.");
        }

        if (isJournal && cashLines.Count != 0)
            return (false, "القيد اليومي لا يحتوي سطر صندوق/بنك؛ استخدم سطوراً محاسبية عادية.");

        if (!isJournal && voucher.Details.Any(x =>
                x.Line_Type != 1 &&
                string.Equals(x.Account_ID.Trim(), voucher.Cash_Account_ID.Trim(), StringComparison.Ordinal)))
        {
            return (false, "لا يجوز استخدام حساب الصندوق أو البنك نفسه كحساب مقابل في سند القبض أو الصرف.");
        }

        var periodIsOpen = await _context.Fiscal_Periods.AsNoTracking().AnyAsync(x =>
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

            if (detail.Local_Amount <= 0)
                return (false, $"المبلغ المحلي في السطر رقم {detail.Line_No} يجب أن يكون أكبر من صفر.");
        }

        var totalDebit = voucher.Details.Sum(x => x.Debit_Amount);
        var totalCredit = voucher.Details.Sum(x => x.Credit_Amount);
        if (decimal.Round(totalDebit, 2) != decimal.Round(totalCredit, 2))
        {
            var difference = decimal.Round(totalDebit - totalCredit, 2);
            return (false,
                $"لا يمكن حفظ السند لأن إجمالي المدين ({totalDebit:N2}) لا يساوي إجمالي الدائن ({totalCredit:N2}). الفرق: {difference:N2}");
        }

        return (true, string.Empty);
    }
}
