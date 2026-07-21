using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Core.Entities.Configuration;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة الاستثناءات الدقيقة للمستخدم فوق صلاحيات دوره.
    /// </summary>
    [Authorize(Roles = "SystemAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserResourcePermissionsController : ControllerBase
    {
        private const string Inherit = "INHERIT";
        private const string Allow = "ALLOW";
        private const string Deny = "DENY";
        private readonly AppDbContext _context;

        public UserResourcePermissionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.AsNoTracking()
                .Where(x => x.Is_Active)
                .OrderBy(x => x.Full_Name)
                .Select(x => new UserPermissionUserDto
                {
                    User_ID = x.User_ID,
                    Full_Name = x.Full_Name,
                    Login_Name = x.Login_Name
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{userId:int}/resource-permissions/{screenId:int}")]
        public async Task<IActionResult> GetPermissions(int userId, int screenId)
        {
            bool userExists = await _context.Users.AsNoTracking()
                .AnyAsync(x => x.User_ID == userId && x.Is_Active);
            if (!userExists)
                return NotFound("المستخدم غير موجود أو غير نشط.");

            var screen = await _context.SystemScreens.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Screen_ID == screenId && x.Is_Active);
            if (screen == null)
                return NotFound("الشاشة غير موجودة أو غير نشطة.");

            string fieldType = "FIELD:" + screenId;
            string actionType = "ACTION:" + screenId;

            var permissions = await _context.User_Resource_Permissions.AsNoTracking()
                .Where(x => x.User_ID == userId &&
                            x.Is_Active &&
                            (x.Resource_Type == fieldType || x.Resource_Type == actionType) &&
                            (x.Effective_To == null || x.Effective_To >= DateTime.UtcNow))
                .ToListAsync();

            var effects = permissions.ToDictionary(
                x => x.Resource_Type + "|" + x.Resource_Code + "|" + x.Permission_Code,
                x => x.Effect,
                StringComparer.OrdinalIgnoreCase);

            var fields = await _context.System_Screen_Fields.AsNoTracking()
                .Where(x => x.Screen_ID == screenId && x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .Select(x => new UserResourcePermissionItemDto
                {
                    Resource_Type = fieldType,
                    Resource_Code = x.Field_Code,
                    Resource_Name = x.Field_Name,
                    View_Mode = Inherit,
                    Edit_Mode = Inherit
                })
                .ToListAsync();

            foreach (var field in fields)
            {
                field.View_Mode = GetMode(effects, fieldType, field.Resource_Code, "VIEW");
                field.Edit_Mode = GetMode(effects, fieldType, field.Resource_Code, "EDIT");
            }

            var actions = await _context.System_Screen_Actions.AsNoTracking()
                .Where(x => x.Screen_ID == screenId && x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .Select(x => new UserResourcePermissionItemDto
                {
                    Resource_Type = actionType,
                    Resource_Code = x.Action_Code,
                    Resource_Name = x.Action_Name,
                    Execute_Mode = Inherit
                })
                .ToListAsync();

            foreach (var action in actions)
                action.Execute_Mode = GetMode(effects, actionType, action.Resource_Code, "EXECUTE");

            return Ok(new UserResourcePermissionResponseDto
            {
                Screen_ID = screenId,
                Screen_Code = screen.Screen_Code,
                Fields = fields,
                Actions = actions
            });
        }

        [HttpPut("{userId:int}/resource-permissions/{screenId:int}")]
        public async Task<IActionResult> SavePermissions(
            int userId,
            int screenId,
            [FromBody] UserResourcePermissionResponseDto request)
        {
            if (request == null)
                return BadRequest("بيانات الصلاحيات مطلوبة.");

            List<UserResourcePermissionItemDto> fields = request.Fields ?? new List<UserResourcePermissionItemDto>();
            List<UserResourcePermissionItemDto> actions = request.Actions ?? new List<UserResourcePermissionItemDto>();

            bool userExists = await _context.Users.AsNoTracking()
                .AnyAsync(x => x.User_ID == userId && x.Is_Active);
            if (!userExists)
                return NotFound("المستخدم غير موجود أو غير نشط.");

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

            if (fields.Any(x => !validFields.Contains(x.Resource_Code)) ||
                actions.Any(x => !validActions.Contains(x.Resource_Code)))
            {
                return BadRequest("توجد موارد لا تنتمي إلى الشاشة المختارة.");
            }

            var old = await _context.User_Resource_Permissions
                .Where(x => x.User_ID == userId &&
                            (x.Resource_Type == fieldType || x.Resource_Type == actionType))
                .ToListAsync();
            _context.User_Resource_Permissions.RemoveRange(old);

            foreach (var field in fields)
            {
                AddPermission(userId, fieldType, field.Resource_Code, "VIEW", field.View_Mode);
                AddPermission(userId, fieldType, field.Resource_Code, "EDIT", field.Edit_Mode);
            }

            foreach (var action in actions)
                AddPermission(userId, actionType, action.Resource_Code, "EXECUTE", action.Execute_Mode);

            // يوثق التغيير دون حفظ تفاصيل حساسة غير لازمة في سجل التدقيق.
            _context.Audit_Logs.Add(new AuditLog
            {
                Table_Name = "user_resource_permissions",
                Record_ID = userId + ":" + screenId,
                Action_Type = "UPDATE",
                User_ID = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Branch_ID = User.FindFirstValue("branch_id"),
                Action_At = DateTime.UtcNow,
                Old_Values = JsonSerializer.Serialize(new { Count = old.Count }),
                New_Values = JsonSerializer.Serialize(new
                {
                    Field_Overrides = fields.Count(x => !string.Equals(x.View_Mode, Inherit, StringComparison.OrdinalIgnoreCase) ||
                                                        !string.Equals(x.Edit_Mode, Inherit, StringComparison.OrdinalIgnoreCase)),
                    Action_Overrides = actions.Count(x => !string.Equals(x.Execute_Mode, Inherit, StringComparison.OrdinalIgnoreCase))
                }),
                Action_Channel = "DESKTOP",
                Device_Name = Request.Headers["User-Agent"].ToString(),
                IP_Address = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Notes = "تم تعديل استثناءات صلاحيات المستخدم."
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حفظ استثناءات المستخدم بنجاح." });
        }

        private static string GetMode(
            IReadOnlyDictionary<string, bool> effects,
            string type,
            string code,
            string permission)
        {
            if (!effects.TryGetValue(type + "|" + code + "|" + permission, out bool effect))
                return Inherit;

            return effect ? Allow : Deny;
        }

        private void AddPermission(
            int userId,
            string type,
            string code,
            string permission,
            string? mode)
        {
            string normalizedMode = (mode ?? Inherit).Trim().ToUpperInvariant();
            if (normalizedMode == Inherit)
                return;

            if (normalizedMode != Allow && normalizedMode != Deny)
                throw new InvalidOperationException("نمط الصلاحية غير معتمد.");

            _context.User_Resource_Permissions.Add(new UserResourcePermission
            {
                User_ID = userId,
                Resource_Type = type,
                Resource_Code = code,
                Permission_Code = permission,
                Effect = normalizedMode == Allow,
                Is_Active = true
            });
        }
    }

    public class UserPermissionUserDto
    {
        public int User_ID { get; set; }
        public string Full_Name { get; set; } = string.Empty;
        public string Login_Name { get; set; } = string.Empty;
        public string Display_Name => Full_Name + " - " + Login_Name;
    }

    public class UserResourcePermissionResponseDto
    {
        public int Screen_ID { get; set; }
        public string Screen_Code { get; set; } = string.Empty;
        public List<UserResourcePermissionItemDto>? Fields { get; set; } = new();
        public List<UserResourcePermissionItemDto>? Actions { get; set; } = new();
    }

    public class UserResourcePermissionItemDto
    {
        public string Resource_Type { get; set; } = string.Empty;
        public string Resource_Code { get; set; } = string.Empty;
        public string Resource_Name { get; set; } = string.Empty;
        public string View_Mode { get; set; } = "INHERIT";
        public string Edit_Mode { get; set; } = "INHERIT";
        public string Execute_Mode { get; set; } = "INHERIT";
    }
}