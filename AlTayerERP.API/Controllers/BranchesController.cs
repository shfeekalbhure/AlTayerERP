using AlTayerERP.API.DTOs;          // استدعاء حزمة نماذج نقل البيانات (Data Transfer Objects)
using AlTayerERP.API.Services;      // استدعاء الخدمات الداخلية للنظام (مثل مولد الأرقام)
using AlTayerERP.Core.Entities;     // استدعاء الكائنات الهيكلية الممثلة لجداول قاعدة البيانات
using AlTayerERP.Infrastructure.Data; // استدعاء سياق الاتصال بقاعدة البيانات (DbContext)
using Microsoft.AspNetCore.Mvc;     // استدعاء مكتبة الـ Web API لبناء الكنترولر والردود
using Microsoft.EntityFrameworkCore; // استدعاء مكتبة الكيانات للتعامل مع قواعد البيانات بشكل كائناتي

namespace AlTayerERP.API.Controllers
{
    // تحديد مسار الـ API ليكون تلقائياً باسم الكنترولر (api/Branches)
    [Route("api/[controller]")]
    // تفعيل ميزات الـ API الذكية مثل التحقق التلقائي من البيانات المرسلة ومصادرها
    [ApiController]
    public class BranchesController : ControllerBase
    {
        // تعريف متغيرات خاصة غير قابلة للتعديل لحفظ سياق قاعدة البيانات والخدمات
        private readonly AppDbContext _context;
        private readonly NumberGeneratorService _numberGenerator;
        private readonly ServerSessionService _sessions;

        // مشيد الكنترولر (Constructor): يتم فيه حقن الاعتماديات (Dependency Injection) لقاعدة البيانات والخدمة
        public BranchesController(AppDbContext context, NumberGeneratorService numberGenerator, ServerSessionService sessions)
        {
            _context = context;
            _numberGenerator = numberGenerator;
            _sessions = sessions;
        }

