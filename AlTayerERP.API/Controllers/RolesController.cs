using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. دالة جلب جميع الأدوار
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .OrderBy(x => x.Role_ID)
                .ToListAsync();

            return Ok(roles);
        }

        // 2. [التعديل الجديد]: دالة إضافة دور جديد مع التوليد التلقائي للكود
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Role_Name))
                return BadRequest("اسم الدور مطلوب.");

            var role = new Role
            {
                // فحص كود الدور: إذا كان فارغاً، يتم توليد كود تلقائي يبدأ بـ ROL متبوعاً بالوقت الحالي بدقة الثانية
                Role_Code = string.IsNullOrWhiteSpace(dto.Role_Code)
                    ? "ROL" + DateTime.Now.ToString("yyyyMMddHHmmss")
                    : dto.Role_Code.Trim(),

                Role_Name = dto.Role_Name.Trim(),
                Description = dto.Description,
                Is_Active = dto.Is_Active,
                Created_At = DateTime.Now
            };

            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();

            return Ok(role);
        }

        // 3. دالة جلب دور محدد بواسطة الرقم
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Role_ID == id);

            if (role == null)
                return NotFound("الدور غير موجود.");

            return Ok(role);
        }

        // 4. دالة تعديل بيانات دور
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Role_ID == id);

            if (role == null)
                return NotFound("الدور غير موجود.");

            role.Role_Code = dto.Role_Code?.Trim();
            role.Role_Name = dto.Role_Name.Trim();
            role.Description = dto.Description;
            role.Is_Active = dto.Is_Active;
            role.Updated_At = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(role);
        }

        // 5. دالة حذف دور
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Role_ID == id);

            if (role == null)
                return NotFound("الدور غير موجود.");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok("تم حذف الدور بنجاح.");
        }
    }
}