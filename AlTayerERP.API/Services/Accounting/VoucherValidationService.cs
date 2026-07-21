using AlTayerERP.API.DTOs.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services.Accounting
{
    /// <summary>
    /// خدمة التحقق من صحة السندات المالية قبل الحفظ أو الترحيل.
    /// </summary>
    public class VoucherValidationService
    {
        private readonly AppDbContext _context;

        public VoucherValidationService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// تنفيذ قواعد التحقق الأساسية للسند.
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateAsync(
            CreateFinancialVoucherDto voucher)
        {
            var result = ValidateDebitCredit(voucher);

            if (!result.IsValid)
                return result;

            // قواعد سند القبض لا تثق بما تعرضه الواجهة؛ تتحقق من الطريقة
            // والصندوق والسطر النقدي في قاعدة البيانات قبل إنشاء أي حركة.
            result = await ValidateReceiptVoucherAsync(voucher);
            if (!result.IsValid)
                return result;

            return (true, string.Empty);
        }

        /// <summary>
        /// قواعد سند القبض: حساب صندوق/بنك نشط، طريقة سداد صالحة،
        /// وسطر نقدي واحد مطابق للرأس والمبلغ المحلي.
        /// </summary>
        private async Task<(bool IsValid, string ErrorMessage)> ValidateReceiptVoucherAsync(
            CreateFinancialVoucherDto voucher)
        {
            // لا تطبق قواعد اتجاه القبض على الأنواع الأخرى التي ستأتي في مراحل لاحقة.
            if (voucher.Voucher_Type_ID != 1)
                return (true, string.Empty);

            if (!int.TryParse(voucher.Branch_ID, out int branchId))
                return (false, "فرع السند غير صالح.");

            bool cashAccountIsAvailable = await _context.Cash_Boxes.AsNoTracking()
                .AnyAsync(x => x.Account_ID == voucher.Cash_Account_ID &&
                               x.Branch_ID == branchId &&
                               x.Is_Active);
            if (!cashAccountIsAvailable)
            {
                return (false, "حساب الصندوق أو البنك غير نشط أو لا يتبع فرع السند.");
            }

            if (!voucher.Payment_Method_ID.HasValue)
                return (false, "طريقة السداد مطلوبة في سند القبض.");

            var paymentMethod = await _context.Payment_Methods.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Payment_Method_ID == voucher.Payment_Method_ID.Value && x.Is_Active);
            if (paymentMethod == null)
                return (false, "طريقة السداد المختارة غير موجودة أو غير نشطة.");

            if (paymentMethod.Requires_Reference && string.IsNullOrWhiteSpace(voucher.Reference_No))
                return (false, "رقم المرجع مطلوب لطريقة السداد المختارة.");

            if (paymentMethod.Requires_Reference_Date && !voucher.Reference_Date.HasValue)
                return (false, "تاريخ المرجع مطلوب لطريقة السداد المختارة.");

            var cashLines = voucher.Details.Where(x => x.Line_Type == 1).ToList();
            if (cashLines.Count != 1)
                return (false, "يجب أن يحتوي سند القبض على سطر صندوق أو بنك واحد فقط.");

            var cashLine = cashLines[0];
            if (!string.Equals(cashLine.Account_ID, voucher.Cash_Account_ID, StringComparison.Ordinal))
                return (false, "حساب سطر الصندوق لا يطابق حساب الصندوق أو البنك في رأس السند.");

            if (cashLine.Currency_ID != voucher.Currency_ID)
                return (false, "عملة سطر الصندوق لا تطابق عملة رأس سند القبض.");

            if (cashLine.Debit_Amount <= 0m || cashLine.Credit_Amount != 0m)
                return (false, "سطر الصندوق في سند القبض يجب أن يكون مديناً فقط.");

            decimal expectedLocalAmount = decimal.Round(voucher.Amount * voucher.Exchange_Rate, 2);
            if (decimal.Round(cashLine.Local_Amount, 2) != expectedLocalAmount ||
                decimal.Round(cashLine.Debit_Amount, 2) != expectedLocalAmount)
            {
                return (false, "مبلغ سطر الصندوق لا يطابق مبلغ سند القبض بعد تحويل العملة.");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// التحقق من أن إجمالي المدين يساوي إجمالي الدائن
        /// وأن كل سطر يحتوي على طرف واحد فقط.
        /// </summary>
        private static (bool IsValid, string ErrorMessage) ValidateDebitCredit(
            CreateFinancialVoucherDto voucher)
        {
            if (voucher.Details == null || voucher.Details.Count < 2)
            {
                return (
                    false,
                    "يجب أن يحتوي السند على سطرين محاسبيين على الأقل."
                );
            }

            foreach (var detail in voucher.Details)
            {
                if (detail.Debit_Amount < 0 || detail.Credit_Amount < 0)
                {
                    return (
                        false,
                        $"لا يسمح بمبلغ سالب في السطر رقم {detail.Line_No}."
                    );
                }

                if (detail.Debit_Amount > 0 && detail.Credit_Amount > 0)
                {
                    return (
                        false,
                        $"لا يمكن أن يكون السطر رقم {detail.Line_No} مدينًا ودائنًا معًا."
                    );
                }

                if (detail.Debit_Amount == 0 && detail.Credit_Amount == 0)
                {
                    return (
                        false,
                        $"يجب إدخال مبلغ مدين أو دائن في السطر رقم {detail.Line_No}."
                    );
                }

                if (detail.Local_Amount <= 0)
                {
                    return (
                        false,
                        $"المبلغ المحلي في السطر رقم {detail.Line_No} يجب أن يكون أكبر من صفر."
                    );
                }
            }

            decimal totalDebit = voucher.Details.Sum(x => x.Debit_Amount);
            decimal totalCredit = voucher.Details.Sum(x => x.Credit_Amount);

            if (decimal.Round(totalDebit, 2) != decimal.Round(totalCredit, 2))
            {
                decimal difference = decimal.Round(
                    totalDebit - totalCredit,
                    2
                );

                return (
                    false,
                    $"لا يمكن حفظ السند لأن إجمالي المدين ({totalDebit:N2}) " +
                    $"لا يساوي إجمالي الدائن ({totalCredit:N2}). " +
                    $"الفرق: {difference:N2}"
                );
            }

            return (true, string.Empty);
        }
    }
}