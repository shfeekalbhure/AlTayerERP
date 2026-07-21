using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة الإعدادات العامة والمالية بالنطاق الحالي للجلسة.
    /// لا تقبل الشركة أو الفرع أو السنة من العميل؛ يفرضها الخادم حسب نطاق الإعداد.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class SystemSettingsController : ControllerBase
    {
        private static readonly HashSet<string> ValidScopes = new(StringComparer.OrdinalIgnoreCase)
        {
            "SYSTEM", "COMPANY", "BRANCH", "FISCAL_YEAR"
        };

        private readonly AppDbContext _context;

        public SystemSettingsController(AppDbContext context) => _context = context;

        private ServerSession? GetSession() =>
            HttpContext.Items["ServerSession"] as ServerSession;

        private IActionResult? RequireSystemAdmin(out ServerSession? session)
        {
            session = GetSession();
            if (session == null)
                return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد." });

            if (!session.Is_System_Admin)
                return Forbid();

            return null;
        }

        /// <summary>
        /// يعرض إعدادات النظام والسياق الحالي فقط، مرتبة من العام إلى الأكثر تخصيصاً.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            var settings = await _context.System_Settings
                .AsNoTracking()
                .Where(x =>
                    x.Scope == "SYSTEM" ||
                    (x.Company_ID == session.Company_ID &&
                     ((x.Scope == "COMPANY" && x.Branch_ID == 0 && x.Fiscal_Year_ID == 0) ||
                      (x.Scope == "BRANCH" && x.Branch_ID == session.Branch_ID && x.Fiscal_Year_ID == 0) ||
                      (x.Scope == "FISCAL_YEAR" && x.Branch_ID == session.Branch_ID &&
                       x.Fiscal_Year_ID == session.Year_ID))))
                .OrderBy(x => x.Setting_Key)
                .ThenBy(x => x.Scope)
                .ToListAsync();

            return Ok(settings);
        }

        /// <summary>
        /// يضيف أو يعدل إعداداً. يسمح فقط بالنطاقات المعتمدة ولا يسمح بتجاوز سياق الجلسة.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveSystemSettingRequest request)
        {
            var accessError = RequireSystemAdmin(out var session);
            if (accessError != null || session == null)
                return accessError!;

            if (request == null ||
                string.IsNullOrWhiteSpace(request.Setting_Key) ||
                string.IsNullOrWhiteSpace(request.Setting_Name))
            {
                return BadRequest(new { message = "مفتاح الإعداد واسمه مطلوبان." });
            }

            // تقبل الواجهة قيمة عرض عربية مثل "SYSTEM | عام للنظام"،
            // لكن التخزين يتم دائماً بالكود الثابت الإنجليزي.
            var scope = NormalizeScope(request.Scope);
            if (!ValidScopes.Contains(scope))
            {
                return BadRequest(new
                {
                    message = "النطاق غير صالح. الخيارات هي: SYSTEM أو COMPANY أو BRANCH أو FISCAL_YEAR."
                });
            }

            var key = request.Setting_Key.Trim();
            var name = request.Setting_Name.Trim();
            if (key.Length > 100 || name.Length > 200)
                return BadRequest(new { message = "مفتاح الإعداد أو اسمه أطول من الحد المسموح به." });

            var (companyId, branchId, yearId) = ResolveScope(session, scope);

            var duplicate = await _context.System_Settings.AnyAsync(x =>
                x.Setting_Key == key &&
                x.Scope == scope &&
                x.Company_ID == companyId &&
                x.Branch_ID == branchId &&
                x.Fiscal_Year_ID == yearId &&
                x.Setting_ID != request.Setting_ID);
            if (duplicate)
                return BadRequest(new { message = "يوجد إعداد بالمفتاح نفسه داخل هذا النطاق." });

            SystemSetting setting;
            if (request.Setting_ID > 0)
            {
                var existingSetting = await _context.System_Settings
                    .FirstOrDefaultAsync(x => x.Setting_ID == request.Setting_ID);
                if (existingSetting == null)
                    return NotFound(new { message = "الإعداد غير موجود." });

                // لا يسمح بتعديل إعداد من سياق شركة/فرع/سنة أخرى.
                if (!IsVisibleInSession(existingSetting, session))
                    return NotFound(new { message = "الإعداد غير موجود ضمن نطاق الجلسة الحالية." });

                setting = existingSetting;
            }
            else
            {
                setting = new SystemSetting { Created_At = DateTime.Now };
                _context.System_Settings.Add(setting);
            }

            setting.Setting_Key = key;
            setting.Setting_Name = name;
            setting.Setting_Value = request.Setting_Value?.Trim() ?? string.Empty;
            setting.Scope = scope;
            setting.Company_ID = companyId;
            setting.Branch_ID = branchId;
            setting.Fiscal_Year_ID = yearId;
            setting.Effective_Date = request.Effective_Date?.Date;
            setting.Description = request.Description?.Trim() ?? string.Empty;
            setting.Is_Active = request.Is_Active;
            setting.Updated_At = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = request.Setting_ID > 0 ? "تم تعديل الإعداد." : "تمت إضافة الإعداد.",
                setting.Setting_ID
            });
        }

        private static string NormalizeScope(string? scope)
        {
            var value = (scope ?? string.Empty).Trim().ToUpperInvariant();
            var separator = value.IndexOf('|');
            return separator >= 0 ? value[..separator].Trim() : value;
        }

        private static (string CompanyId, int BranchId, int YearId) ResolveScope(ServerSession session, string scope) =>
            scope switch
            {
                "SYSTEM" => (string.Empty, 0, 0),
                "COMPANY" => (session.Company_ID, 0, 0),
                "BRANCH" => (session.Company_ID, session.Branch_ID, 0),
                "FISCAL_YEAR" => (session.Company_ID, session.Branch_ID, session.Year_ID),
                _ => throw new InvalidOperationException("نطاق غير معتمد.")
            };

        private static bool IsVisibleInSession(SystemSetting setting, ServerSession session) =>
            setting.Scope == "SYSTEM" ||
            (setting.Company_ID == session.Company_ID &&
             ((setting.Scope == "COMPANY" && setting.Branch_ID == 0 && setting.Fiscal_Year_ID == 0) ||
              (setting.Scope == "BRANCH" && setting.Branch_ID == session.Branch_ID && setting.Fiscal_Year_ID == 0) ||
              (setting.Scope == "FISCAL_YEAR" && setting.Branch_ID == session.Branch_ID &&
               setting.Fiscal_Year_ID == session.Year_ID)));
    }

    /// <summary>
    /// عقد الحفظ لا يحتوي على مفاتيح الشركة أو الفرع أو السنة؛ الخادم يحددها من الجلسة.
    /// </summary>
    public sealed class SaveSystemSettingRequest
    {
        public int Setting_ID { get; set; }
        public string Setting_Key { get; set; } = string.Empty;
        public string Setting_Name { get; set; } = string.Empty;
        public string Setting_Value { get; set; } = string.Empty;
        public string Scope { get; set; } = "SYSTEM";
        public DateTime? Effective_Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool Is_Active { get; set; } = true;
    }
}
