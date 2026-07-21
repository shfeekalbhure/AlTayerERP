using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.AsNoTracking()
                .OrderBy(x => x.Role_ID)
                .ToListAsync();
            return Ok(roles);
        }

        [HttpGet("{id:int}/permissions")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> GetPermissions(int id)
        {
            bool roleExists = await _context.Roles.AsNoTracking()
                .AnyAsync(x => x.Role_ID == id);
            if (!roleExists)
                return NotFound("الدور غير موجود.");

            var assigned = await _context.RolePermissions.AsNoTracking()
                .Where(x => x.Role_ID == id)
                .ToDictionaryAsync(x => x.Screen_ID);

            var result = await _context.SystemScreens.AsNoTracking()
                .Where(x => x.Is_Active)
                .OrderBy(x => x.Module_Name)
                .ThenBy(x => x.Sort_Order)
                .Select(x => new RoleScreenPermissionDto
                {
                    Screen_ID = x.Screen_ID,
                    Screen_Code = x.Screen_Code,
                    Screen_Name = x.Screen_Name,
                    Module_Name = x.Module_Name
                })
                .ToListAsync();

            foreach (RoleScreenPermissionDto item in result)
            {
                if (!assigned.TryGetValue(item.Screen_ID, out RolePermission? permission))
                    continue;

                item.Can_View = permission.Can_View;
                item.Can_Add = permission.Can_Add;
                item.Can_Edit = permission.Can_Edit;
                item.Can_Delete = permission.Can_Delete;
                item.Can_Print = permission.Can_Print;
                item.Can_Export = permission.Can_Export;
                item.Can_Import = permission.Can_Import;
                item.Can_Approve = permission.Can_Approve;
                item.Can_UnApprove = permission.Can_UnApprove;
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Role_Name))
                return BadRequest("اسم الدور مطلوب.");

            var role = new Role
            {
                Role_Code = string.IsNullOrWhiteSpace(dto.Role_Code)
                    ? "ROL" + DateTime.UtcNow.ToString("yyyyMMddHHmmss")
                    : dto.Role_Code.Trim(),
                Role_Name = dto.Role_Name.Trim(),
                Description = dto.Description,
                Is_Active = dto.Is_Active,
                Created_At = DateTime.UtcNow
            };

            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            return Ok(role);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Role_Name))
                return BadRequest("اسم الدور مطلوب.");

            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Role_ID == id);
            if (role == null)
                return NotFound("الدور غير موجود.");

            role.Role_Code = dto.Role_Code?.Trim();
            role.Role_Name = dto.Role_Name.Trim();
            role.Description = dto.Description;
            role.Is_Active = dto.Is_Active;
            role.Updated_At = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(role);
        }

        [HttpPut("{id:int}/permissions")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> SavePermissions(
            int id,
            [FromBody] List<RoleScreenPermissionDto> request)
        {
            if (request == null)
                return BadRequest("بيانات الصلاحيات مطلوبة.");

            bool roleExists = await _context.Roles.AsNoTracking()
                .AnyAsync(x => x.Role_ID == id);
            if (!roleExists)
                return NotFound("الدور غير موجود.");

            var screenIds = request.Select(x => x.Screen_ID).Distinct().ToList();
            int validCount = await _context.SystemScreens.AsNoTracking()
                .CountAsync(x => screenIds.Contains(x.Screen_ID) && x.Is_Active);
            if (validCount != screenIds.Count)
                return BadRequest("توجد شاشة غير صالحة ضمن طلب الصلاحيات.");

            var existing = await _context.RolePermissions
                .Where(x => x.Role_ID == id)
                .ToDictionaryAsync(x => x.Screen_ID);

            foreach (RoleScreenPermissionDto input in request)
            {
                if (!existing.TryGetValue(input.Screen_ID, out RolePermission? permission))
                {
                    permission = new RolePermission
                    {
                        Role_ID = id,
                        Screen_ID = input.Screen_ID
                    };
                    _context.RolePermissions.Add(permission);
                }

                permission.Can_View = input.Can_View;
                permission.Can_Add = input.Can_Add;
                permission.Can_Edit = input.Can_Edit;
                permission.Can_Delete = input.Can_Delete;
                permission.Can_Print = input.Can_Print;
                permission.Can_Export = input.Can_Export;
                permission.Can_Import = input.Can_Import;
                permission.Can_Approve = input.Can_Approve;
                permission.Can_UnApprove = input.Can_UnApprove;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حفظ صلاحيات الدور بنجاح." });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Role_ID == id);
            if (role == null)
                return NotFound("الدور غير موجود.");

            if (role.Is_System_Admin)
                return BadRequest("لا يمكن حذف دور مدير النظام.");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return Ok("تم حذف الدور بنجاح.");
        }
    }

    public class RoleScreenPermissionDto
    {
        public int Screen_ID { get; set; }
        public string Screen_Code { get; set; } = string.Empty;
        public string Screen_Name { get; set; } = string.Empty;
        public string Module_Name { get; set; } = string.Empty;
        public bool Can_View { get; set; }
        public bool Can_Add { get; set; }
        public bool Can_Edit { get; set; }
        public bool Can_Delete { get; set; }
        public bool Can_Print { get; set; }
        public bool Can_Export { get; set; }
        public bool Can_Import { get; set; }
        public bool Can_Approve { get; set; }
        public bool Can_UnApprove { get; set; }
    }
}