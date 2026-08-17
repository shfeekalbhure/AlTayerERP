using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// استثناءات صلاحيات المستخدم على مستوى الشاشة. الاستثناء SCREEN/<Screen_Code>
    /// يحل محل صلاحية الدور لتلك الشاشة، ولذلك يمكن أن يمثل منحاً أو منعاً صريحاً.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class UserPermissionOverridesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditTrailService _audit;

        public UserPermissionOverridesController(AppDbContext context, AuditTrailService audit)
        {
            _context = context;
            _audit = audit;
        }

        private bool IsAdministrator(out ServerSession session)
        {
            session = HttpContext.Items["ServerSession"] as ServerSession ?? default!;
            return session != null && session.Is_System_Admin;
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> Get(int userId)
        {
            if (!IsAdministrator(out _)) return Forbid();

            var rows = await _context.User_Permissions.AsNoTracking()
                .Where(x => x.User_ID == userId && x.Permission_Category == "SCREEN")
                .OrderBy(x => x.Permission_Name)
                .ToListAsync();
            return Ok(rows);
        }

        [HttpPut("{userId:int}/{screenCode}")]
        public async Task<IActionResult> Save(int userId, string screenCode, [FromBody] SaveUserPermissionOverrideRequest request)
        {
            if (!IsAdministrator(out var session)) return Forbid();
            if (request == null || string.IsNullOrWhiteSpace(screenCode))
                return BadRequest("بيانات الاستثناء غير صالحة.");

            var normalizedCode = screenCode.Trim();
            var userExists = await _context.Users.AnyAsync(x => x.User_ID == userId);
            var screenExists = await _context.SystemScreens.AnyAsync(x => x.Screen_Code == normalizedCode);
            if (!userExists || !screenExists) return NotFound("المستخدم أو الشاشة غير موجود.");

            var row = await _context.User_Permissions.FirstOrDefaultAsync(x =>
                x.User_ID == userId && x.Permission_Category == "SCREEN" && x.Permission_Name == normalizedCode);

            var before = row == null ? null : new
            {
                row.Can_View, row.Can_Add, row.Can_Edit, row.Can_Delete, row.Can_Print,
                row.Can_Export, row.Can_Import, row.Can_Approve, row.Can_UnApprove
            };

            if (!request.Can_View && (request.Can_Add || request.Can_Edit || request.Can_Delete ||
                request.Can_Print || request.Can_Export || request.Can_Import ||
                request.Can_Approve || request.Can_UnApprove))
                return BadRequest("لا يمكن منح عملية من دون حق العرض.");

            if (row == null)
            {
                row = new UserPermission
                {
                    User_ID = userId,
                    Permission_Category = "SCREEN",
                    Permission_Name = normalizedCode
                };
                _context.User_Permissions.Add(row);
            }

            row.Can_View = request.Can_View;
            row.Can_Add = request.Can_Add;
            row.Can_Edit = request.Can_Edit;
            row.Can_Delete = request.Can_Delete;
            row.Can_Print = request.Can_Print;
            row.Can_Export = request.Can_Export;
            row.Can_Import = request.Can_Import;
            row.Can_Approve = request.Can_Approve;
            row.Can_UnApprove = request.Can_UnApprove;

            _audit.Add(session, HttpContext, "user_permissions", $"{userId}:SCREEN:{normalizedCode}",
                "OVERRIDE_SAVE", before, new
                {
                    row.Can_View, row.Can_Add, row.Can_Edit, row.Can_Delete, row.Can_Print,
                    row.Can_Export, row.Can_Import, row.Can_Approve, row.Can_UnApprove
                });
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حفظ استثناء المستخدم." });
        }

        [HttpDelete("{userId:int}/{screenCode}")]
        public async Task<IActionResult> Delete(int userId, string screenCode)
        {
            if (!IsAdministrator(out var session)) return Forbid();

            var row = await _context.User_Permissions.FirstOrDefaultAsync(x =>
                x.User_ID == userId && x.Permission_Category == "SCREEN" && x.Permission_Name == screenCode.Trim());
            if (row == null) return NotFound("استثناء المستخدم غير موجود.");

            _context.User_Permissions.Remove(row);
            _audit.Add(session, HttpContext, "user_permissions", $"{userId}:SCREEN:{screenCode.Trim()}",
                "OVERRIDE_DELETE");
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حذف استثناء المستخدم والعودة إلى صلاحيات الدور." });
        }
    }

    public sealed class SaveUserPermissionOverrideRequest
    {
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
