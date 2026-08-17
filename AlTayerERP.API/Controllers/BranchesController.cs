using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة الفروع Branches. لا تقبل هذه الواجهة حقول التدقيق من العميل،
    /// ولا تنفذ حذفاً فعلياً للسجلات المرجعية.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NumberGeneratorService _numberGenerator;
        private readonly ServerSessionService _sessions;

        /// <summary>حقن قاعدة البيانات وخدمات الترقيم والجلسة.</summary>
        public BranchesController(
            AppDbContext context,
            NumberGeneratorService numberGenerator,
            ServerSessionService sessions)
        {
            _context = context;
            _numberGenerator = numberGenerator;
            _sessions = sessions;
        }

        /// <summary>
        /// يحصل على جلسة مدير النظام التي يثبتها Middleware الخادم،
        /// بدلاً من الوثوق بمعرف مستخدم يرسله تطبيق سطح المكتب.
        /// </summary>
        private bool TryGetAdminSession(out ServerSession session)
        {
            session = HttpContext.Items["ServerSession"] as ServerSession
                ?? new ServerSession(
                    string.Empty, 0, 0, false, string.Empty, 0, 0, "unknown",
                    DateTime.MinValue, DateTime.MinValue);
            return session.Is_System_Admin;
        }

        /// <summary>إنشاء فرع تحت شركة نشطة مع تدقيق وإنشاء رقم فرع مركزي عند عدم إدخاله.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();

            var error = await ValidateAsync(dto, null);
            if (error is not null) return BadRequest(error);

            var branch = new TenantBranch
            {
                Company_ID = dto.Company_ID.Trim(),
                Branch_Code = string.IsNullOrWhiteSpace(dto.Branch_Code)
                    ? await _numberGenerator.GenerateNextNumberAsync("BRANCH", dto.Company_ID.Trim())
                    : dto.Branch_Code.Trim().ToUpperInvariant(),
                Created_Date = DateTime.UtcNow,
                Created_By = session.User_ID,
                Edit_Count = 0,
                Is_Active = true
            };

            if (await _context.Tenant_Branches.AnyAsync(x =>
                x.Company_ID == branch.Company_ID && x.Branch_Code == branch.Branch_Code))
                return Conflict("كود الفرع مكرر داخل الشركة.");

            MapBusinessFields(dto, branch);
            branch.Is_Active = true; // الإيقاف لا يتم ضمن الحفظ العام بل بعملية مستقلة ذات سبب.

            await using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Tenant_Branches.Add(branch);
            await _context.SaveChangesAsync();
            await SaveBranchGeographyAsync(branch.Branch_ID, dto.City_ID!.Value);
            AddAudit(session, branch, "CREATE", null, Snapshot(branch), "إنشاء فرع");
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetBranch), new { id = branch.Branch_ID }, branch);
        }

        /// <summary>عرض فروع شركة واحدة مع معلومات التدقيق اللازمة للشاشة الإدارية.</summary>
        [HttpGet]
        public async Task<IActionResult> GetBranches([FromQuery] string companyId)
        {
            if (!TryGetAdminSession(out _)) return Forbid();
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("معرف الشركة مطلوب.");

            var branches = await _context.Tenant_Branches.AsNoTracking()
                .Where(x => x.Company_ID == companyId.Trim())
                .OrderBy(x => x.Branch_Name)
                .Select(x => new
                {
                    x.Branch_ID, x.Company_ID, x.Branch_Code, x.Branch_Name, x.Branch_Name_EN,
                    x.Address, x.Branch_Type, x.Parent_Branch_ID, x.Phone, x.Mobile, x.Email,
                    x.Website, x.Manager_Name, x.Notes, x.Allow_Credit, x.Allow_Percentage,
                    x.Is_Active, x.Currency_ID, x.Created_By, x.Created_Date, x.Updated_By,
                    x.Updated_Date, x.Edit_Count, x.Stopped_By, x.Stopped_At, x.Stopped_Reason,
                    x.Reactivated_By, x.Reactivated_At, x.Reactivate_Reason
                })
                .ToListAsync();

            return Ok(branches);
        }

        /// <summary>عرض فرع واحد بغرض الاستعراض أو التعديل.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBranch(int id)
        {
            if (!TryGetAdminSession(out _)) return Forbid();
            var branch = await _context.Tenant_Branches.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Branch_ID == id);
            return branch is null ? NotFound("الفرع غير موجود.") : Ok(branch);
        }

        /// <summary>قائمة فروع نشطة فقط؛ لا تعرض الفروع الموقوفة لشاشات العمليات.</summary>
        [HttpGet("GetActiveBranchesLookup")]
        public async Task<IActionResult> GetActiveBranchesLookup([FromQuery] string companyId)
        {
            // هذه قائمة دخول عامة محدودة: تعرض فروعاً نشطة فقط للشركة المختارة.
            // لا تمنح صلاحيات إدارية ولا تكشف بيانات تشغيلية.
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("معرف الشركة مطلوب.");

            var data = await _context.Tenant_Branches.AsNoTracking()
                .Where(x => x.Company_ID == companyId.Trim() && x.Is_Active)
                .OrderBy(x => x.Branch_Name)
                .Select(x => new { x.Branch_ID, x.Branch_Code, x.Branch_Name })
                .ToListAsync();
            return Ok(data);
        }

        /// <summary>قائمة شركات نشطة لاختيار الشركة الأم للفرع.</summary>
        [HttpGet("GetCompaniesLookup")]
        public async Task<IActionResult> GetCompaniesLookup()
        {
            // شاشة الدخول تحتاج هذه القائمة قبل أن توجد جلسة؛ تعاد أسماء الشركات النشطة فقط.
            return Ok(await _context.Companies.AsNoTracking()
                .Where(x => x.Is_Active)
                .OrderBy(x => x.Company_Name_AR)
                .Select(x => new { x.Company_ID, x.Company_Name_AR })
                .ToListAsync());
        }

        /// <summary>
        /// تعديل بيانات عمل الفرع فقط. لا يسمح بتغيير الشركة الأم أو كود الفرع أو الحالة
        /// من خلال PUT العام؛ هذه عمليات مستقلة ومراجعة.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBranch(int id, [FromBody] CreateBranchDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();

            var branch = await _context.Tenant_Branches.FirstOrDefaultAsync(x => x.Branch_ID == id);
            if (branch is null) return NotFound("الفرع غير موجود.");
            if (!branch.Is_Active) return Conflict("لا يمكن تعديل فرع موقوف؛ أعد تفعيله أولاً.");

            var error = await ValidateAsync(dto, id);
            if (error is not null) return BadRequest(error);
            if (!string.Equals(branch.Company_ID, dto.Company_ID?.Trim(), StringComparison.Ordinal))
                return Conflict("لا يسمح بتغيير الشركة الأم للفرع بعد إنشائه.");

            var oldValues = Snapshot(branch);
            MapBusinessFields(dto, branch);
            branch.Is_Active = true;
            branch.Updated_Date = DateTime.UtcNow;
            branch.Updated_By = session.User_ID;
            branch.Edit_Count += 1;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            await SaveBranchGeographyAsync(branch.Branch_ID, dto.City_ID!.Value);
            AddAudit(session, branch, "UPDATE", oldValues, Snapshot(branch), "تعديل بيانات فرع");
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(branch);
        }

        /// <summary>
        /// إيقاف الفرع بدلاً من الحذف. السبب إلزامي ويمنع الإيقاف عندما توجد
        /// فروع أبناء أو مستخدمون نشطون متصلون بالفرع.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeactivateBranch(int id, [FromBody] RecordStatusChangeDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("سبب إيقاف الفرع مطلوب.");

            var branch = await _context.Tenant_Branches.FirstOrDefaultAsync(x => x.Branch_ID == id);
            if (branch is null) return NotFound("الفرع غير موجود.");
            if (!branch.Is_Active) return Conflict("الفرع موقوف مسبقاً.");

            if (await _context.Tenant_Branches.AnyAsync(x => x.Parent_Branch_ID == id && x.Is_Active))
                return Conflict("لا يمكن إيقاف الفرع قبل إيقاف الفروع التابعة النشطة.");

            if (await _context.Users.AnyAsync(x => x.Branch_ID == id && x.Is_Active))
                return Conflict("لا يمكن إيقاف الفرع قبل نقل المستخدمين النشطين أو إيقافهم.");

            var oldValues = Snapshot(branch);
            branch.Is_Active = false;
            branch.Updated_Date = DateTime.UtcNow;
            branch.Updated_By = session.User_ID;
            branch.Stopped_By = session.User_ID;
            branch.Stopped_At = DateTime.UtcNow;
            branch.Stopped_Reason = dto.Reason.Trim();
            branch.Edit_Count += 1;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            AddAudit(session, branch, "DEACTIVATE", oldValues, Snapshot(branch), branch.Stopped_Reason);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { message = "تم إيقاف الفرع دون حذف تاريخه." });
        }

        /// <summary>إعادة تفعيل فرع بعد التحقق من أن الشركة الأم لا تزال نشطة.</summary>
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateBranch(int id, [FromBody] RecordStatusChangeDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("سبب إعادة تفعيل الفرع مطلوب.");

            var branch = await _context.Tenant_Branches.FirstOrDefaultAsync(x => x.Branch_ID == id);
            if (branch is null) return NotFound("الفرع غير موجود.");
            if (!await _context.Companies.AnyAsync(x => x.Company_ID == branch.Company_ID && x.Is_Active))
                return Conflict("لا يمكن إعادة تفعيل الفرع قبل تفعيل الشركة الأم.");

            var oldValues = Snapshot(branch);
            branch.Is_Active = true;
            branch.Updated_Date = DateTime.UtcNow;
            branch.Updated_By = session.User_ID;
            branch.Reactivated_By = session.User_ID;
            branch.Reactivated_At = DateTime.UtcNow;
            branch.Reactivate_Reason = dto.Reason.Trim();
            branch.Edit_Count += 1;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            AddAudit(session, branch, "REACTIVATE", oldValues, Snapshot(branch), branch.Reactivate_Reason);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(branch);
        }

        /// <summary>إرجاع سياق الجلسة الحالي مع منع الاستعلام عن نطاق آخر عبر Query String.</summary>
        [HttpGet("GetSessionInfo")]
        public async Task<IActionResult> GetSessionInfo([FromQuery] string companyId, [FromQuery] int branchId, [FromQuery] int yearId)
        {
            // يعتمد على ServerSession التي تحقق منها JWT Authentication Handler، ولا يفسر
            // أي معرف مستخدم أو نطاق عمل مرسل من التطبيق.
            if (HttpContext.Items["ServerSession"] is not ServerSession session)
                return Unauthorized("انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد.");

            if (!string.Equals(session.Company_ID, companyId?.Trim(), StringComparison.Ordinal) ||
                session.Branch_ID != branchId || session.Year_ID != yearId)
                return Forbid();

            var data = await (
                from company in _context.Companies.AsNoTracking()
                join branch in _context.Tenant_Branches.AsNoTracking() on company.Company_ID equals branch.Company_ID
                join year in _context.Fiscal_Years.AsNoTracking() on company.Company_ID equals year.Company_ID
                where company.Company_ID == session.Company_ID &&
                    branch.Branch_ID == session.Branch_ID &&
                    year.Fiscal_Year_ID == session.Year_ID
                select new { company.Company_ID, company.Company_Name_AR, branch.Branch_ID, branch.Branch_Name, year.Year_Name }
            ).FirstOrDefaultAsync();

            return data is null ? NotFound("تعذر العثور على سياق الجلسة.") : Ok(data);
        }

        /// <summary>التحقق من الشركة، الأب، العملة، وتفرد اسم الفرع في نطاق الشركة.</summary>
        private async Task<string?> ValidateAsync(CreateBranchDto dto, int? branchId)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Company_ID) || string.IsNullOrWhiteSpace(dto.Branch_Name))
                return "الشركة واسم الفرع بالعربية حقول مطلوبة.";

            var companyId = dto.Company_ID.Trim();
            if (!await _context.Companies.AnyAsync(x => x.Company_ID == companyId && x.Is_Active))
                return "الشركة الأم غير موجودة أو موقوفة.";

            if (dto.Currency_ID <= 0) return "العملة الافتراضية للفرع غير صالحة.";

            if (!dto.City_ID.HasValue || dto.City_ID.Value <= 0)
                return "المدينة مطلوبة للفرع.";

            if (await LoadCityGeographyAsync(dto.City_ID.Value) is null)
                return "المدينة غير موجودة أو موقوفة.";

            if (dto.Parent_Branch_ID.HasValue)
            {
                if (dto.Parent_Branch_ID == branchId) return "لا يجوز جعل الفرع أباً لنفسه.";
                var parentIsValid = await _context.Tenant_Branches.AnyAsync(x =>
                    x.Branch_ID == dto.Parent_Branch_ID &&
                    x.Company_ID == companyId &&
                    x.Is_Active &&
                    (x.Branch_Type == "فرع رئيسي" || x.Branch_Type == "فرع" ||
                     x.Branch_Type == "MAIN" || x.Branch_Type == "BRANCH"));
                if (!parentIsValid) return "الفرع الأب يجب أن يكون نشطاً ومن نوع فرع رئيسي أو فرع ومن الشركة نفسها.";

                if (branchId.HasValue && await WouldCreateHierarchyCycleAsync(branchId.Value, dto.Parent_Branch_ID.Value))
                    return "لا يمكن حفظ الفرع الأب لأنه يؤدي إلى حلقة في التسلسل الهرمي للفروع.";
            }

            var name = dto.Branch_Name.Trim();
            var duplicate = await _context.Tenant_Branches.AnyAsync(x =>
                x.Company_ID == companyId && x.Branch_Name == name &&
                (!branchId.HasValue || x.Branch_ID != branchId.Value));
            return duplicate ? "اسم الفرع مكرر داخل الشركة." : null;
        }

        /// <summary>يحفظ مدينة الفرع ومعها الدولة والمحافظة التابعة لها من جدول المدن فقط.</summary>
        private async Task SaveBranchGeographyAsync(int branchId, int cityId)
        {
            var city = await LoadCityGeographyAsync(cityId)
                ?? throw new InvalidOperationException("المدينة غير موجودة أو موقوفة.");

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Tenant_Branches
                SET Country_ID = {city.Country_ID}, Governorate_ID = {city.Governorate_ID}, City_ID = {city.City_ID}
                WHERE Branch_ID = {branchId}");
        }

        private Task<BranchCityGeography?> LoadCityGeographyAsync(int cityId) =>
            _context.Database.SqlQueryRaw<BranchCityGeography>(
                "SELECT City_ID, Country_ID, Governorate_ID FROM cities WHERE City_ID = {0} AND Is_Active = 1", cityId)
                .SingleOrDefaultAsync();

        /// <summary>يتأكد من أن الأب المختار ليس أحد فروع الابن، منعاً للدورات الهرمية.</summary>
        private async Task<bool> WouldCreateHierarchyCycleAsync(int branchId, int proposedParentId)
        {
            var currentId = proposedParentId;
            var visited = new HashSet<int>();
            while (currentId > 0 && visited.Add(currentId))
            {
                if (currentId == branchId) return true;
                currentId = await _context.Tenant_Branches.AsNoTracking()
                    .Where(x => x.Branch_ID == currentId)
                    .Select(x => x.Parent_Branch_ID ?? 0)
                    .SingleOrDefaultAsync();
            }
            return currentId > 0;
        }

        /// <summary>نسخ بيانات العمل المسموح بها فقط، ولا تنقل حقول التدقيق أو الإيقاف من DTO.</summary>
        private static void MapBusinessFields(CreateBranchDto dto, TenantBranch branch)
        {
            branch.Branch_Name = dto.Branch_Name.Trim();
            branch.Branch_Name_EN = dto.Branch_Name_EN?.Trim();
            branch.Address = dto.Address?.Trim();
            branch.Branch_Type = string.IsNullOrWhiteSpace(dto.Branch_Type) ? "فرعي" : dto.Branch_Type.Trim();
            branch.Parent_Branch_ID = dto.Parent_Branch_ID;
            branch.Phone = dto.Phone?.Trim();
            branch.Mobile = dto.Mobile?.Trim();
            branch.Email = dto.Email?.Trim();
            branch.Website = dto.Website?.Trim();
            branch.Manager_Name = dto.Manager_Name?.Trim();
            branch.Notes = dto.Notes?.Trim();
            branch.Allow_Credit = dto.Allow_Credit;
            branch.Allow_Percentage = dto.Allow_Percentage;
            branch.Currency_ID = dto.Currency_ID;
        }

        /// <summary>إضافة سجل Audit_Logs داخل المعاملة نفسها وبالمستخدم والنطاق الموثوقين من الخادم.</summary>
        private void AddAudit(ServerSession session, TenantBranch branch, string action, string? oldValues, string newValues, string? notes)
        {
            _context.Audit_Logs.Add(new AlTayerERP.Core.Entities.Accounting.AuditLog
            {
                Table_Name = "tenant_branches",
                Record_ID = branch.Branch_ID.ToString(),
                Action_Type = action,
                User_ID = session.User_ID.ToString(),
                Branch_ID = session.Branch_ID,
                Action_At = DateTime.UtcNow,
                Old_Values = oldValues,
                New_Values = newValues,
                Action_Channel = "DESKTOP",
                Device_Name = Request.Headers["X-Device-ID"].ToString(),
                IP_Address = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Notes = notes
            });
        }

        /// <summary>لقطة حقول الأعمال والتتبع اللازمة لتقرير التدقيق، من دون بيانات سرية.</summary>
        private static string Snapshot(TenantBranch branch) => JsonSerializer.Serialize(new
        {
            branch.Branch_ID, branch.Company_ID, branch.Branch_Code, branch.Branch_Name,
            branch.Branch_Name_EN, branch.Branch_Type, branch.Parent_Branch_ID, branch.Is_Active,
            branch.Currency_ID, branch.Allow_Credit, branch.Allow_Percentage, branch.Edit_Count,
            branch.Stopped_Reason, branch.Reactivate_Reason
        });

        private sealed class BranchCityGeography
        {
            public int City_ID { get; set; }
            public int Country_ID { get; set; }
            public int Governorate_ID { get; set; }
        }
    }
}
