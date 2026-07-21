using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة الإعدادات متعددة المستويات وقراءة قيمها الفعالة.
    /// جميع عمليات الإدارة محصورة بمدير النظام لحماية الإعدادات الحساسة.
    /// </summary>
    [Authorize(Roles = "SystemAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private static readonly string[] AllowedScopeTypes =
        {
            "GLOBAL", "GROUP", "COMPANY", "BRANCH", "MODULE", "SCREEN", "ROLE", "USER"
        };

        private static readonly string[] AllowedDataTypes =
        {
            "STRING", "INTEGER", "DECIMAL", "BOOLEAN", "DATE", "JSON"
        };

        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _context.System_Settings.AsNoTracking()
                .OrderBy(x => x.Module_Name)
                .ThenBy(x => x.Setting_Code)
                .Select(x => new
                {
                    x.Setting_ID,
                    x.Setting_Code,
                    x.Setting_Name,
                    x.Module_Name,
                    x.Data_Type,
                    x.Default_Value,
                    x.Is_Sensitive,
                    x.Is_Active
                })
                .ToListAsync();

            return Ok(settings);
        }

        [HttpGet("{settingId:long}/scope-values")]
        public async Task<IActionResult> GetScopeValues(long settingId)
        {
            bool exists = await _context.System_Settings
                .AnyAsync(x => x.Setting_ID == settingId);

            if (!exists)
                return NotFound("الإعداد غير موجود.");

            var values = await _context.Setting_Scope_Values.AsNoTracking()
                .Where(x => x.Setting_ID == settingId)
                .OrderByDescending(x => x.Is_Active)
                .ThenByDescending(x => x.Created_At)
                .Select(x => new
                {
                    x.Setting_Scope_Value_ID,
                    x.Setting_ID,
                    x.Scope_Type,
                    x.Scope_ID,
                    x.Value,
                    x.Effective_From,
                    x.Effective_To,
                    x.Is_Active,
                    x.Created_By,
                    x.Created_At,
                    x.Change_Reason
                })
                .ToListAsync();

            return Ok(values);
        }

        [HttpPost("{settingId:long}/scope-values")]
        public async Task<IActionResult> CreateScopeValue(long settingId, [FromBody] SettingScopeValueRequest request)
        {
            if (request == null)
                return BadRequest("بيانات القيمة مطلوبة.");

            var setting = await _context.System_Settings
                .FirstOrDefaultAsync(x => x.Setting_ID == settingId && x.Is_Active);

            if (setting == null)
                return NotFound("الإعداد غير موجود أو غير نشط.");

            string scopeType = (request.Scope_Type ?? string.Empty).Trim().ToUpperInvariant();
            string scopeId = (request.Scope_ID ?? string.Empty).Trim();

            if (!AllowedScopeTypes.Contains(scopeType))
                return BadRequest("نوع النطاق غير معتمد.");

            if (string.IsNullOrWhiteSpace(scopeId))
                return BadRequest("معرّف النطاق مطلوب.");

            if (request.Effective_To.HasValue && request.Effective_From.HasValue &&
                request.Effective_To.Value < request.Effective_From.Value)
            {
                return BadRequest("تاريخ نهاية الفعالية لا يمكن أن يسبق تاريخ بدايتها.");
            }

            if (setting.Is_Sensitive && string.IsNullOrWhiteSpace(request.Change_Reason))
                return BadRequest("سبب التغيير إلزامي للإعدادات الحساسة.");

            if (!IsValueValidForType(request.Value, setting.Data_Type))
                return BadRequest("القيمة لا تطابق نوع بيانات الإعداد.");

            string createdBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";

            // لا نحدّث القيمة السابقة حتى يبقى السجل التدقيقي كاملاً.
            _context.Setting_Scope_Values.Add(new Core.Entities.Configuration.SettingScopeValue
            {
                Setting_ID = settingId,
                Scope_Type = scopeType,
                Scope_ID = scopeId,
                Value = request.Value?.Trim(),
                Effective_From = request.Effective_From,
                Effective_To = request.Effective_To,
                Is_Active = true,
                Created_By = createdBy,
                Created_At = DateTime.UtcNow,
                Change_Reason = request.Change_Reason?.Trim()
            });

            await _context.SaveChangesAsync();
            return Ok(new { Message = "تم حفظ تجاوز الإعداد بنجاح." });
        }

        [HttpPost("scope-values/{scopeValueId:long}/deactivate")]
        public async Task<IActionResult> DeactivateScopeValue(long scopeValueId)
        {
            var value = await _context.Setting_Scope_Values
                .FirstOrDefaultAsync(x => x.Setting_Scope_Value_ID == scopeValueId);

            if (value == null)
                return NotFound("قيمة النطاق غير موجودة.");

            if (!value.Is_Active)
                return Ok(new { Message = "القيمة معطلة مسبقاً." });

            value.Is_Active = false;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم تعطيل القيمة مع الاحتفاظ بالسجل." });
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
                .OrderByDescending(x => x.Created_At)
                .ToListAsync();

            foreach (ScopeKey scope in scopes)
            {
                var value = values.FirstOrDefault(x =>
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

        private static bool IsValueValidForType(string? value, string dataType)
        {
            if (value == null)
                return true;

            switch ((dataType ?? "STRING").Trim().ToUpperInvariant())
            {
                case "INTEGER":
                    return long.TryParse(value, out _);
                case "DECIMAL":
                    return decimal.TryParse(value, out _);
                case "BOOLEAN":
                    return bool.TryParse(value, out _);
                case "DATE":
                    return DateTime.TryParse(value, out _);
                case "JSON":
                    try
                    {
                        System.Text.Json.JsonDocument.Parse(value);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                default:
                    return true;
            }
        }

        private sealed record ScopeKey(string Type, string? Id);
    }

    public class SettingScopeValueRequest
    {
        public string Scope_Type { get; set; } = string.Empty;
        public string Scope_ID { get; set; } = string.Empty;
        public string? Value { get; set; }
        public DateTime? Effective_From { get; set; }
        public DateTime? Effective_To { get; set; }
        public string? Change_Reason { get; set; }
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