using AlTayerERP.API.DTOs.Accounting;
using AlTayerERP.Infrastructure.Data;

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

            // سنضيف بقية قواعد التحقق هنا خطوة خطوة.
            await Task.CompletedTask;

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