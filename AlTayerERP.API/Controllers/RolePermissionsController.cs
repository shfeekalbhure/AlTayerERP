using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة صلاحيات الأدوار. تعديل الصلاحيات محصور بمدير النظام لأن هذه الشاشة
    /// تتحكم بوصول بقية النظام، بينما مصدر الجلسة هو AuthenticationHandler.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditTrailService _audit;

        public RolePermissionsController(AppDbContext context, AuditTrailService audit)
        {
            _context = context;
            _audit = audit;
        }

        private bool TryGetSession(out ServerSession session)
        {
            session = HttpContext.Items["ServerSession"] as ServerSession ?? default!;
            return session != null;
        }

        private bool IsAdministrator(out ServerSession session) =>
            TryGetSession(out session) && session.Is_System_Admin;

        [HttpGet("GetScreens")]
        public async Task<IActionResult> GetScreens()
        {
            if (!IsAdministrator(out _))
                return Forbid();

            var screens = await _context.SystemScreens.AsNoTracking()
                .Where(x => x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .Select(x => new { x.Screen_ID, x.Screen_Code, x.Screen_Name, x.Module_Name })
                .ToListAsync();

            return Ok(screens);
        }

        [HttpGet("GetRolePermissions/{roleId:int}")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            if (!IsAdministrator(out _))
                return Forbid();

            var permissions = await _context.RolePermissions.AsNoTracking()
                .Where(x => x.Role_ID == roleId)
                .ToListAsync();

            return Ok(permissions);
        }

        /// <summary>
        /// يستبدل مصفوفة الدور داخل Transaction واحدة، ويسجل ملخصاً فقط دون بيانات حساسة.
        /// </summary>
        [HttpPost("SaveRolePermissions")]
        public async Task<IActionResult> SaveRolePermissions([FromBody] List<SaveRolePermissionDto> permissions)
        {
            if (!IsAdministrator(out var session))
                return Forbid();

            if (permissions == null || permissions.Count == 0)
                return BadRequest("لا توجد صلاحيات للحفظ.");

            var roleId = permissions.First().Role_ID;
            if (roleId <= 0 || permissions.Any(x => x.Role_ID != roleId || x.Screen_ID <= 0))
                return BadRequest("بيانات الصلاحيات غير صالحة أو تخص أكثر من دور.");

            if (permissions.Any(x => !x.Can_View &&
                (x.Can_Add || x.Can_Edit || x.Can_Delete || x.Can_Print || x.Can_Export ||
                 x.Can_Import || x.Can_Approve || x.Can_UnApprove)))
            {
                return BadRequest("لا يمكن منح عملية على شاشة ليس لها حق العرض.");
            }

            var roleExists = await _context.Roles.AnyAsync(x => x.Role_ID == roleId && x.Is_Active);
            if (!roleExists)
                return BadRequest("الدور المحدد غير موجود أو غير فعال.");

            var requestedScreenIds = permissions.Select(x => x.Screen_ID).Distinct().ToList();
            var activeScreenIds = await _context.SystemScreens.AsNoTracking()
                .Where(x => x.Is_Active && requestedScreenIds.Contains(x.Screen_ID))
                .Select(x => x.Screen_ID)
                .ToListAsync();
            if (activeScreenIds.Count != requestedScreenIds.Count)
                return BadRequest("تحتوي العملية على شاشة غير موجودة أو غير فعالة.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldRows = await _context.RolePermissions.Where(x => x.Role_ID == roleId).ToListAsync();
                _context.RolePermissions.RemoveRange(oldRows);

                var newRows = permissions.Select(x => new RolePermission
                {
                    Role_ID = roleId,
                    Screen_ID = x.Screen_ID,
                    Can_View = x.Can_View,
                    Can_Add = x.Can_Add,
                    Can_Edit = x.Can_Edit,
                    Can_Delete = x.Can_Delete,
                    Can_Print = x.Can_Print,
                    Can_Export = x.Can_Export,
                    Can_Import = x.Can_Import,
                    Can_Approve = x.Can_Approve,
                    Can_UnApprove = x.Can_UnApprove
                }).ToList();

                await _context.RolePermissions.AddRangeAsync(newRows);
                _audit.Add(session, HttpContext, "role_permissions", roleId.ToString(), "UPDATE",
                    new { Count = oldRows.Count },
                    new { Count = newRows.Count, GrantedView = newRows.Count(x => x.Can_View) },
                    "استبدال مصفوفة صلاحيات الدور");
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "تم حفظ الصلاحيات بنجاح." });
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "تعذر حفظ الصلاحيات حالياً.");
            }
        }
    }
}
