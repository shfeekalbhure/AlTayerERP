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

            var groupNameAr = dto.Group_Name_AR.Trim();
            if (await _context.Tenant_Groups.AnyAsync(x => x.Group_Name_AR == groupNameAr))
            {
                return Conflict(new { message = "اسم المجموعة التجارية موجود مسبقاً." });
            }

            var newGroup = new TenantGroup
            {
                // Group_ID: معرف داخلي ثابت للمجموعة ولا يتغير بعد الإنشاء.
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

        /// <summary>
        /// تعديل اسم المجموعة أو حالة تفعيلها. لا يوجد حذف فعلي للمجموعة.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(string id, [FromBody] CreateTenantGroupDto dto)
        {
            if (string.IsNullOrWhiteSpace(id) || dto == null || string.IsNullOrWhiteSpace(dto.Group_Name_AR))
                return BadRequest(new { message = "معرف المجموعة واسمها العربي مطلوبان." });

            var group = await _context.Tenant_Groups.FirstOrDefaultAsync(x => x.Group_ID == id);
            if (group is null) return NotFound(new { message = "المجموعة التجارية غير موجودة." });

            var name = dto.Group_Name_AR.Trim();
            if (await _context.Tenant_Groups.AnyAsync(x => x.Group_ID != id && x.Group_Name_AR == name))
                return Conflict(new { message = "اسم المجموعة التجارية موجود مسبقاً." });

            // Group_Name_AR وGroup_Name_EN: أسماء العرض فقط؛ Group_ID يبقى ثابتاً.
            group.Group_Name_AR = name;
            group.Group_Name_EN = dto.Group_Name_EN?.Trim() ?? string.Empty;
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        /// <summary>
        /// إيقاف المجموعة بدلاً من حذفها، حفاظاً على الشركات والبيانات التاريخية المرتبطة.
        /// </summary>
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateGroup(string id)
        {
            var group = await _context.Tenant_Groups.FirstOrDefaultAsync(x => x.Group_ID == id);
            if (group is null) return NotFound(new { message = "المجموعة التجارية غير موجودة." });

            var hasActiveCompanies = await _context.Companies.AnyAsync(x => x.Group_ID == id && x.Is_Active);
            if (hasActiveCompanies)
                return BadRequest(new { message = "لا يمكن إيقاف مجموعة فيها شركات فعالة." });

            group.Is_Active = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إيقاف المجموعة التجارية دون حذفها." });
        }
    }
}