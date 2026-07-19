using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    // ======================================================
    // خدمة توليد الأرقام المركزية في النظام
    // تعتمد على جدولين:
    // 1) numbering_settings  = قواعد الترقيم
    // 2) numbering_counters  = العدادات الفعلية لكل شركة/فرع/سنة
    // ======================================================
    public class NumberGeneratorService
    {
        // الاتصال بقاعدة البيانات
        private readonly AppDbContext _context;

        public NumberGeneratorService(AppDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // توليد رقم بدون شركة
        // يستخدم للشركات أو المستندات العامة
        // مثال: CO-00001
        // ======================================================
        public async Task<string> GenerateNextNumberAsync(string documentType)
        {
            return await GenerateNextNumberInternalAsync(
                documentType,
                companyId: null,
                branchId: null);
        }

        // ======================================================
        // توليد رقم حسب الشركة
        // يستخدم للفروع والبوالص والتذاكر والسندات
        // مثال: SHF-BR-0001
        // ======================================================
        public async Task<string> GenerateNextNumberAsync(string documentType, string companyId)
        {
            return await GenerateNextNumberInternalAsync(
                documentType,
                companyId,
                branchId: null);
        }

        // ======================================================
        // الدالة الداخلية الأساسية لتوليد الرقم
        // ======================================================
        private async Task<string> GenerateNextNumberInternalAsync(
            string documentType,
            string? companyId,
            int? branchId)
        {
            // جلب إعدادات الترقيم (معدّل ليشمل Trim و ToUpper)
            var setting = await _context.Numbering_Settings
                .FirstOrDefaultAsync(x =>
                    x.Document_Type.Trim().ToUpper() == documentType.Trim().ToUpper() &&
                    x.Is_Active);

            if (setting == null)
                throw new Exception("لا توجد إعدادات ترقيم لهذا النوع: " + documentType);

            // تحديد السنة إذا كان الإعداد يستخدم السنة
            int? yearValue = setting.Use_Year ? DateTime.Now.Year : null;

            // إذا كان الترقيم حسب الشركة ولا توجد شركة، نوقف العملية
            if (setting.Use_Company && string.IsNullOrWhiteSpace(companyId))
                throw new Exception("هذا النوع يحتاج شركة لتوليد الرقم.");

            // جلب الشركة إذا وجدت
            Company? company = null;

            if (!string.IsNullOrWhiteSpace(companyId))
            {
                company = await _context.Companies
                    .FirstOrDefaultAsync(x => x.Company_ID == companyId);

                if (company == null)
                    throw new Exception("لم يتم العثور على الشركة المرتبطة بهذا الرقم.");
            }

            // البحث عن عداد مطابق (معدّل ليشمل Trim و ToUpper في Document_Type)
            var counter = await _context.Numbering_Counters
                .FirstOrDefaultAsync(x =>
                    x.Document_Type.Trim().ToUpper() == documentType.Trim().ToUpper() &&
                    x.Company_ID == (setting.Use_Company ? companyId : null) &&
                    x.Branch_ID == (setting.Use_Branch ? branchId : null) &&
                    x.Year_Value == yearValue);

            // إذا لم يوجد عداد، ننشئه
            if (counter == null)
            {
                counter = new NumberingCounter
                {
                    Document_Type = documentType,
                    Company_ID = setting.Use_Company ? companyId : null,
                    Branch_ID = setting.Use_Branch ? branchId : null,
                    Year_Value = yearValue,
                    Last_Number = 0,
                    Created_At = DateTime.Now
                };

                await _context.Numbering_Counters.AddAsync(counter);
            }

            // زيادة آخر رقم
            counter.Last_Number += 1;
            counter.Updated_At = DateTime.Now;

            // تجهيز الرقم بالأصفار
            string numberPart = counter.Last_Number
                .ToString()
                .PadLeft(setting.Digits_Count, '0');

            // تكوين البادئة النهائية
            string prefix = BuildPrefix(setting.Prefix, company);

            // الرقم النهائي
            string finalNumber = $"{prefix}-{numberPart}";

            // حفظ العداد
            await _context.SaveChangesAsync();

            return finalNumber;
        }

        // ======================================================
        // تكوين البادئة النهائية
        // إذا وجدت شركة ورمزها موجود:
        // SHF + BR = SHF-BR
        // إذا لا توجد شركة:
        // CO
        // ======================================================
        private string BuildPrefix(string documentPrefix, Company? company)
        {
            if (company != null && !string.IsNullOrWhiteSpace(company.Company_Prefix))
            {
                return $"{company.Company_Prefix.Trim().ToUpper()}-{documentPrefix.Trim().ToUpper()}";
            }

            return documentPrefix.Trim().ToUpper();
        }
    }
}