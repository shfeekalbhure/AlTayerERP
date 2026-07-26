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
    /// إدارة المجموعات التجارية. مصدر هوية الإنشاء والتعديل هو جلسة الخادم،
    /// ولا يسمح بحذف المجموعة فعلياً لحماية الترابط والتدقيق.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TenantGroupsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TenantGroupsController(AppDbContext context) => _context = context;

        /// <summary>يتأكد من أن الطلب صادر عن مدير النظام بعد مرور Middleware الجلسة.</summary>
        private bool TryGetAdminSession(out ServerSession session)
        {
            session = HttpContext.Items["ServerSession"] as ServerSession
                ?? new ServerSession(string.Empty, 0, 0, false, string.Empty, 0, 0, DateTime.MinValue);
            return session.Is_System_Admin;
        }

        /// <summary>عرض المجموعات التجارية لمدير النظام، بما فيها الموقوفة لإدارتها.</summary>
        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            if (!TryGetAdminSession(out _)) return Forbid();

            var groups = await _context.Tenant_Groups.AsNoTracking()
                .OrderBy(x => x.Sort_Order).ThenBy(x => x.Group_Name_AR)
                .ToListAsync();
            var groupIds = groups.Select(x => x.Group_ID).ToList();
            var companyCounts = await _context.Companies.AsNoTracking()
                .Where(x => groupIds.Contains(x.Group_ID))
                .GroupBy(x => x.Group_ID)
                .Select(x => new { Group_ID = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.Group_ID, x => x.Count);
            var mainCompanyIds = groups.Where(x => !string.IsNullOrWhiteSpace(x.Main_Company_ID))
                .Select(x => x.Main_Company_ID!).Distinct().ToList();
            var mainCompanyNames = await _context.Companies.AsNoTracking()
                .Where(x => mainCompanyIds.Contains(x.Company_ID))
                .Select(x => new { x.Company_ID, x.Company_Name_AR })
                .ToDictionaryAsync(x => x.Company_ID, x => x.Company_Name_AR);
            foreach (var group in groups)
            {
                group.Companies_Count = companyCounts.GetValueOrDefault(group.Group_ID);
                group.Main_Company_Name = !string.IsNullOrWhiteSpace(group.Main_Company_ID)
                    ? mainCompanyNames.GetValueOrDefault(group.Main_Company_ID) : null;
            }
            return Ok(groups);
        }

        /// <summary>عرض مجموعة واحدة مع بيانات التدقيق للقراءة فقط.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(string id)
        {
            if (!TryGetAdminSession(out _)) return Forbid();

            var group = await _context.Tenant_Groups.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Group_ID == id);
            return group is null ? NotFound("المجموعة التجارية غير موجودة.") : Ok(group);
        }

        /// <summary>قائمة الشركات النشطة التابعة للمجموعة فقط لاختيار الشركة الرئيسية.</summary>
        [HttpGet("{id}/companies")]
        public async Task<IActionResult> GetGroupCompanies(string id)
        {
            if (!TryGetAdminSession(out _)) return Forbid();
            if (!await _context.Tenant_Groups.AsNoTracking().AnyAsync(x => x.Group_ID == id))
                return NotFound("المجموعة التجارية غير موجودة.");

            var companies = await _context.Companies.AsNoTracking()
                .Where(x => x.Group_ID == id && x.Is_Active)
                .OrderBy(x => x.Company_Name_AR)
                .Select(x => new { x.Company_ID, x.Company_Name_AR })
                .ToListAsync();
            return Ok(companies);
        }

        /// <summary>إنشاء مجموعة وربط حقول التدقيق بالمستخدم الموجود في جلسة الخادم.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateTenantGroupDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();

            var validation = await ValidateAsync(dto);
            if (validation is not null) return BadRequest(validation);

            var group = new TenantGroup
            {
                Group_ID = Guid.NewGuid().ToString(),
                Created_At = DateTime.UtcNow,
                Created_By = session.User_ID,
                Edit_Count = 0,
                Is_Active = true
            };
            Map(dto, group);
            _context.Tenant_Groups.Add(group);
            // INSERT متوافق مع قيد سجلات التدقيق في قواعد البيانات السابقة.
            AddAuditLog(session, group, "INSERT", null, BuildSnapshot(group));
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGroup), new { id = group.Group_ID }, group);
        }

        /// <summary>تعديل مجموعة مع زيادة العداد من الخادم فقط.</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(string id, [FromBody] CreateTenantGroupDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();

            var group = await _context.Tenant_Groups.FirstOrDefaultAsync(x => x.Group_ID == id);
            if (group is null) return NotFound("المجموعة التجارية غير موجودة.");
            if (!group.Is_Active)
                return Conflict("لا يمكن تعديل مجموعة موقوفة؛ أعد تفعيلها أولاً.");

            var validation = await ValidateAsync(dto, id);
            if (validation is not null) return BadRequest(validation);

            var oldValues = BuildSnapshot(group);
            Map(dto, group);
            group.Updated_At = DateTime.UtcNow;
            group.Updated_By = session.User_ID;
            group.Edit_Count += 1;
            AddAuditLog(session, group, "UPDATE", oldValues, BuildSnapshot(group));
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        /// <summary>
        /// إيقاف المجموعة بدلاً من حذفها. المجموعة المرتبطة بشركات تبقى قابلة
        /// للاستعراض تاريخياً ولا تظهر في شاشة الدخول عند إيقافها.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateGroup(string id, [FromBody] RecordStatusChangeDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("سبب الإيقاف مطلوب.");

            var group = await _context.Tenant_Groups.FirstOrDefaultAsync(x => x.Group_ID == id);
            if (group is null) return NotFound("المجموعة التجارية غير موجودة.");

            if (await _context.Companies.AnyAsync(x => x.Group_ID == id && x.Is_Active))
                return Conflict("لا يمكن إيقاف المجموعة لوجود شركات نشطة مرتبطة بها.");
            if (await _context.Tenant_Groups.AnyAsync(x => x.Parent_Group_ID == id && x.Is_Active))
                return Conflict("لا يمكن إيقاف المجموعة لوجود مجموعات فرعية نشطة مرتبطة بها.");

            var oldValues = BuildSnapshot(group);
            group.Is_Active = false;
            group.Show_In_Login = false;
            group.Updated_At = DateTime.UtcNow;
            group.Updated_By = session.User_ID;
            group.Stopped_By = session.User_ID;
            group.Stopped_At = DateTime.UtcNow;
            group.Stopped_Reason = dto.Reason.Trim();
            group.Edit_Count += 1;
            AddAuditLog(session, group, "DEACTIVATE", oldValues, BuildSnapshot(group));
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إيقاف المجموعة التجارية دون حذف تاريخها." });
        }


        /// <summary>إعادة تفعيل مجموعة مع سبب إلزامي وتسجيل تدقيقي كامل.</summary>
        [HttpPost("{id}/reactivate")]
        public async Task<IActionResult> ReactivateGroup(string id, [FromBody] RecordStatusChangeDto dto)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("سبب إعادة التفعيل مطلوب.");

            var group = await _context.Tenant_Groups.FirstOrDefaultAsync(x => x.Group_ID == id);
            if (group is null) return NotFound("المجموعة التجارية غير موجودة.");
            if (!string.IsNullOrWhiteSpace(group.Parent_Group_ID) &&
                !await _context.Tenant_Groups.AsNoTracking().AnyAsync(x => x.Group_ID == group.Parent_Group_ID && x.Is_Active))
                return Conflict("لا يمكن إعادة تفعيل المجموعة قبل إعادة تفعيل المجموعة الأم.");

            var oldValues = BuildSnapshot(group);
            group.Is_Active = true;
            group.Show_In_Login = true;
            group.Updated_At = DateTime.UtcNow;
            group.Updated_By = session.User_ID;
            group.Reactivated_By = session.User_ID;
            group.Reactivated_At = DateTime.UtcNow;
            group.Reactivate_Reason = dto.Reason.Trim();
            group.Edit_Count += 1;
            AddAuditLog(session, group, "REACTIVATE", oldValues, BuildSnapshot(group));
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        /// <summary>ملخص تدقيق موثوق للعرض فقط في تذييل الشاشة.</summary>
        [HttpGet("{id}/audit-info")]
        public async Task<IActionResult> GetAuditInfo(string id)
        {
            if (!TryGetAdminSession(out _)) return Forbid();
            if (!await _context.Tenant_Groups.AnyAsync(x => x.Group_ID == id)) return NotFound();

            var logs = await _context.Audit_Logs.AsNoTracking()
                .Where(x => x.Table_Name == "tenant_groups" && x.Record_ID == id)
                .OrderBy(x => x.Action_At)
                .ToListAsync();
            var ids = logs.Select(x => int.TryParse(x.User_ID, out var userId) ? userId : 0).Where(x => x > 0).Distinct().ToList();
            var names = await _context.Users.AsNoTracking().Where(x => ids.Contains(x.User_ID)).ToDictionaryAsync(x => x.User_ID, x => x.Full_Name);
            string NameOf(string? userId) => int.TryParse(userId, out var parsed) && names.TryGetValue(parsed, out var name) ? name : "غير متاح";
            var created = logs.FirstOrDefault(x => x.Action_Type is "CREATE" or "INSERT");
            var lastUpdate = logs.LastOrDefault(x => x.Action_Type == "UPDATE");
            return Ok(new { Created_By = NameOf(created?.User_ID), Created_At = created?.Action_At, Updated_By = NameOf(lastUpdate?.User_ID), Updated_At = lastUpdate?.Action_At, Edit_Count = logs.Count(x => x.Action_Type == "UPDATE"), Print_Count = logs.Count(x => x.Action_Type == "PRINT") });
        }

        /// <summary>يسجل معاينة طباعة بطاقة المجموعة ضمن التدقيق المركزي.</summary>
        [HttpPost("{id}/print")]
        public async Task<IActionResult> RegisterPrint(string id)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            var group = await _context.Tenant_Groups.AsNoTracking().FirstOrDefaultAsync(x => x.Group_ID == id);
            if (group is null) return NotFound("المجموعة التجارية غير موجودة.");
            AddAuditLog(session, group, "PRINT", null, BuildSnapshot(group));
            await _context.SaveChangesAsync();
            return Ok();
        }
        /// <summary>يتحقق من الحقول الفريدة وصحة المجموعة الأم قبل الحفظ.</summary>
        private async Task<string?> ValidateAsync(CreateTenantGroupDto dto, string? excludeId = null)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Group_Code) ||
                string.IsNullOrWhiteSpace(dto.Group_Name_AR) ||
                string.IsNullOrWhiteSpace(dto.Short_Name) ||
                string.IsNullOrWhiteSpace(dto.Group_Type))
                return "كود المجموعة والاسم العربي والاسم المختصر ونوع المجموعة حقول مطلوبة.";

            var code = dto.Group_Code.Trim().ToUpperInvariant();
            if (await _context.Tenant_Groups.AnyAsync(x => x.Group_Code == code && x.Group_ID != excludeId))
                return "كود المجموعة مستخدم مسبقاً.";

            if (!string.IsNullOrWhiteSpace(dto.Parent_Group_ID))
            {
                if (dto.Parent_Group_ID == excludeId)
                    return "لا يمكن أن تكون المجموعة أباً لنفسها.";

                var parentExists = await _context.Tenant_Groups.AnyAsync(x =>
                    x.Group_ID == dto.Parent_Group_ID && x.Is_Active);
                if (!parentExists)
                    return "المجموعة الأم غير موجودة أو موقوفة.";

                // يمنع تكوين دورة: أ ← ب ثم ب ← أ، أو أي مستوى أعمق منها.
                var visited = new HashSet<string>(StringComparer.Ordinal) { excludeId ?? string.Empty };
                var parentId = dto.Parent_Group_ID;
                while (!string.IsNullOrWhiteSpace(parentId))
                {
                    if (!visited.Add(parentId))
                        return "لا يمكن ربط المجموعة الأم لأنه سينشئ دورة هرمية.";
                    parentId = await _context.Tenant_Groups.AsNoTracking()
                        .Where(x => x.Group_ID == parentId)
                        .Select(x => x.Parent_Group_ID)
                        .FirstOrDefaultAsync();
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Main_Company_ID))
            {
                if (string.IsNullOrWhiteSpace(excludeId))
                    return "احفظ المجموعة أولاً ثم اختر الشركة الرئيسية المرتبطة بها.";
                var companyValid = await _context.Companies.AsNoTracking().AnyAsync(x => x.Company_ID == dto.Main_Company_ID && x.Group_ID == excludeId && x.Is_Active);
                if (!companyValid) return "الشركة الرئيسية يجب أن تكون نشطة ومرتبطة بهذه المجموعة.";
            }
            return null;
        }

        /// <summary>
        /// يضيف سجل تدقيق داخل نفس وحدة العمل قبل الحفظ. قيمة المستخدم والفرع
        /// تأتي من ServerSession ولا يعتمد السجل على أي قيمة من واجهة المكتب.
        /// </summary>
        private void AddAuditLog(ServerSession session, TenantGroup group, string action,
            string? oldValues, string newValues)
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
                Notes = "إدارة المجموعات التجارية"
            });
        }

        /// <summary>ينتج لقطة بيانات عمل قابلة للمراجعة دون بيانات سرية.</summary>
        private static string BuildSnapshot(TenantGroup group) =>
            JsonSerializer.Serialize(new
            {
                group.Group_ID, group.Group_Code, group.Group_Name_AR, group.Group_Name_EN,
                group.Short_Name, group.Group_Type, group.Parent_Group_ID,
                group.Main_Company_ID, group.Default_Currency_Code, group.Show_In_Login,
                group.Sort_Order, group.Is_Active, group.Notes, group.Edit_Count
            });

        /// <summary>ينقل حقول العمل فقط من DTO؛ حقول التدقيق مستثناة عمداً.</summary>
        private static void Map(CreateTenantGroupDto dto, TenantGroup group)
        {
            group.Group_Code = dto.Group_Code.Trim().ToUpperInvariant();
            group.Group_Name_AR = dto.Group_Name_AR.Trim();
            group.Group_Name_EN = dto.Group_Name_EN?.Trim() ?? string.Empty;
            group.Short_Name = dto.Short_Name.Trim();
            group.Group_Type = dto.Group_Type.Trim();
            group.Parent_Group_ID = string.IsNullOrWhiteSpace(dto.Parent_Group_ID) ? null : dto.Parent_Group_ID;
            group.Main_Company_ID = string.IsNullOrWhiteSpace(dto.Main_Company_ID) ? null : dto.Main_Company_ID;
            group.Default_Currency_Code = dto.Default_Currency_Code?.Trim();
            group.Country_Name = dto.Country_Name?.Trim();
            group.City_Name = dto.City_Name?.Trim();
            group.Short_Address = dto.Short_Address?.Trim();
            group.Phone = dto.Phone?.Trim();
            group.Email = dto.Email?.Trim();
            group.Manager_Name = dto.Manager_Name?.Trim();
            group.Show_In_Login = dto.Show_In_Login;
            group.Sort_Order = dto.Sort_Order;
            group.Notes = dto.Notes?.Trim();
            // الحالة لا تتغير عبر الحفظ العام؛ الإيقاف وإعادة التفعيل مساران مستقلان بسبب وتدقيق.
        }
    }
}
