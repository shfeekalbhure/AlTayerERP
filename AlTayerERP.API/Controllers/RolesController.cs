using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>إدارة الأدوار؛ الحذف منطقي لحماية تاريخ المستخدمين والصلاحيات.</summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditTrailService _audit;

        public RolesController(AppDbContext context, AuditTrailService audit)
        {
            _context = context;
            _audit = audit;
        }

        private bool IsAdministrator(out ServerSession session)
        {
            session = HttpContext.Items["ServerSession"] as ServerSession ?? default!;
            return session != null && session.Is_System_Admin;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            if (!IsAdministrator(out _)) return Forbid();
            return Ok(await _context.Roles.AsNoTracking().OrderBy(x => x.Role_ID).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            if (!IsAdministrator(out _)) return Forbid();
            var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Role_ID == id);
            return role == null ? NotFound("الدور غير موجود.") : Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            if (!IsAdministrator(out var session)) return Forbid();
            if (dto == null || string.IsNullOrWhiteSpace(dto.Role_Name))
                return BadRequest("اسم الدور مطلوب.");

            var code = string.IsNullOrWhiteSpace(dto.Role_Code)
                ? $"ROL{DateTime.UtcNow:yyyyMMddHHmmssfff}"
                : dto.Role_Code.Trim();

            if (await _context.Roles.AnyAsync(x => x.Role_Code == code))
                return Conflict("كود الدور مستخدم مسبقاً.");

            var role = new Role
            {
                Role_Code = code,
                Role_Name = dto.Role_Name.Trim(),
                Description = dto.Description?.Trim(),
                Is_Active = true,
                // لا يسمح إنشاء دور مدير النظام من الواجهة؛ هذه صلاحية نشر/إدارة محمية.
                Is_System_Admin = false,
                Created_At = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            _audit.Add(session, HttpContext, "roles", "new", "CREATE",
                newValues: new { role.Role_Code, role.Role_Name, role.Is_Active });
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Role_ID }, role);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            if (!IsAdministrator(out var session)) return Forbid();
            if (dto == null || string.IsNullOrWhiteSpace(dto.Role_Name))
                return BadRequest("اسم الدور مطلوب.");

            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Role_ID == id);
            if (role == null) return NotFound("الدور غير موجود.");

            if (!string.IsNullOrWhiteSpace(dto.Role_Code) &&
                await _context.Roles.AnyAsync(x => x.Role_ID != id && x.Role_Code == dto.Role_Code.Trim()))
                return Conflict("كود الدور مستخدم مسبقاً.");

            var before = new { role.Role_Code, role.Role_Name, role.Description, role.Is_Active };
            role.Role_Code = string.IsNullOrWhiteSpace(dto.Role_Code) ? role.Role_Code : dto.Role_Code.Trim();
            role.Role_Name = dto.Role_Name.Trim();
            role.Description = dto.Description?.Trim();
            role.Is_Active = dto.Is_Active;
            role.Updated_At = DateTime.UtcNow;

            _audit.Add(session, HttpContext, "roles", id.ToString(), "UPDATE", before,
                new { role.Role_Code, role.Role_Name, role.Description, role.Is_Active });
            await _context.SaveChangesAsync();
            return Ok(role);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeactivateRole(int id)
        {
            if (!IsAdministrator(out var session)) return Forbid();

            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Role_ID == id);
            if (role == null) return NotFound("الدور غير موجود.");
            if (role.Is_System_Admin) return BadRequest("لا يمكن إيقاف دور مدير النظام.");

            role.Is_Active = false;
            role.Updated_At = DateTime.UtcNow;
            _audit.Add(session, HttpContext, "roles", id.ToString(), "DEACTIVATE",
                new { Is_Active = true }, new { Is_Active = false });
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف الدور بنجاح." });
        }
    }
}