        /// <summary>
        /// دالة إنشاء فرع جديد في النظام
        /// </summary>
        [HttpPost] // تحديد نوع الطلب كـ POST لإضافة بيانات جديدة
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchDto dto)
        {
            // التحقق من أن البيانات المرسلة ليست فارغة، وأن الحقول الإلزامية (اسم الفرع، معرف الشركة) تحتوي على قيم
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Branch_Name) ||
                string.IsNullOrWhiteSpace(dto.Company_ID))
            {
                // إرجاع خطأ 400 (Bad Request) إذا كانت البيانات الأساسية ناقصة
                return BadRequest("بيانات الفرع الأساسية غير مكتملة");
            }

            var locationError = await ValidateLocationAsync(dto.Country_ID, dto.Governorate_ID, dto.City_ID);
            if (locationError is not null) return BadRequest(locationError);

            try
            {
                // إنشاء كائن جديد من نوع الفرع (TenantBranch) لنقله إلى قاعدة البيانات
                var newBranch = new TenantBranch
                {
                    // ربط الفرع بالشركة التابع لها
                    Company_ID = dto.Company_ID,

                    // إذا كان كود الفرع فارغاً، يتم توليده تلقائياً عبر الخدمة المخصصة، وإلا يتم تنظيف الفراغات المحيطة بالكود المدخل
                    Branch_Code = string.IsNullOrWhiteSpace(dto.Branch_Code)
                        ? await _numberGenerator.GenerateNextNumberAsync("BRANCH", dto.Company_ID)
                        : dto.Branch_Code.Trim(),

                    // إسناد وتنظيف النصوص (إزالة الفراغات الزائدة من البداية والنهاية عبر Trim)
                    Branch_Name = dto.Branch_Name.Trim(),
                    Branch_Name_EN = dto.Branch_Name_EN?.Trim(),
                    Address = dto.Address?.Trim(),
                    Country_ID = dto.Country_ID,
                    Governorate_ID = dto.Governorate_ID,
                    City_ID = dto.City_ID,
                    Postal_Code = dto.Postal_Code?.Trim(),

                    // إذا لم يتم إرسال نوع الفرع، يتم اعتباره "فرعي" بشكل افتراضي
                    Branch_Type = dto.Branch_Type?.Trim() ?? "فرعي",
                    Parent_Branch_ID = dto.Parent_Branch_ID,

                    // إسناد بيانات الاتصال والمعلومات الإضافية مع حمايتها من الفراغات
                    Phone = dto.Phone?.Trim(),
                    Mobile = dto.Mobile?.Trim(),
                    Email = dto.Email?.Trim(),
                    Website = dto.Website?.Trim(),
                    Manager_Name = dto.Manager_Name?.Trim(),
                    Notes = dto.Notes?.Trim(),

                    // إسناد الإعدادات والصلاحيات المالية والنشاط
                    Allow_Credit = dto.Allow_Credit,
                    Allow_Percentage = dto.Allow_Percentage,
                    Is_Active = dto.Is_Active,

                    // إذا كان معرف العملة المرسل أقل من أو يساوي صفر، يتم تعيين العملة الافتراضية رقم 1
                    Currency_ID = dto.Currency_ID <= 0 ? 1 : dto.Currency_ID,

                    // تسجيل تاريخ ووقت إنشاء الفرع الحالي، وتصفير تاريخ التحديث
                    Created_Date = DateTime.Now,
                    Updated_Date = null
                };

                // إضافة كائن الفرع الجديد إلى ذاكرة الـ Entity Framework بشكل غير متزامن
                await _context.Tenant_Branches.AddAsync(newBranch);
                // حفظ التغييرات والفرع الجديد فعلياً داخل قاعدة البيانات (SQL Server مثلاً)
                await _context.SaveChangesAsync();

                // إرجاع كود النجاح 201 (Created) مع البيانات المحفوظة، وهو الأصح هندسياً لعمليات الـ POST
                return CreatedAtAction(nameof(GetBranches), new { companyId = newBranch.Company_ID }, newBranch);
            }
            catch (Exception ex)
            {
                // في حال حدوث أي خطأ غير متوقع، يتم إرجاع كود الخطأ 500 (Internal Server Error) مع تفاصيل الخطأ
                return StatusCode(500, ex.ToString());
            }
        }

        /// <summary>
        /// دالة جلب قائمة الفروع التابعة لشركة محددة (تعرض النشط والموقف والمجمد للمراجعة وإعادة التنشيط)
        /// </summary>
        [HttpGet] // تحديد نوع الطلب كـ GET لقراءة البيانات
        public async Task<IActionResult> GetBranches([FromQuery] string companyId)
        {
            try
            {
                // [التعديل المعتمد]: إزالة شرط Is_Active لعرض كافة الفروع لتسهيل التعديل والتحكم من الشاشة الإدارية
                var branches = await _context.Tenant_Branches
                    .Where(x => x.Company_ID == companyId)
                    .OrderBy(x => x.Branch_Name)
                    .Select(x => new
                    {
                        x.Branch_ID,
                        x.Company_ID,
                        x.Branch_Code,
                        x.Branch_Name,
                        x.Branch_Name_EN,
                        x.Address,
                        x.Country_ID,
                        x.Governorate_ID,
                        x.City_ID,
                        x.Postal_Code,
                        x.Branch_Type,
                        x.Parent_Branch_ID,
                        x.Phone,
                        x.Mobile,
                        x.Email,
                        x.Website,
                        x.Manager_Name,
                        x.Notes,
                        x.Allow_Credit,
                        x.Allow_Percentage,
                        x.Is_Active,
                        x.Currency_ID
                    })
                    .ToListAsync(); // تحويل النتيجة النهائية إلى قائمة بشكل غير متزامن

                // إرجاع قائمة الفروع مع كود النجاح 200 (Ok)
                return Ok(branches);
            }
            catch (Exception ex)
            {
                // إرجاع كود الخطأ 500 في حال فشل الاتصال بقاعدة البيانات أو حدوث خطأ بالسيرفر
                return StatusCode(500, ex.ToString());
            }
        }

        /// <summary>
        /// دالة الـ Lookup لجلب الفروع النشطة فقط لتغذية شاشات العمل اليومي والقوائم المنسدلة المصفاة
        /// </summary>
        [HttpGet("GetActiveBranchesLookup")]
        public async Task<IActionResult> GetActiveBranchesLookup([FromQuery] string companyId)
        {
            var data = await _context.Tenant_Branches
                .Where(x => x.Company_ID == companyId && x.Is_Active)
                .OrderBy(x => x.Branch_Name)
                .Select(x => new { x.Branch_ID, x.Branch_Name })
                .ToListAsync();

            return Ok(data);
        }

        /// <summary>
        /// دالة جلب قائمة مبسطة بالشركات النشطة فقط لملء القوائم المنسدلة (Dropdown/Lookup)
        /// </summary>
        [HttpGet("GetCompaniesLookup")] // تحديد مسار فرعي خاص بالدالة لتصبح: api/Branches/GetCompaniesLookup
        public async Task<IActionResult> GetCompaniesLookup()
        {
            try
            {
                // جلب الشركات من قاعدة البيانات
                var companies = await _context.Companies
                    .Where(c => c.Is_Active) // تصفية البيانات لجلب الشركات النشطة (Active) فقط
                    .Select(c => new
                    {
                        c.Company_ID,      // جلب المعرف الخاص بالشركة
                        c.Company_Name_AR  // جلب الاسم العربي للشركة فقط وتقليل بقية التفاصيل غير المهمة للـ Lookup
                    })
                    .ToListAsync(); // تحويل النتيجة إلى قائمة بشكل غير متزامن

                // إرجاع قائمة الشركات المنسدلة مع كود النجاح 200 (Ok)
                return Ok(companies);
            }
            catch (Exception ex)
            {
                // إرجاع كود الخطأ 500 في حال حدوث مشكلة بالسيرفر أثناء جلب الشركات
                return StatusCode(500, ex.ToString());
            }
        }

        /// <summary>
        /// دالة تعديل بيانات فرع حالي أو اعتماده في النظام
        /// PUT: api/Branches/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBranch(int id, [FromBody] CreateBranchDto dto)
        {
            // 1. التحقق من صحة واكتمال كائن البيانات القادم من الشاشة
            if (dto == null || string.IsNullOrWhiteSpace(dto.Branch_Name))
            {
                return BadRequest("البيانات المرسلة للتعديل غير مكتملة أو خاطئة.");
            }

            var locationError = await ValidateLocationAsync(dto.Country_ID, dto.Governorate_ID, dto.City_ID);
            if (locationError is not null) return BadRequest(locationError);

            try
            {
                // 2. البحث عن الفرع المستهدف بالتعديل داخل جدول الفروع بقاعدة البيانات
                var existingBranch = await _context.Tenant_Branches.FirstOrDefaultAsync(b => b.Branch_ID == id);

                if (existingBranch == null)
                {
                    return NotFound($"الفرع المطلوب ذو الرقم {id} غير موجود في قاعدة البيانات.");
                }

                // 3. تحديث وإسناد الحقول بالقيم الجديدة القادمة من الواجهة مع تنظيف النصوص عبر Trim
                existingBranch.Company_ID = dto.Company_ID;
                existingBranch.Branch_Name = dto.Branch_Name.Trim();
                existingBranch.Branch_Name_EN = dto.Branch_Name_EN?.Trim();
                existingBranch.Address = dto.Address?.Trim();
                existingBranch.Country_ID = dto.Country_ID;
                existingBranch.Governorate_ID = dto.Governorate_ID;
                existingBranch.City_ID = dto.City_ID;
                existingBranch.Postal_Code = dto.Postal_Code?.Trim();
                existingBranch.Branch_Type = dto.Branch_Type?.Trim() ?? "فرعي";
                existingBranch.Parent_Branch_ID = dto.Parent_Branch_ID; // تحديث معرف الفرع الأب شجرياً

                existingBranch.Phone = dto.Phone?.Trim();
                existingBranch.Mobile = dto.Mobile?.Trim();
                existingBranch.Email = dto.Email?.Trim();
                existingBranch.Website = dto.Website?.Trim();
                existingBranch.Manager_Name = dto.Manager_Name?.Trim();
                existingBranch.Notes = dto.Notes?.Trim();

                existingBranch.Allow_Credit = dto.Allow_Credit;
                existingBranch.Allow_Percentage = dto.Allow_Percentage;

                // تحديث حالة النشاط (نشط/موقوف) وهي المسؤولة أيضاً عن تلبية طلبات أزرار الاعتماد وإلغاء الاعتماد
                existingBranch.Is_Active = dto.Is_Active;
                existingBranch.Currency_ID = dto.Currency_ID <= 0 ? 1 : dto.Currency_ID;

                // تسجيل وتحديث طابع وقت التعديل الحالي على السيرفر
                existingBranch.Updated_Date = DateTime.Now;

                // 4. حفظ كافة التغييرات المباشرة بداخل قاعدة البيانات
                await _context.SaveChangesAsync();

                // إرجاع كود النجاح 200 مع كائن الفرع بعد التحديث
                return Ok(existingBranch);
            }
            catch (Exception ex)
            {
                // إرجاع كود الخطأ 500 في حال حدوث مشكلة تقنية بالسيرفر
                return StatusCode(500, $"حدث خطأ داخلي في الخادم أثناء التعديل: {ex.Message}");
            }
        }

        /// <summary>
        /// دالة حذف فرع من النظام 
        /// DELETE: api/Branches/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var branch = await _context.Tenant_Branches
                .FirstOrDefaultAsync(x => x.Branch_ID == id);

            if (branch == null)
                return NotFound("الفرع غير موجود.");

            // التحقق من وجود عمليات مرتبطة بالفرع
            bool hasTransactions =
                await _context.Users.AnyAsync(x => x.Branch_ID == id)
                // || await _context.Shipments.AnyAsync(x => x.Branch_ID == id)
                // || await _context.Tickets.AnyAsync(x => x.Branch_ID == id)
                // || await _context.AccountingEntries.AnyAsync(x => x.Branch_ID == id)
                ;

            if (hasTransactions)
            {
                return BadRequest("لا يمكن حذف الفرع لأنه مرتبط بعمليات داخل النظام.");
            }

            _context.Tenant_Branches.Remove(branch);
            await _context.SaveChangesAsync();

            return Ok("تم حذف الفرع بنجاح.");
        }


        /// <summary>
        /// جلب اسم الشركة واسم الفرع واسم السنة للجلسة الحالية
        /// GET: api/Branches/GetSessionInfo
        /// </summary>
        [HttpGet("GetSessionInfo")]
        public async Task<IActionResult> GetSessionInfo([FromQuery] string companyId, [FromQuery] int branchId, [FromQuery] int yearId)
        {
            // يمنع قراءة سياق شركة/فرع/سنة أخرى بمجرد تغيير قيم الاستعلام.
            if (!_sessions.TryGet(Request.Headers["X-Session-Token"].ToString(), out var session))
                return Unauthorized("انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد.");

            if (!string.Equals(session.Company_ID, companyId?.Trim(), StringComparison.Ordinal) ||
                session.Branch_ID != branchId || session.Year_ID != yearId)
            {
                return Forbid();
            }

            var data = await (
                from company in _context.Companies.AsNoTracking()
                join branch in _context.Tenant_Branches.AsNoTracking()
                    on company.Company_ID equals branch.Company_ID
                join year in _context.Fiscal_Years.AsNoTracking()
                    on company.Company_ID equals year.Company_ID
                where company.Company_ID == session.Company_ID &&
                      branch.Branch_ID == session.Branch_ID &&
                      year.Fiscal_Year_ID == session.Year_ID
                select new
                {
                    Company_ID = company.Company_ID,
                    company.Company_Name_AR,
                    Branch_ID = branch.Branch_ID,
                    branch.Branch_Name,
                    year.Year_Name
                }).FirstOrDefaultAsync();

            if (data == null)
                return NotFound("تعذر العثور على سياق الجلسة.");

            return Ok(data);
        }

        private async Task<string?> ValidateLocationAsync(long? countryId, long? governorateId, long? cityId)
        {
            if (!countryId.HasValue)
                return governorateId.HasValue || cityId.HasValue ? "يجب اختيار الدولة قبل المحافظة أو المدينة." : null;

            if (!await _context.Countries.AnyAsync(x => x.Country_ID == countryId && x.Is_Active))
                return "الدولة المختارة غير موجودة أو غير نشطة.";

            if (governorateId.HasValue)
            {
                var governorate = await _context.Governorates.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Governorate_ID == governorateId && x.Is_Active);
                if (governorate is null || governorate.Country_ID != countryId)
                    return "المحافظة المختارة لا تتبع الدولة المحددة أو غير نشطة.";
            }
            else if (cityId.HasValue) return "يجب اختيار المحافظة قبل المدينة.";

            if (cityId.HasValue)
            {
                var city = await _context.Cities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.City_ID == cityId && x.Is_Active);
                if (city is null || city.Governorate_ID != governorateId)
                    return "المدينة المختارة لا تتبع المحافظة المحددة أو غير نشطة.";
            }
            return null;
        }
    }
}