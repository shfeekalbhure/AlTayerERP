using AlTayerERP.API.DTOs;
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
        public TenantGroupsController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.Tenant_Groups
                .AsNoTracking()
                .OrderBy(x => x.Sort_Order)
                .ThenBy(x => x.Group_Name_AR)
                .ToListAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(string id)
        {
            var group = await _context.Tenant_Groups.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Group_ID == id);
            return group == null ? NotFound("المجموعة التجارية غير موجودة.") : Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateTenantGroupDto dto)
        {
            var validation = await ValidateAsync(dto);
            if (validation != null) return BadRequest(validation);

            var group = new TenantGroup
            {
                Group_ID = Guid.NewGuid().ToString(),
                Created_At = DateTime.UtcNow
            };
            Map(dto, group);
            _context.Tenant_Groups.Add(group);
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(string id, [FromBody] CreateTenantGroupDto dto)
        {
            var group = await _context.Tenant_Groups.FirstOrDefaultAsync(x => x.Group_ID == id);
            if (group == null) return NotFound("المجموعة التجارية غير موجودة.");

            var validation = await ValidateAsync(dto, id);
            if (validation != null) return BadRequest(validation);

            Map(dto, group);
            group.Updated_At = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(string id)
        {
            var group = await _context.Tenant_Groups.FirstOrDefaultAsync(x => x.Group_ID == id);
            if (group == null) return NotFound("المجموعة التجارية غير موجودة.");

            var used = await _context.Companies.AnyAsync(x => x.Group_ID == id);
            if (used) return BadRequest("لا يمكن حذف مجموعة مرتبطة بشركات. أوقفها بدلاً من الحذف.");

            _context.Tenant_Groups.Remove(group);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string?> ValidateAsync(CreateTenantGroupDto dto, string? excludeId = null)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Group_Code) ||
                string.IsNullOrWhiteSpace(dto.Group_Name_AR) ||
                string.IsNullOrWhiteSpace(dto.Short_Name) ||
                string.IsNullOrWhiteSpace(dto.Group_Type))
                return "كود المجموعة والاسم العربي والاسم المختصر ونوع المجموعة حقول مطلوبة.";

            var code = dto.Group_Code.Trim().ToUpperInvariant();
            if (await _context.Tenant_Groups.AnyAsync(x => x.Group_Code == code && x.Group_ID != excludeId))
                return "كود المجموعة مستخدم مسبقاً.";

            if (!string.IsNullOrWhiteSpace(dto.Parent_Group_ID) && dto.Parent_Group_ID == excludeId)
                return "لا يمكن أن تكون المجموعة أباً لنفسها.";

            return null;
        }

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