using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchTypesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public BranchTypesController(AppDbContext context) => _context = context;

        private IActionResult? RequireSystemAdmin()
        {
            var session = HttpContext.Items["ServerSession"] as ServerSession;
            if (session == null) return Unauthorized("انتهت الجلسة أو أنها غير صالحة.");
            return session.Is_System_Admin ? null : Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            return Ok(await _context.Branch_Types
                .AsNoTracking()
                .Where(x => !activeOnly || x.Is_Active)
                .OrderBy(x => x.Sort_Order).ThenBy(x => x.Branch_Type_Name_AR)
                .ToListAsync());
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookup() => Ok(await _context.Branch_Types.AsNoTracking()
            .Where(x => x.Is_Active)
            .OrderBy(x => x.Sort_Order).ThenBy(x => x.Branch_Type_Name_AR)
            .Select(x => new { x.Branch_Type_Code, x.Branch_Type_Name_AR })
            .ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BranchTypeDto dto)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            var error = await ValidateAsync(dto);
            if (error != null) return BadRequest(error);
            var type = new BranchType
            {
                Branch_Type_Code = dto.Branch_Type_Code.Trim().ToUpperInvariant(),
                Branch_Type_Name_AR = dto.Branch_Type_Name_AR.Trim(),
                Branch_Type_Name_EN = dto.Branch_Type_Name_EN?.Trim(),
                Sort_Order = dto.Sort_Order,
                Is_Active = dto.Is_Active,
                Notes = dto.Notes?.Trim()
            };
            _context.Branch_Types.Add(type);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = type.Branch_Type_ID }, type);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] BranchTypeDto dto)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            var type = await _context.Branch_Types.FindAsync(id);
            if (type == null) return NotFound("نوع الفرع غير موجود.");
            var error = await ValidateAsync(dto, id);
            if (error != null) return BadRequest(error);
            var newCode = dto.Branch_Type_Code.Trim().ToUpperInvariant();
            if (!string.Equals(type.Branch_Type_Code, newCode, StringComparison.OrdinalIgnoreCase) &&
                await _context.Tenant_Branches.AnyAsync(x => x.Branch_Type == type.Branch_Type_Code))
                return BadRequest("لا يمكن تغيير رمز نوع فرع مستخدم. أنشئ نوعاً جديداً ثم انقل الفروع بطريقة معتمدة.");
            if (!dto.Is_Active && await _context.Tenant_Branches.AnyAsync(x => x.Branch_Type == type.Branch_Type_Code && x.Is_Active))
                return BadRequest("لا يمكن إيقاف نوع مرتبط بفروع نشطة. أوقف الفروع أو انقلها أولاً.");
            type.Branch_Type_Code = newCode;
            type.Branch_Type_Name_AR = dto.Branch_Type_Name_AR.Trim();
            type.Branch_Type_Name_EN = dto.Branch_Type_Name_EN?.Trim();
            type.Sort_Order = dto.Sort_Order;
            type.Is_Active = dto.Is_Active;
            type.Notes = dto.Notes?.Trim();
            type.Updated_At = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(type);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null) return accessError;
            var type = await _context.Branch_Types.FindAsync(id);
            if (type == null) return NotFound("نوع الفرع غير موجود.");
            var used = await _context.Tenant_Branches.AnyAsync(x => x.Branch_Type == type.Branch_Type_Code);
            if (used) return BadRequest("لا يمكن حذف نوع مستخدم في فروع. أوقفه بدلاً من الحذف.");
            _context.Branch_Types.Remove(type);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string?> ValidateAsync(BranchTypeDto dto, int? excludeId = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Branch_Type_Code) || string.IsNullOrWhiteSpace(dto.Branch_Type_Name_AR))
                return "رمز النوع واسمه العربي مطلوبان.";
            var code = dto.Branch_Type_Code.Trim().ToUpperInvariant();
            if (code.Length > 30) return "رمز نوع الفرع لا يتجاوز 30 حرفاً.";
            var exists = await _context.Branch_Types.AnyAsync(x => x.Branch_Type_Code == code && (!excludeId.HasValue || x.Branch_Type_ID != excludeId));
            return exists ? "رمز نوع الفرع مستخدم مسبقاً." : null;
        }
    }
}
