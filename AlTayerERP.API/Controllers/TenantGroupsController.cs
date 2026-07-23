using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantGroupsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NumberGeneratorService _numberGenerator;

        public TenantGroupsController(AppDbContext context, NumberGeneratorService numberGenerator)
        {
            _context = context;
            _numberGenerator = numberGenerator;
        }

        private IActionResult? RequireSystemAdmin()
        {
            var session = HttpContext.Items["ServerSession"] as ServerSession;
            if (session == null) return Unauthorized("انتهت الجلسة أو أنها غير صالحة.");
            return session.Is_System_Admin ? null : Forbid();
        }

        // POST: api/TenantGroups
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateTenantGroupDto dto)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            if (dto == null || string.IsNullOrWhiteSpace(dto.Group_Name_AR))
            {
                return BadRequest("بيانات المجموعة غير مكتملة أو اسم المجموعة بالعربي فارغ!");
            }

            var groupName = dto.Group_Name_AR.Trim();
            if (await _context.Tenant_Groups.AnyAsync(x => x.Group_Name_AR == groupName))
                return BadRequest("اسم المجموعة التجارية مستخدم مسبقاً.");

            var newGroup = new TenantGroup
            {
                Group_ID = Guid.NewGuid().ToString(),
                Group_Code = await _numberGenerator.GenerateNextNumberAsync("BUSINESS_GROUP"),
                Group_Name_AR = groupName,
                Group_Name_EN = dto.Group_Name_EN?.Trim() ?? string.Empty,
                Notes = dto.Notes?.Trim(),
                Created_At = DateTime.UtcNow,
                Is_Active = true
            };

            try
            {
                await _context.Tenant_Groups.AddAsync(newGroup);
                await AddAuditAsync(newGroup, "CREATE");
                await _context.SaveChangesAsync();

                return Ok(newGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء الحفظ في قاعدة البيانات: {ex.Message}");
            }
        }

        // GET: api/TenantGroups?activeOnly=false
        [HttpGet]
        public async Task<IActionResult> GetGroups([FromQuery] bool activeOnly = false)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            try
            {
                var groups = await _context.Tenant_Groups
                    .Where(g => !activeOnly || g.Is_Active)
                    .Select(g => new
                    {
                        g.Group_ID,
                        g.Group_Code,
                        g.Group_Name_AR,
                        g.Group_Name_EN,
                        g.Notes,
                        g.Is_Active,
                        g.Created_At,
                        g.Updated_At
                    })
                    .ToListAsync();

                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء جلب المجموعات: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(string id, [FromBody] CreateTenantGroupDto dto)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            if (dto == null || string.IsNullOrWhiteSpace(dto.Group_Name_AR))
                return BadRequest("اسم المجموعة بالعربي مطلوب.");
            var group = await _context.Tenant_Groups.FindAsync(id);
            if (group == null) return NotFound("المجموعة التجارية غير موجودة.");
            var groupName = dto.Group_Name_AR.Trim();
            if (await _context.Tenant_Groups.AnyAsync(x => x.Group_ID != id && x.Group_Name_AR == groupName))
                return BadRequest("اسم المجموعة التجارية مستخدم مسبقاً.");
            group.Group_Name_AR = groupName;
            group.Group_Name_EN = dto.Group_Name_EN?.Trim() ?? string.Empty;
            group.Notes = dto.Notes?.Trim();
            group.Updated_At = DateTime.UtcNow;
            await AddAuditAsync(group, "UPDATE");
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] bool isActive)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            var group = await _context.Tenant_Groups.FindAsync(id);
            if (group == null) return NotFound("المجموعة التجارية غير موجودة.");
            if (!isActive && await _context.Companies.AnyAsync(x => x.Group_ID == id && x.Is_Active))
                return BadRequest("لا يمكن إيقاف مجموعة بها شركات نشطة. أوقف شركاتها أولاً.");
            group.Is_Active = isActive;
            group.Updated_At = DateTime.UtcNow;
            await AddAuditAsync(group, isActive ? "ACTIVATE" : "DEACTIVATE");
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(string id)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            var group = await _context.Tenant_Groups.FindAsync(id);
            if (group == null) return NotFound("المجموعة التجارية غير موجودة.");
            if (await _context.Companies.AnyAsync(x => x.Group_ID == id))
                return BadRequest("لا يمكن حذف مجموعة مرتبطة بشركات. أوقفها بدلاً من الحذف.");
            _context.Tenant_Groups.Remove(group);
            await AddAuditAsync(group, "DELETE");
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task AddAuditAsync(TenantGroup group, string action)
        {
            var session = HttpContext.Items["ServerSession"] as ServerSession;
            await _context.Audit_Logs.AddAsync(new AuditLog
            {
                Table_Name = "tenant_groups",
                Record_ID = group.Group_ID,
                Action_Type = action,
                User_ID = session?.User_ID.ToString(),
                Branch_ID = session?.Branch_ID.ToString(),
                Action_At = DateTime.UtcNow,
                Action_Channel = "DESKTOP",
                New_Values = System.Text.Json.JsonSerializer.Serialize(new
                {
                    group.Group_Code,
                    group.Group_Name_AR,
                    group.Is_Active
                })
            });
        }
    }
}
