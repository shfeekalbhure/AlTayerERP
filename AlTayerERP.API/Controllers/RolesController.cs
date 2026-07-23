using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

/// <summary>
/// إدارة الأدوار (Roles). الدور سجل مرجعي لا يحذف فعلياً حتى لا تنكسر
/// علاقة المستخدمين وسجل التدقيق به.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private const string SessionHeader = "X-Session-Token";
    private readonly AppDbContext _context;
    private readonly ServerSessionService _sessions;

    public RolesController(AppDbContext context, ServerSessionService sessions)
    {
        _context = context;
        _sessions = sessions;
    }

    /// <summary>إدارة الدور والصلاحيات لا تتاح إلا لمدير النظام.</summary>
    private bool IsSystemAdmin() =>
        _sessions.TryGet(Request.Headers[SessionHeader].ToString(), out var session) &&
        session.Is_System_Admin;

    /// <summary>جلب الأدوار مرتبة، بما فيها الأدوار الموقوفة لإدارتها.</summary>
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        if (!IsSystemAdmin()) return Forbid();
        return Ok(await _context.Roles.AsNoTracking()
            .OrderBy(x => x.Role_Name)
            .ToListAsync());
    }

    /// <summary>إنشاء دور مع كود ثابت وفريد للاستخدام في الصلاحيات والتكامل.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        if (!IsSystemAdmin()) return Forbid();
        if (dto is null || string.IsNullOrWhiteSpace(dto.Role_Code) ||
            string.IsNullOrWhiteSpace(dto.Role_Name))
            return BadRequest("كود الدور واسمه حقول مطلوبة.");

        var roleCode = dto.Role_Code.Trim().ToUpperInvariant();
        var roleName = dto.Role_Name.Trim();

        if (await _context.Roles.AnyAsync(x => x.Role_Code == roleCode))
            return Conflict("كود الدور مستخدم مسبقاً.");
        if (await _context.Roles.AnyAsync(x => x.Role_Name == roleName))
            return Conflict("اسم الدور مستخدم مسبقاً.");

        var role = new Role
        {
            // Role_Code: معرف ثابت مثل ACCOUNTANT ولا يعتمد على وقت الإنشاء.
            Role_Code = roleCode,
            Role_Name = roleName,
            Description = dto.Description?.Trim(),
            Is_Active = dto.Is_Active,
            Created_At = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRoleById), new { id = role.Role_ID }, role);
    }

    /// <summary>جلب دور محدد بمعرفه الداخلي.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRoleById(int id)
    {
        if (!IsSystemAdmin()) return Forbid();
        var role = await _context.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Role_ID == id);
        return role is null ? NotFound("الدور غير موجود.") : Ok(role);
    }

    /// <summary>تعديل بيانات الدور دون تغيير معرفه أو إنشاء تاريخ جديد له.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
    {
        if (!IsSystemAdmin()) return Forbid();
        if (dto is null || string.IsNullOrWhiteSpace(dto.Role_Code) ||
            string.IsNullOrWhiteSpace(dto.Role_Name))
            return BadRequest("كود الدور واسمه حقول مطلوبة.");

        var role = await _context.Roles.FirstOrDefaultAsync(x => x.Role_ID == id);
        if (role is null) return NotFound("الدور غير موجود.");

        var roleCode = dto.Role_Code.Trim().ToUpperInvariant();
        var roleName = dto.Role_Name.Trim();

        if (await _context.Roles.AnyAsync(x => x.Role_ID != id && x.Role_Code == roleCode))
            return Conflict("كود الدور مستخدم مسبقاً.");
        if (await _context.Roles.AnyAsync(x => x.Role_ID != id && x.Role_Name == roleName))
            return Conflict("اسم الدور مستخدم مسبقاً.");

        // لا يجوز إيقاف دور مدير النظام؛ ينفذ تغيير هذه السياسة من مسار إداري مستقل.
        if (role.Is_System_Admin && !dto.Is_Active)
            return BadRequest("لا يمكن إيقاف دور مدير النظام.");

        role.Role_Code = roleCode;
        role.Role_Name = roleName;
        role.Description = dto.Description?.Trim();
        role.Is_Active = dto.Is_Active;
        role.Updated_At = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(role);
    }

    /// <summary>
    /// إيقاف الدور بدلاً من حذفه. يمنع الإيقاف عندما يملك مستخدمون نشطون هذا الدور.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateRole(int id)
    {
        if (!IsSystemAdmin()) return Forbid();
        var role = await _context.Roles.FirstOrDefaultAsync(x => x.Role_ID == id);
        if (role is null) return NotFound("الدور غير موجود.");
        if (role.Is_System_Admin)
            return BadRequest("لا يمكن إيقاف دور مدير النظام.");
        if (await _context.Users.AnyAsync(x => x.Role_ID == id && x.Is_Active))
            return Conflict("لا يمكن إيقاف دور مرتبط بمستخدمين نشطين. انقلهم إلى دور آخر أولاً.");

        role.Is_Active = false;
        role.Updated_At = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { message = "تم إيقاف الدور دون حذف تاريخه." });
    }
}