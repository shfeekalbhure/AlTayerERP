using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// يضيف القيم المرجعية الأساسية لسند القبض عند قاعدة بيانات جديدة فقط.
    /// لا يحدّث السجلات الموجودة ولا يحذفها.
    /// </summary>
    public sealed class VoucherReferenceDataSeeder
    {
        private readonly AppDbContext _context;

        public VoucherReferenceDataSeeder(AppDbContext context) => _context = context;

        public async Task EnsureSeededAsync()
        {
            await EnsurePaymentMethodsAsync();
            await EnsureVoucherTypesAsync();
            await EnsureVoucherStatusesAsync();
            await _context.SaveChangesAsync();
        }

        private async Task EnsurePaymentMethodsAsync()
        {
            await AddPaymentMethodIfMissingAsync("CASH", "نقدي", true, false, false, false, 1);
            await AddPaymentMethodIfMissingAsync("CHEQUE", "شيك", false, true, true, true, 2);
            await AddPaymentMethodIfMissingAsync("BANK_TRANSFER", "تحويل بنكي", false, true, true, true, 3);
        }

        private async Task AddPaymentMethodIfMissingAsync(
            string code,
            string arabicName,
            bool isCash,
            bool isBank,
            bool requiresReference,
            bool requiresReferenceDate,
            int sortOrder)
        {
            if (await _context.Payment_Methods.AnyAsync(x => x.Payment_Method_Code == code))
                return;

            _context.Payment_Methods.Add(new PaymentMethod
            {
                Payment_Method_Code = code,
                Payment_Method_Name_AR = arabicName,
                Is_Cash = isCash,
                Is_Bank = isBank,
                Requires_Reference = requiresReference,
                Requires_Reference_Date = requiresReferenceDate,
                Is_Active = true,
                Sort_Order = sortOrder
            });
        }

        private async Task EnsureVoucherTypesAsync()
        {
            await AddVoucherTypeIfMissingAsync("RECEIPT", "سند قبض", "Receipt Voucher", 1);
            await AddVoucherTypeIfMissingAsync("PAYMENT", "سند صرف", "Payment Voucher", 2);
            await AddVoucherTypeIfMissingAsync("JOURNAL", "قيد يومية", "Journal Voucher", 3);
            await AddVoucherTypeIfMissingAsync("ADJUSTMENT", "قيد تسوية", "Adjustment Voucher", 4);
            await AddVoucherTypeIfMissingAsync("OPENING", "قيد افتتاحي", "Opening Voucher", 5);
        }

        private async Task AddVoucherTypeIfMissingAsync(string code, string nameAr, string nameEn, int sortOrder)
        {
            if (await _context.Voucher_Types.AnyAsync(x => x.Voucher_Type_Code == code))
                return;

            _context.Voucher_Types.Add(new VoucherType
            {
                Voucher_Type_Code = code,
                Voucher_Type_Name_AR = nameAr,
                Voucher_Type_Name_EN = nameEn,
                Is_Active = true,
                Sort_Order = sortOrder
            });
        }

        private async Task EnsureVoucherStatusesAsync()
        {
            await AddVoucherStatusIfMissingAsync("DRAFT", "مسودة", 1);
            await AddVoucherStatusIfMissingAsync("PENDING", "معلق", 2);
            await AddVoucherStatusIfMissingAsync("REVIEWED", "تمت المراجعة", 3);
            await AddVoucherStatusIfMissingAsync("RETURNED", "معاد للتصحيح", 4);
            await AddVoucherStatusIfMissingAsync("APPROVED", "معتمد", 5);
            await AddVoucherStatusIfMissingAsync("POSTED", "مرحل", 6);
            await AddVoucherStatusIfMissingAsync("CANCELLED", "ملغي", 7);
            await AddVoucherStatusIfMissingAsync("REVERSED", "معكوس", 8);
        }

        private async Task AddVoucherStatusIfMissingAsync(string code, string arabicName, int sortOrder)
        {
            if (await _context.Voucher_Statuses.AnyAsync(x => x.Voucher_Status_Code == code))
                return;

            _context.Voucher_Statuses.Add(new VoucherStatus
            {
                Voucher_Status_Code = code,
                Voucher_Status_Name_AR = arabicName,
                Is_Active = true,
                Sort_Order = sortOrder
            });
        }
    }
}
