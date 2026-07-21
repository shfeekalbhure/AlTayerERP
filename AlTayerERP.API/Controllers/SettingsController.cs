using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// محرك قراءة الإعدادات الهرمية.
    /// في هذه المرحلة يقتصر على مدير النظام لحماية الإعدادات الحساسة.
    /// </summary>
    [Authorize(Roles = "SystemAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("resolve")]
        public async Task<IActionResult> Resolve([FromBody] SettingResolveRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Setting_Code))
                return BadRequest("رمز الإعداد مطلوب.");

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? roleId = User.FindFirstValue("role_id");
            string? companyId = User.FindFirstValue("company_id");
            string? branchId = User.FindFirstValue("branch_id");

            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(roleId) ||
                string.IsNullOrWhiteSpace(companyId) ||
                string.IsNullOrWhiteSpace(branchId))
            {
                return Unauthorized("رمز الدخول لا يحتوي نطاق الجلسة كاملاً.");
            }

            var setting = await _context.System_Settings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Setting_Code == request.Setting_Code && x.Is_Active);

            if (setting == null)
                return NotFound("الإعداد غير موجود أو غير نشط.");

            // يطبق المحرك القيمة الأكثر تخصيصاً قبل الرجوع للقيمة الافتراضية.
            var scopes = new[]
            {
                new ScopeKey("USER", userId),
                new ScopeKey("ROLE", roleId),
                new ScopeKey("SCREEN", request.Screen_Code),
                new ScopeKey("MODULE", request.Module_Name),
                new ScopeKey("BRANCH", branchId),
                new ScopeKey("COMPANY", companyId),
                new ScopeKey("GLOBAL", "GLOBAL")
            }
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToList();

            var values = await _context.Setting_Scope_Values.AsNoTracking()
                .Where(x => x.Setting_ID == setting.Setting_ID &&
                            x.Is_Active &&
                            (x.Effective_From == null || x.Effective_From <= DateTime.UtcNow) &&
                            (x.Effective_To == null || x.Effective_To >= DateTime.UtcNow))
                .ToListAsync();

            foreach (ScopeKey scope in scopes)
            {
                var value = values.LastOrDefault(x =>
                    string.Equals(x.Scope_Type, scope.Type, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Scope_ID, scope.Id, StringComparison.OrdinalIgnoreCase));

                if (value != null)
                {
                    return Ok(new SettingResolveResponse
                    {
                        Setting_Code = setting.Setting_Code,
                        Value = value.Value,
                        Source_Scope_Type = scope.Type
                    });
                }
            }

            return Ok(new SettingResolveResponse
            {
                Setting_Code = setting.Setting_Code,
                Value = setting.Default_Value,
                Source_Scope_Type = "DEFAULT"
            });
        }

        private sealed record ScopeKey(string Type, string? Id);
    }

    public class SettingResolveRequest
    {
        public string Setting_Code { get; set; } = string.Empty;
        public string? Module_Name { get; set; }
        public string? Screen_Code { get; set; }
    }

    public class SettingResolveResponse
    {
        public string Setting_Code { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string Source_Scope_Type { get; set; } = string.Empty;
    }
}