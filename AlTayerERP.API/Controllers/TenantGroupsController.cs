using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class TenantGroupsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TenantGroupsController(AppDbContext context) => _context = context;

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

            var groups = await _context.Tenant_Groups.AsNoTracking()
                .OrderByDescending(x => x.Is_Default)
                .ThenBy(x => x.Group_Name_AR)
                .Select(x => new TenantGroup
                {
                    Group_ID = x.Group_ID,
                    Group_Code = x.Group_Code,
                    Group_Name_AR = x.Group_Name_AR,
                    Group_Name_EN = x.Group_Name_EN,
                    Is_Default = x.Is_Default,
                    Show_In_Login = x.Show_In_Login,
                    Show_In_Tree = x.Show_In_Tree,
                    Notes = x.Notes,
                    Is_Active = x.Is_Active
                })
                .ToListAsync(cancellationToken);

            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(string id, CancellationToken cancellationToken)
        {
            if (!TryGetAdminSession(out _)) return Forbid();

            var group = await _context.Tenant_Groups.AsNoTracking()
                .Where(x => x.Group_ID == id)
                .Select(x => new TenantGroup
                {
                    Group_ID = x.Group_ID,
                    Group_Code = x.Group_Code,
                    Group_Name_AR = x.Group_Name_AR,
                    Group_Name_EN = x.Group_Name_EN,
                    Is_Default = x.Is_Default,
                    Show_In_Login = x.Show_In_Login,
                    Show_In_Tree = x.Show_In_Tree,
                    Notes = x.Notes,
                    Is_Active = x.Is_Active
                })
                .FirstOrDefaultAsync(cancellationToken);

            return group is null ? NotFound("المجموعة التجارية غير موجودة.") : Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateTenantGroupDto dto, CancellationToken cancellationToken)
        {
            if (!TryGetAdminSession(out var session)) return Forbid();
            var validation = await ValidateAsync(dto, null, cancellationToken);
            if (validation is not null) return BadRequest(validation);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            if (dto.Is_Default)
                await ClearOtherDefaultsAsync(null, cancellationToken);

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
            AddAuditLog(session, group, "INSERT", null, BuildSnapshot(group));
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return CreatedAtAction(nameof(GetGroup), new { id = group.Group_ID }, group);
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
            group.Updated_At = DateTime.UtcNow;
            group.Updated_By = session.User_ID;
            group.Edit_Count += 1;
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
            group.Updated_At = DateTime.UtcNow;
            group.Updated_By = session.User_ID;
            group.Stopped_By = session.User_ID;
            group.Stopped_At = DateTime.UtcNow;
            group.Stopped_Reason = dto.Reason.Trim();
            group.Edit_Count += 1;
            AddAuditLog(session, group, "DEACTIVATE", oldValues, BuildSnapshot(group));
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
            group.Updated_At = DateTime.UtcNow;
            group.Updated_By = session.User_ID;
            group.Reactivated_By = session.User_ID;
            group.Reactivated_At = DateTime.UtcNow;
            group.Reactivate_Reason = dto.Reason.Trim();
            group.Edit_Count += 1;
            AddAuditLog(session, group, "REACTIVATE", oldValues, BuildSnapshot(group));
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

        private void AddAuditLog(ServerSession session, TenantGroup group, string action, string? oldValues, string newValues)
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
            group.Notes,
            group.Edit_Count
        });

        private static void Map(CreateTenantGroupDto dto, TenantGroup group)
        {
            group.Group_Code = dto.Group_Code.Trim().ToUpperInvariant();
            group.Group_Name_AR = dto.Group_Name_AR.Trim();
            group.Group_Name_EN = dto.Group_Name_EN?.Trim() ?? string.Empty;
            group.Short_Name = dto.Group_Name_AR.Trim();
            group.Is_Default = dto.Is_Default;
            group.Show_In_Login = dto.Show_In_Login;
            group.Show_In_Tree = dto.Show_In_Tree;
            group.Notes = dto.Notes?.Trim();
        }
    }
}
