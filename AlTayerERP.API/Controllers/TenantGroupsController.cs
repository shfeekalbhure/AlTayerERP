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

        public TenantGroupsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/TenantGroups
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateTenantGroupDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Group_Name_AR))
            {
                return BadRequest("بيانات المجموعة غير مكتملة أو اسم المجموعة بالعربي فارغ!");
            }

            var newGroup = new TenantGroup
            {
                Group_ID = Guid.NewGuid().ToString(),
                Group_Name_AR = dto.Group_Name_AR.Trim(),
                Group_Name_EN = dto.Group_Name_EN?.Trim() ?? string.Empty,
                Created_At = DateTime.UtcNow,
                Is_Active = true
            };

            try
            {
                await _context.Tenant_Groups.AddAsync(newGroup);
                await _context.SaveChangesAsync();

                return Ok(newGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء الحفظ في قاعدة البيانات: {ex.Message}");
            }
        }

        // GET: api/TenantGroups
        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            try
            {
                var groups = await _context.Tenant_Groups
                    .Where(g => g.Is_Active)
                    .Select(g => new
                    {
                        g.Group_ID,
                        g.Group_Name_AR
                    })
                    .ToListAsync();

                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"حدث خطأ أثناء جلب المجموعات: {ex.Message}");
            }
        }
    }
}