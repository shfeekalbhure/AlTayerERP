using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartOfAccountsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AccountNumberService _accountNumberService;

        public ChartOfAccountsController(AppDbContext context, AccountNumberService accountNumberService)
        {
            _context = context;
            _accountNumberService = accountNumberService;
        }

        /// <summary> 
        /// جلب كافة الحسابات المالية التابعة لشركة محددة بشكل صارم 
        /// </summary> 
        [HttpGet]
        public async Task<IActionResult> GetAccounts([FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("معرف الشركة مطلوب.");
            try
            {
                var data = await _context.Chart_Of_Accounts
                    .AsNoTracking()
                    .Where(x => x.Company_ID == companyId.Trim())
                    .OrderBy(x => x.Account_Code)
                    .Select(x => new {
                        Account_ID = x.Account_ID ?? "",
                        Company_ID = x.Company_ID ?? "",
                        Parent_Account_ID = x.Parent_Account_ID ?? "",
                        Account_Code = x.Account_Code ?? "",
                        Account_Name_AR = x.Account_Name_AR ?? "",
                        Account_Name_EN = x.Account_Name_EN ?? "",
                        Account_Type = x.Account_Type ?? "",
                        Account_Category = x.Account_Category ?? "",
                        Normal_Balance = x.Normal_Balance ?? "",
                        Account_Level = x.Account_Level,
                        Is_Postable = x.Is_Postable,
                        Currency_Code = x.Currency_Code ?? "YER",
                        Is_Active = x.Is_Active,
                        Notes = x.Notes ?? "",
                        Allow_ManualEntry = x.Allow_ManualEntry,
                        System_Account = x.System_Account,
                        Requires_CostCenter = x.Requires_CostCenter,
                        Requires_Party = x.Requires_Party,
                        Requires_Project = x.Requires_Project,
                        Is_Summary_Account = x.Is_Summary_Account,
                        Affects_Balance_Sheet = x.Affects_Balance_Sheet,
                        Affects_Income_Statement = x.Affects_Income_Statement,
                        Multi_Currency = x.Multi_Currency,
                        Account_Path = x.Account_Path ?? "",
                        Account_Serial = x.Account_Serial,
                        Created_By = x.Created_By ?? "",
                        Updated_At = x.Updated_At,
                        Updated_By = x.Updated_By ?? ""
                    })
                    .ToListAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"فشل جلب دليل الحسابات: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary> 
        /// تعديل الملاحظة الأولى: جلب قائمة مصغرة وخفيفة للحسابات للاستخدام في المنسدلات والـ Lookups
        /// </summary> 
        /// <summary>
        /// جلب الحسابات المتاحة للاختيار في السندات والقيود.
        ///
        /// شروط الإحضار:
        /// 1- الحساب تابع للشركة الحالية.
        /// 2- الحساب نشط وغير موقوف.
        /// 3- الحساب قابل للترحيل.
        /// 4- الحساب ليس حسابًا رئيسيًا أو تجميعيًا.
        /// </summary>
        [HttpGet("GetLookup")]
        public async Task<IActionResult> GetLookup(
            [FromQuery] string companyId)
        {
            // التحقق من إرسال معرف الشركة.
            if (string.IsNullOrWhiteSpace(companyId))
            {
                return BadRequest("رقم الشركة مطلوب.");
            }

            try
            {
                // تنظيف معرف الشركة من المسافات الزائدة.
                string cleanCompanyId = companyId.Trim();

                // جلب الحسابات الفرعية النشطة والقابلة للترحيل فقط.
                var data = await _context.Chart_Of_Accounts
                    .AsNoTracking()
                    .Where(account =>

                        // الحسابات التابعة للشركة الحالية فقط.
                        account.Company_ID == cleanCompanyId &&

                        // استبعاد الحسابات الموقوفة.
                        account.Is_Active &&

                        // استبعاد الحسابات الرئيسية غير القابلة للترحيل.
                        account.Is_Postable &&

                        // استبعاد الحسابات التجميعية.
                        !account.Is_Summary_Account)

                    // ترتيب النتائج حسب رقم الحساب.
                    .OrderBy(account => account.Account_Code)

                    // إرجاع البيانات التي تحتاجها شاشة البحث فقط.
                    .Select(account => new
                    {
                        // معرف الحساب الحقيقي الذي سيحفظ في السند.
                        Account_ID =
                            account.Account_ID ?? string.Empty,

                        // رقم الحساب الظاهر للمستخدم.
                        Account_Code =
                            account.Account_Code ?? string.Empty,

                        // اسم الحساب العربي.
                        Account_Name_AR =
                            account.Account_Name_AR ?? string.Empty,

                        // اسم الحساب الإنجليزي للبحث عند توفره.
                        Account_Name_EN =
                            account.Account_Name_EN ?? string.Empty,

                        // مجموعة الحساب التي ستظهر في العمود الثالث.
                        Account_Group =
                            account.Account_Category ?? string.Empty
                    })
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    $"حدث خطأ أثناء جلب حسابات شاشة البحث: " +
                    $"{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary> 
        /// جلب حساب محدد مع التحقق الصارم من تبعيته للشركة 
        /// </summary> 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(string id, [FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("معرف الشركة مطلوب للأمان.");
            try
            {
                var account = await _context.Chart_Of_Accounts
                    .FirstOrDefaultAsync(x => x.Account_ID == id && x.Company_ID == companyId.Trim());
                if (account == null) return NotFound("الحساب غير موجود أو لا تملك صلاحية الوصول إليه.");
                return Ok(account);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"فشل جلب الحساب: {ex.Message}");
            }
        }

        /// <summary> 
        /// إنشاء حساب جديد بداخل الدليل 
        /// </summary> 
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var validation = ValidateDto(dto);
            if (validation != null) return validation;
            try
            {
                string accountType = ConvertAccountTypeToDb(dto.Account_Type);
                string accountCategory = ConvertAccountCategoryToDb(dto.Account_Category);
                string normalBalance = ConvertNormalBalanceToDb(dto.Normal_Balance) ?? "Debit";
                string generatedCode = await _accountNumberService.GenerateAccountCodeAsync(dto.Company_ID.Trim(), dto.Parent_Account_ID);
                if (string.IsNullOrWhiteSpace(generatedCode)) return BadRequest("فشل النظام في توليد رقم الحساب.");
                bool exists = await _context.Chart_Of_Accounts.AnyAsync(x => x.Company_ID == dto.Company_ID.Trim() && x.Account_Code == generatedCode);
                if (exists) return BadRequest($"رقم الحساب [{generatedCode}] موجود مسبقاً.");
                var account = new ChartOfAccount
                {
                    Company_ID = dto.Company_ID.Trim(),
                    Parent_Account_ID = string.IsNullOrWhiteSpace(dto.Parent_Account_ID) ? null : dto.Parent_Account_ID.Trim(),
                    Account_Code = generatedCode,
                    Account_Name_AR = dto.Account_Name_AR.Trim(),
                    Account_Name_EN = dto.Account_Name_EN?.Trim(),
                    Account_Type = accountType,
                    Account_Category = accountCategory,
                    Normal_Balance = normalBalance,
                    Account_Level = dto.Account_Level <= 0 ? 1 : dto.Account_Level,
                    Is_Postable = dto.Is_Postable,
                    Currency_Code = string.IsNullOrWhiteSpace(dto.Currency_Code) ? "YER" : dto.Currency_Code.Trim(),
                    Is_Active = dto.Is_Active,
                    Notes = dto.Notes?.Trim(),
                    Allow_ManualEntry = dto.Allow_ManualEntry,
                    System_Account = dto.System_Account,
                    Requires_Party = dto.Requires_Party,
                    Requires_CostCenter = dto.Requires_CostCenter,
                    Requires_Project = dto.Requires_Project,
                    Is_Summary_Account = dto.Is_Summary_Account,
                    Affects_Balance_Sheet = dto.Affects_Balance_Sheet,
                    Affects_Income_Statement = dto.Affects_Income_Statement,
                    Multi_Currency = dto.Multi_Currency,
                    Account_Path = dto.Account_Path?.Trim(),
                    Account_Serial = dto.Account_Serial,
                    Created_By = dto.Created_By?.Trim(),
                    Created_At = DateTime.UtcNow,
                    Updated_At = dto.Updated_At,
                    Updated_By = dto.Updated_By?.Trim()
                };
                await _context.Chart_Of_Accounts.AddAsync(account);
                await _context.SaveChangesAsync();
                return Ok(account);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"خطأ قاعدة البيانات: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ داخلي: {ex.Message}");
            }
        }

        /// <summary> 
        /// تعديل حساب مع قفل أمني على معرف الشركة لمنع التداخل 
        /// </summary> 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(string id, [FromBody] CreateAccountDto dto)
        {
            var validation = ValidateDto(dto);
            if (validation != null) return validation;
            try
            {
                var account = await _context.Chart_Of_Accounts
                    .FirstOrDefaultAsync(x => x.Account_ID == id && x.Company_ID == dto.Company_ID.Trim());
                if (account == null) return NotFound("الحساب غير موجود أو لا تملك صلاحية تعديله.");
                account.Account_Name_AR = dto.Account_Name_AR.Trim();
                account.Account_Name_EN = dto.Account_Name_EN?.Trim();
                account.Account_Type = ConvertAccountTypeToDb(dto.Account_Type);
                account.Account_Category = ConvertAccountCategoryToDb(dto.Account_Category);
                account.Normal_Balance = ConvertNormalBalanceToDb(dto.Normal_Balance) ?? "Debit";
                account.Is_Postable = dto.Is_Postable;
                account.Currency_Code = string.IsNullOrWhiteSpace(dto.Currency_Code) ? "YER" : dto.Currency_Code.Trim();
                account.Is_Active = dto.Is_Active;
                account.Notes = dto.Notes?.Trim();
                account.Allow_ManualEntry = dto.Allow_ManualEntry;
                account.System_Account = dto.System_Account;
                account.Requires_Party = dto.Requires_Party;
                account.Requires_CostCenter = dto.Requires_CostCenter;
                account.Requires_Project = dto.Requires_Project;
                account.Is_Summary_Account = dto.Is_Summary_Account;
                account.Affects_Balance_Sheet = dto.Affects_Balance_Sheet;
                account.Affects_Income_Statement = dto.Affects_Income_Statement;
                account.Multi_Currency = dto.Multi_Currency;
                account.Account_Path = dto.Account_Path?.Trim();
                account.Account_Serial = dto.Account_Serial;
                account.Updated_By = dto.Updated_By?.Trim();
                account.Updated_At = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(account);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"خطأ قاعدة البيانات: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"فشل تعديل الحساب: {ex.Message}");
            }
        }

        /// <summary> 
        /// حذف حساب مع التحقق من تبعيته للشركة وجود حسابات فرعية 
        /// </summary> 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(string id, [FromQuery] string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("معرف الشركة مطلوب للأمان.");
            try
            {
                var account = await _context.Chart_Of_Accounts
                    .FirstOrDefaultAsync(x => x.Account_ID == id && x.Company_ID == companyId.Trim());
                if (account == null) return NotFound("الحساب غير موجود أو لا تملك صلاحية حذفه.");
                bool hasChildren = await _context.Chart_Of_Accounts
                    .AnyAsync(x => x.Parent_Account_ID == id && x.Company_ID == companyId.Trim());
                if (hasChildren) return BadRequest("لا يمكن حذف الحساب لأنه يحتوي على حسابات فرعية.");
                _context.Chart_Of_Accounts.Remove(account);
                await _context.SaveChangesAsync();
                return Ok("تم حذف الحساب بنجاح.");
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"خطأ قاعدة البيانات: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"فشل حذف الحساب: {ex.Message}");
            }
        }

        private IActionResult? ValidateDto(CreateAccountDto dto)
        {
            if (dto == null) return BadRequest("لم تصل بيانات الحساب.");
            if (string.IsNullOrWhiteSpace(dto.Company_ID)) return BadRequest("رقم الشركة مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.Account_Name_AR)) return BadRequest("اسم الحساب بالعربي مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.Account_Type)) return BadRequest("نوع الحساب مطلوب.");
            return null;
        }

        private static string ConvertAccountTypeToDb(string value)
        {
            value = value?.Trim() ?? "";
            return value switch { "أصل" or "اصول" or "أصول" => "Asset", "خصم" or "خصوم" or "التزام" or "التزامات" => "Liability", "حقوق ملكية" => "Equity", "إيراد" or "ايراد" or "إيرادات" or "ايرادات" => "Revenue", "مصروف" or "مصروفات" => "Expense", _ => value };
        }

        private static string? ConvertAccountCategoryToDb(string? value)
        {
            value = value?.Trim();
            if (string.IsNullOrWhiteSpace(value)) return null;
            return value switch { "نقدية" or "صندوق" => "Cash", "بنك" or "بنوك" => "Bank", "عميل" or "عملاء" => "Customer", "مورد" or "موردين" => "Vendor", "إيراد" or "ايراد" or "إيرادات" => "Revenue", "مصروف" or "مصروفات" => "Expense", "أصل" or "أصول" => "Asset", "خصم" or "خصوم" => "Liability", _ => value };
        }

        private static string? ConvertNormalBalanceToDb(string? value)
        {
            value = value?.Trim();
            if (string.IsNullOrWhiteSpace(value)) return null;
            return value switch { "مدين" => "Debit", "دائن" => "Credit", _ => value };
        }


        /// <summary> 
        /// إنشاء حساب جديد بداخل الدليل 
        /// </summary> 
        // دالة هيدر الـ API تستقبل طلب من نوع GET، واسم الرابط هو GetCashParentLookup
        // دالة هيدر الـ API تستقبل طلب من نوع GET، واسم الرابط هو GetCashParentLookup
        [HttpGet("GetCashParentLookup")]
        public async Task<IActionResult> GetCashParentLookup([FromQuery] string companyId)
        {
            // التحقق من المدخلات: إذا كان رقم الشركة فارغاً أو يحتوي على مسافات فقط
            if (string.IsNullOrWhiteSpace(companyId))
                // يعيد النظام خطأ للمستخدم (طلب غير صالح) مع رسالة تنبيه
                return BadRequest("رقم الشركة مطلوب.");

            // الحل السريع والمؤقت: جلب حساب الأب الرئيسي للصناديق مباشرة عن طريق الكود الثابت "111"
            var parent = await _context.Chart_Of_Accounts
                .AsNoTracking() // تحسين الأداء: إيقاف تتبع التعديلات لأننا نريد قراءة البيانات فقط دون تعديلها
                .Where(x =>
                    x.Company_ID == companyId.Trim() && // مطابقة رقم الشركة مع حذف المسافات الزائدة
                    x.Account_Code == "111" &&          // التحديد المباشر بكود الحساب الرئيسي للصناديق (مثال: 111) لضمان عدم التداخل
                    x.Is_Active)                        // التأكد أن حساب الأب نشط وغير موقف في الدليل المحاسبي
                .Select(x => new
                {
                    x.Account_ID,     // معرف الحساب الفريد في قاعدة البيانات لربطه كأب (Parent_Account_ID) للحساب الجديد
                    x.Account_Code,   // كود الحساب الرئيسي (111) لتوليد أرقام الصناديق الفرعية بناءً عليه تلقائياً
                    x.Account_Level,  // مستوى الحساب الحالي في الدليل لزيادة مستوى الصندوق الجديد بمقدار 1

                    // دمج كود الحساب مع اسمه العربي ليظهر بشكل منسق في الواجهة (مثال: "111 - حساب الصناديق الرئيسي")
                    Account_Name_AR = (x.Account_Code ?? "") + " - " + (x.Account_Name_AR ?? "")
                })
                // جلب أول حساب يطابق الشروط المذكورة أعلاه (وهو الحساب رقم 111)
                .FirstOrDefaultAsync();

            // التحقق: إذا لم يجد النظام حساب الأب رقم "111" في قاعدة البيانات لهذه الشركة
            if (parent == null)
                // يعيد النظام قائمة فارغة للشاشة حتى لا يحدث خطأ (Crash) في الواجهات
                return Ok(new List<object>());

            // إذا وجد الحساب، يعيده للشاشة داخل مصفوفة (Array) لكي تستقبله الشاشة بشكل صحيح
            return Ok(new[] { parent });
        }





    }
}