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

        [HttpGet("{id:int}/resource-permissions/{screenId:int}")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> GetResourcePermissions(int id, int screenId)
        {
            bool roleExists = await _context.Roles.AsNoTracking()
                .AnyAsync(x => x.Role_ID == id);
            if (!roleExists)
                return NotFound("الدور غير موجود.");

            var screen = await _context.SystemScreens.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Screen_ID == screenId && x.Is_Active);
            if (screen == null)
                return NotFound("الشاشة غير موجودة أو غير نشطة.");

            string fieldType = "FIELD:" + screenId;
            string actionType = "ACTION:" + screenId;
            var assigned = await _context.Role_Resource_Permissions.AsNoTracking()
                .Where(x => x.Role_ID == id &&
                            (x.Resource_Type == fieldType || x.Resource_Type == actionType) &&
                            x.Is_Active && x.Effect)
                .ToListAsync();

            var grants = assigned
                .Select(x => x.Resource_Type + "|" + x.Resource_Code + "|" + x.Permission_Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var fields = await _context.System_Screen_Fields.AsNoTracking()
                .Where(x => x.Screen_ID == screenId && x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .Select(x => new ResourcePermissionDto
                {
                    Resource_Type = fieldType,
                    Resource_Code = x.Field_Code,
                    Resource_Name = x.Field_Name,
                    Permission_View = false,
                    Permission_Edit = false
                })
                .ToListAsync();

            foreach (ResourcePermissionDto item in fields)
            {
                item.Permission_View = grants.Contains(fieldType + "|" + item.Resource_Code + "|VIEW");
                item.Permission_Edit = grants.Contains(fieldType + "|" + item.Resource_Code + "|EDIT");
            }

            var actions = await _context.System_Screen_Actions.AsNoTracking()
                .Where(x => x.Screen_ID == screenId && x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .Select(x => new ResourcePermissionDto
                {
                    Resource_Type = actionType,
                    Resource_Code = x.Action_Code,
                    Resource_Name = x.Action_Name,
                    Permission_Execute = false
                })
                .ToListAsync();

            foreach (ResourcePermissionDto item in actions)
                item.Permission_Execute = grants.Contains(actionType + "|" + item.Resource_Code + "|EXECUTE");

            return Ok(new ResourcePermissionResponseDto
            {
                Screen_ID = screenId,
                Screen_Code = screen.Screen_Code,
                Fields = fields,
                Actions = actions
            });
        }

        [HttpPut("{id:int}/resource-permissions/{screenId:int}")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> SaveResourcePermissions(
            int id,
            int screenId,
            [FromBody] ResourcePermissionResponseDto request)
        {
            if (request == null)
                return BadRequest("بيانات الصلاحيات الدقيقة مطلوبة.");

            bool roleExists = await _context.Roles.AsNoTracking()
                .AnyAsync(x => x.Role_ID == id);
            if (!roleExists)
                return NotFound("الدور غير موجود.");

            bool screenExists = await _context.SystemScreens.AsNoTracking()
                .AnyAsync(x => x.Screen_ID == screenId && x.Is_Active);
            if (!screenExists)
                return NotFound("الشاشة غير موجودة أو غير نشطة.");

            string fieldType = "FIELD:" + screenId;
            string actionType = "ACTION:" + screenId;
            var validFields = await _context.System_Screen_Fields.AsNoTracking()
                .Where(x => x.Screen_ID == screenId && x.Is_Active)
                .Select(x => x.Field_Code)
                .ToListAsync();
            var validActions = await _context.System_Screen_Actions.AsNoTracking()
                .Where(x => x.Screen_ID == screenId && x.Is_Active)
                .Select(x => x.Action_Code)
                .ToListAsync();

            if (request.Fields.Any(x => !validFields.Contains(x.Resource_Code)) ||
                request.Actions.Any(x => !validActions.Contains(x.Resource_Code)))
            {
                return BadRequest("توجد موارد لا تنتمي إلى الشاشة المختارة.");
            }

            var old = await _context.Role_Resource_Permissions
                .Where(x => x.Role_ID == id &&
                            (x.Resource_Type == fieldType || x.Resource_Type == actionType))
                .ToListAsync();
            _context.Role_Resource_Permissions.RemoveRange(old);

            foreach (ResourcePermissionDto field in request.Fields)
            {
                AddGrant(id, fieldType, field.Resource_Code, "VIEW", field.Permission_View);
                AddGrant(id, fieldType, field.Resource_Code, "EDIT", field.Permission_Edit);
            }

            foreach (ResourcePermissionDto action in request.Actions)
                AddGrant(id, actionType, action.Resource_Code, "EXECUTE", action.Permission_Execute);

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حفظ صلاحيات الحقول والأزرار بنجاح." });
        }

        private void AddGrant(int roleId, string resourceType, string resourceCode, string permissionCode, bool granted)
        {
            if (!granted)
                return;

            _context.Role_Resource_Permissions.Add(new AlTayerERP.Core.Entities.Configuration.RoleResourcePermission
            {
                Role_ID = roleId,
                Resource_Type = resourceType,
                Resource_Code = resourceCode,
                Permission_Code = permissionCode,
                Effect = true,
                Is_Active = true
            });
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

    public class ResourcePermissionResponseDto
    {
        public int Screen_ID { get; set; }
        public string Screen_Code { get; set; } = string.Empty;
        public List<ResourcePermissionDto> Fields { get; set; } = new();
        public List<ResourcePermissionDto> Actions { get; set; } = new();
    }

    public class ResourcePermissionDto
    {
        public string Resource_Type { get; set; } = string.Empty;
        public string Resource_Code { get; set; } = string.Empty;
        public string Resource_Name { get; set; } = string.Empty;
        public bool Permission_View { get; set; }
        public bool Permission_Edit { get; set; }
        public bool Permission_Execute { get; set; }
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