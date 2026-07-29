using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class TenantGroupsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TenantGroupsController> _logger;

        public TenantGroupsController(AppDbContext context, ILogger<TenantGroupsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private bool TryGetAdminSession(out ServerSession session)
        {
            session = HttpContext.Items["ServerSession"] as ServerSession
                ?? new ServerSession(string.Empty, 0, 0, false, string.Empty, 0, 0, DateTime.MinValue);
            return session.Is_System_Admin;
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups(CancellationToken cancellationToken)
        {
            if (!TryGetAdminSession(out _)) return Forbid();

            try
            {
                // القراءة الخام تستخدم COALESCE كي تتحمل قواعد قديمة تحتوي NULL
                // من دون تغيير بياناتها أو جعل الحقول الإلزامية اختيارية عند الحفظ.
                const string sql = """
                    SELECT
                        COALESCE(Group_ID, '') AS Group_ID,
                        COALESCE(Group_Code, '') AS Group_Code,
                        COALESCE(Group_Name_AR, '') AS Group_Name_AR,
                        COALESCE(Group_Name_EN, '') AS Group_Name_EN,
                        COALESCE(Is_Default, 0) AS Is_Default,
                        COALESCE(Show_In_Login, 1) AS Show_In_Login,
                        COALESCE(Show_In_Tree, 1) AS Show_In_Tree,
                        COALESCE(Notes, '') AS Notes,
                        COALESCE(Is_Active, 1) AS Is_Active
                    FROM tenant_groups
                    ORDER BY COALESCE(Is_Default, 0) DESC,
                             COALESCE(Group_Name_AR, '')
                    """;

                var groups = await _context.Database
                    .SqlQueryRaw<TenantGroupLookupResult>(sql)
                    .ToListAsync(cancellationToken);

                return Ok(groups);
            }
            catch (Exception ex)
            {
                LogSafeError(ex, "تعذر تحميل المجموعات التجارية من قاعدة البيانات.");
                return SafeServerError("تعذر تحميل المجموعات التجارية. راجع مسؤول النظام.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(string id, CancellationToken cancellationToken)
        {
            if (!TryGetAdminSession(out _)) return Forbid();

            try
            {
                const string sql = """
                    SELECT
                        COALESCE(Group_ID, '') AS Group_ID,
                        COALESCE(Group_Code, '') AS Group_Code,
                        COALESCE(Group_Name_AR, '') AS Group_Name_AR,
                        COALESCE(Group_Name_EN, '') AS Group_Name_EN,
                        COALESCE(Is_Default, 0) AS Is_Default,
                        COALESCE(Show_In_Login, 1) AS Show_In_Login,
                        COALESCE(Show_In_Tree, 1) AS Show_In_Tree,
                        COALESCE(Notes, '') AS Notes,
                        COALESCE(Is_Active, 1) AS Is_Active
                    FROM tenant_groups
                    WHERE Group_ID = {0}
                    """;

                var group = await _context.Database
                    .SqlQueryRaw<TenantGroupLookupResult>(sql, id)
                    .FirstOrDefaultAsync(cancellationToken);

                return group is null ? NotFound("المجموعة التجارية غير موجودة.") : Ok(group);
            }
            catch (Exception ex)
            {
                LogSafeError(ex, "تعذر تحميل المجموعة التجارية المطلوبة.");
                return SafeServerError("تعذر تحميل المجموعة التجارية. راجع مسؤول النظام.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateTenantGroupDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            var validation = await ValidateAsync(dto, null, cancellationToken);
            if (validation is not null) return BadRequest(validation);

            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                if (dto.Is_Default)
                    await ClearOtherDefaultsAsync(null, cancellationToken);

                var group = new TenantGroup
                {
                    Group_ID = Guid.NewGuid().ToString(),
                    Created_At = DateTime.UtcNow,
                    Is_Active = true
                };
                Map(dto, group);
                _context.Tenant_Groups.Add(group);
                AddAuditLog(session, group, "INSERT", null, BuildSnapshot(group));
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return CreatedAtAction(nameof(GetGroup), new { id = group.Group_ID }, group);
            }
            catch (DbUpdateException ex)
            {
                LogSafeError(ex, "تعذر حفظ المجموعة التجارية بسبب خطأ في تحديث البيانات.");
                return SafeServerError("تعذر حفظ المجموعة التجارية. راجع مسؤول النظام.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(string id, [FromBody] CreateTenantGroupDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();

            var group = await _context.Tenant_Groups.FirstOrDefaultAsync(x => x.Group_ID == id, cancellationToken);
            if (group is null) return NotFound("المجموعة التجارية غير موجودة.");
            if (!group.Is_Active) return Conflict("لا يمكن تعديل مجموعة موقوفة؛ أعد تفعيلها أولاً.");

            var validation = await ValidateAsync(dto, id, cancellationToken);
            if (validation is not null) return BadRequest(validation);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            if (dto.Is_Default)
                await ClearOtherDefaultsAsync(id, cancellationToken);

            var oldValues = BuildSnapshot(group);
            Map(dto, group);
            AddAuditLog(session, group, "UPDATE", oldValues, BuildSnapshot(group));
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(group);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateGroup(string id, [FromBody] RecordStatusChangeDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Reason)) return BadRequest("سبب الإيقاف مطلوب.");

            var group = await _context.Tenant_Groups.FirstOrDefaultAsync(x => x.Group_ID == id, cancellationToken);
            if (group is null) return NotFound("المجموعة التجارية غير موجودة.");
            if (group.Is_Default) return Conflict("لا يمكن إيقاف المجموعة الافتراضية قبل تعيين مجموعة افتراضية أخرى.");
            if (await _context.Companies.AnyAsync(x => x.Group_ID == id && x.Is_Active, cancellationToken))
                return Conflict("لا يمكن إيقاف المجموعة لوجود شركات نشطة مرتبطة بها.");

            var oldValues = BuildSnapshot(group);
            group.Is_Active = false;
            group.Show_In_Login = false;
            group.Show_In_Tree = false;
            AddAuditLog(session, group, "DEACTIVATE", oldValues, BuildSnapshot(group), dto.Reason.Trim());
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new { message = "تم إيقاف المجموعة التجارية دون حذف تاريخها." });
        }

        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> ReactivateGroup(string id, [FromBody] RecordStatusChangeDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Reason)) return BadRequest("سبب إعادة التفعيل مطلوب.");

            var group = await _context.Tenant_Groups.FirstOrDefaultAsync(x => x.Group_ID == id, cancellationToken);
            if (group is null) return NotFound("المجموعة التجارية غير موجودة.");

            var oldValues = BuildSnapshot(group);
            group.Is_Active = true;
            AddAuditLog(session, group, "REACTIVATE", oldValues, BuildSnapshot(group), dto.Reason.Trim());
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(group);
        }

        [HttpGet("{id}/audit-info")]
        public async Task<IActionResult> GetAuditInfo(string id, CancellationToken cancellationToken)
        {
            if (!TryGetAdminSession(out _)) return Forbid();
            if (!await _context.Tenant_Groups.AnyAsync(x => x.Group_ID == id, cancellationToken)) return NotFound();

            var logs = await _context.Audit_Logs.AsNoTracking()
                .Where(x => x.Table_Name == "tenant_groups" && x.Record_ID == id)
                .OrderBy(x => x.Action_At)
                .ToListAsync(cancellationToken);
            var ids = logs.Select(x => int.TryParse(x.User_ID, out var userId) ? userId : 0).Where(x => x > 0).Distinct().ToList();
            var names = await _context.Users.AsNoTracking().Where(x => ids.Contains(x.User_ID))
                .ToDictionaryAsync(x => x.User_ID, x => x.Full_Name, cancellationToken);
            string NameOf(string? userId) => int.TryParse(userId, out var parsed) && names.TryGetValue(parsed, out var name) ? name : "غير متاح";
            var created = logs.FirstOrDefault(x => x.Action_Type is "CREATE" or "INSERT");
            var lastUpdate = logs.LastOrDefault(x => x.Action_Type == "UPDATE");
            return Ok(new
            {
                Created_By = NameOf(created?.User_ID),
                Created_At = created?.Action_At,
                Updated_By = NameOf(lastUpdate?.User_ID),
                Updated_At = lastUpdate?.Action_At,
                Edit_Count = logs.Count(x => x.Action_Type == "UPDATE"),
                Print_Count = logs.Count(x => x.Action_Type == "PRINT")
            });
        }

        [HttpPost("{id}/print")]
        public async Task<IActionResult> RegisterPrint(string id, CancellationToken cancellationToken)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            var group = await _context.Tenant_Groups.AsNoTracking().FirstOrDefaultAsync(x => x.Group_ID == id, cancellationToken);
            if (group is null) return NotFound("المجموعة التجارية غير موجودة.");
            AddAuditLog(session, group, "PRINT", null, BuildSnapshot(group));
            await _context.SaveChangesAsync(cancellationToken);
            return Ok();
        }

        private async Task<string?> ValidateAsync(CreateTenantGroupDto dto, string? excludeId, CancellationToken cancellationToken)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Group_Code) ||
                string.IsNullOrWhiteSpace(dto.Group_Name_AR))
                return "كود المجموعة والاسم العربي حقول مطلوبة.";

            var code = dto.Group_Code.Trim().ToUpperInvariant();
            if (await _context.Tenant_Groups.AnyAsync(x => x.Group_Code == code && x.Group_ID != excludeId, cancellationToken))
                return "كود المجموعة مستخدم مسبقاً.";
            return null;
        }

        private async Task ClearOtherDefaultsAsync(string? currentId, CancellationToken cancellationToken)
        {
            var defaults = await _context.Tenant_Groups
                .Where(x => x.Is_Default && x.Group_ID != currentId)
                .ToListAsync(cancellationToken);
            foreach (var item in defaults) item.Is_Default = false;
        }

        private void AddAuditLog(ServerSession session, TenantGroup group, string action, string? oldValues, string newValues, string? reason = null)
        {
            _context.Audit_Logs.Add(new AlTayerERP.Core.Entities.Accounting.AuditLog
            {
                Table_Name = "tenant_groups",
                Record_ID = group.Group_ID,
                Action_Type = action,
                User_ID = session.User_ID.ToString(),
                Branch_ID = session.Branch_ID.ToString(),
                Action_At = DateTime.UtcNow,
                Old_Values = oldValues,
                New_Values = newValues,
                Action_Channel = "DESKTOP",
                Device_Name = Request.Headers["X-Device-ID"].ToString(),
                IP_Address = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Notes = string.IsNullOrWhiteSpace(reason)
                    ? "إدارة المجموعات التجارية"
                    : $"إدارة المجموعات التجارية - السبب: {reason}"
            });
        }

        private static string BuildSnapshot(TenantGroup group) => JsonSerializer.Serialize(new
        {
            group.Group_ID,
            group.Group_Code,
            group.Group_Name_AR,
            group.Group_Name_EN,
            group.Is_Default,
            group.Show_In_Login,
            group.Show_In_Tree,
            group.Is_Active,
            group.Notes
        });

        private static void Map(CreateTenantGroupDto dto, TenantGroup group)
        {
            group.Group_Code = dto.Group_Code.Trim().ToUpperInvariant();
            group.Group_Name_AR = dto.Group_Name_AR.Trim();
            group.Group_Name_EN = string.IsNullOrWhiteSpace(dto.Group_Name_EN) ? null : dto.Group_Name_EN.Trim();
            group.Is_Default = dto.Is_Default;
            group.Show_In_Login = dto.Show_In_Login;
            group.Show_In_Tree = dto.Show_In_Tree;
            group.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        }

        /// <summary>يسجل نوع الخطأ ورسالة منقحة فقط دون رؤوس الطلب أو أسرار الاتصال.</summary>
        private void LogSafeError(Exception exception, string operation)
        {
            _logger.LogError("{Operation} النوع: {ExceptionType}. الرسالة: {SafeMessage}",
                operation,
                exception.GetType().FullName,
                RedactSecrets(exception.Message));
        }

        private static string RedactSecrets(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "لا توجد رسالة تفصيلية.";
            var result = value;
            foreach (var key in new[] { "Authorization", "X-Session-Token", "Cookie", "Password", "ConnectionString", "Bearer" })
                result = Regex.Replace(result, $@"(?i){Regex.Escape(key)}\s*[:=]\s*[^\s,;]+", $"{key}=[محجوب]");
            return result.Length > 800 ? result[..800] : result;
        }

        /// <summary>يعيد نصاً عربياً مباشراً كي لا تعرض الواجهة JSON أو تفاصيل تقنية.</summary>
        private static ContentResult SafeServerError(string message) => new()
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            ContentType = "text/plain; charset=utf-8",
            Content = message
        };

        /// <summary>نموذج قراءة آمن مستقل عن قيود nullability في كيان EF.</summary>
        public sealed class TenantGroupLookupResult
        {
            public string Group_ID { get; set; } = string.Empty;
            public string Group_Code { get; set; } = string.Empty;
            public string Group_Name_AR { get; set; } = string.Empty;
            public string Group_Name_EN { get; set; } = string.Empty;
            public bool Is_Default { get; set; }
            public bool Show_In_Login { get; set; }
            public bool Show_In_Tree { get; set; }
            public string Notes { get; set; } = string.Empty;
            public bool Is_Active { get; set; }
        }
    }
}
