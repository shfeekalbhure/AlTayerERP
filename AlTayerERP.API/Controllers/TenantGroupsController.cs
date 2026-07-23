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
                Edit_Count = 0
            };
            Map(dto, group);
            _context.Tenant_Groups.Add(group);
            AddAuditLog(session, group, "CREATE", null, BuildSnapshot(group));
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
            group.Is_Active = dto.Is_Active;
        }
    }
}