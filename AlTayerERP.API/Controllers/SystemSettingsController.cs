using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إعدادات النظام متعددة المستويات (SystemSettings).
    /// الأولوية عند القراءة: FISCAL_YEAR ثم BRANCH ثم COMPANY ثم SYSTEM.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class SystemSettingsController : ControllerBase
    {
        private static readonly HashSet<string> ValidScopes = new(StringComparer.OrdinalIgnoreCase)
        {
            "SYSTEM", "COMPANY", "BRANCH", "FISCAL_YEAR"
        };

        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;
        private readonly SettingsResolverService _resolver;

        public SystemSettingsController(
            AppDbContext context,
            ScreenAuthorizationService authorization,
            AuditTrailService audit,
            SettingsResolverService resolver)
        {
            _context = context;
            _authorization = authorization;
            _audit = audit;
            _resolver = resolver;
        }

        private async Task<IActionResult?> DenyUnlessAsync(ScreenOperation operation)
        {
            if (HttpContext.Items["ServerSession"] is not ServerSession session)
                return Unauthorized();
            return await _authorization.IsAllowedAsync(session, "GeneralSettings", operation)
                ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var denied = await DenyUnlessAsync(ScreenOperation.View);
            if (denied != null) return denied;
            var session = (ServerSession)HttpContext.Items["ServerSession"]!;

            var settings = await _context.System_Settings.AsNoTracking()
                .Where(x => IsVisibleInSession(x, session))
                .OrderBy(x => x.Setting_Key)
                .ThenBy(x => x.Scope)
                .ToListAsync();
            return Ok(settings);
        }

        /// <summary>يعيد القيمة الفعالة بعد تطبيق ترتيب الأولويات، ولا يكشف نطاقات أخرى.</summary>
        [HttpGet("Resolve/{settingKey}")]
        public async Task<IActionResult> Resolve(string settingKey, CancellationToken cancellationToken)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.View);
            if (denied != null) return denied;
            var session = (ServerSession)HttpContext.Items["ServerSession"]!;

            var result = await _resolver.ResolveAsync(settingKey, session, cancellationToken: cancellationToken);
            return result == null
                ? NotFound(new { message = "لا توجد قيمة فعالة لهذا الإعداد ضمن نطاق الجلسة." })
                : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveSystemSettingRequest request)
        {
            var denied = await DenyUnlessAsync(request?.Setting_ID > 0 ? ScreenOperation.Edit : ScreenOperation.Add);
            if (denied != null) return denied;
            var session = (ServerSession)HttpContext.Items["ServerSession"]!;

            if (request == null || string.IsNullOrWhiteSpace(request.Setting_Key) ||
                string.IsNullOrWhiteSpace(request.Setting_Name))
                return BadRequest(new { message = "مفتاح الإعداد واسمه مطلوبان." });

            var scope = NormalizeScope(request.Scope);
            if (!ValidScopes.Contains(scope))
                return BadRequest(new { message = "النطاق غير صالح." });

            var key = request.Setting_Key.Trim().ToUpperInvariant();
            var name = request.Setting_Name.Trim();
            if (key.Length > 100 || name.Length > 200)
                return BadRequest(new { message = "مفتاح الإعداد أو اسمه أطول من الحد المسموح به." });

            var (companyId, branchId, yearId) = ResolveScope(session, scope);
            var duplicate = await _context.System_Settings.AnyAsync(x =>
                x.Setting_Key == key && x.Scope == scope && x.Company_ID == companyId &&
                x.Branch_ID == branchId && x.Fiscal_Year_ID == yearId &&
                x.Setting_ID != request.Setting_ID);
            if (duplicate) return Conflict(new { message = "يوجد إعداد بالمفتاح نفسه داخل هذا النطاق." });

            SystemSetting setting;
            object? before = null;
            if (request.Setting_ID > 0)
            {
                var existing = await _context.System_Settings.FirstOrDefaultAsync(x => x.Setting_ID == request.Setting_ID);
                if (existing == null || !IsVisibleInSession(existing, session))
                    return NotFound(new { message = "الإعداد غير موجود ضمن نطاق الجلسة." });

                setting = existing;
                before = new { setting.Setting_Key, setting.Setting_Value, setting.Scope, setting.Is_Active };
            }
            else
            {
                setting = new SystemSetting { Created_At = DateTime.UtcNow };
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
            setting.Updated_At = DateTime.UtcNow;

            _audit.Add(session, HttpContext, "system_settings",
                request.Setting_ID == 0 ? "new" : request.Setting_ID.ToString(),
                request.Setting_ID == 0 ? "CREATE" : "UPDATE", before,
                new { setting.Setting_Key, setting.Setting_Value, setting.Scope, setting.Is_Active });

            await _context.SaveChangesAsync();
            return Ok(new { message = request.Setting_ID == 0 ? "تمت إضافة الإعداد." : "تم تعديل الإعداد.", setting.Setting_ID });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var denied = await DenyUnlessAsync(ScreenOperation.Delete);
            if (denied != null) return denied;
            var session = (ServerSession)HttpContext.Items["ServerSession"]!;

            var setting = await _context.System_Settings.FirstOrDefaultAsync(x => x.Setting_ID == id);
            if (setting == null || !IsVisibleInSession(setting, session))
                return NotFound(new { message = "الإعداد غير موجود ضمن نطاق الجلسة." });

            setting.Is_Active = false;
            setting.Updated_At = DateTime.UtcNow;
            _audit.Add(session, HttpContext, "system_settings", id.ToString(), "DEACTIVATE",
                new { Is_Active = true }, new { Is_Active = false });
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف الإعداد." });
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
