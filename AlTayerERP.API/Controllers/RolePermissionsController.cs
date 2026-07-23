using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة صلاحيات الأدوار. كل الطلبات هنا تعتمد على جلسة أصدرها الخادم بعد الدخول.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionsController : ControllerBase
    {
        private const string SessionHeader = "X-Session-Token";
        private readonly AppDbContext _context;
        private readonly ServerSessionService _sessions;

        public RolePermissionsController(AppDbContext context, ServerSessionService sessions)
        {
            _context = context;
            _sessions = sessions;
        }

        // استخراج الجلسة من الترويسة؛ لا نثق بمُعرّف الدور القادم من العميل.
        private bool TryGetSession(out ServerSession session) =>
            _sessions.TryGet(Request.Headers[SessionHeader].ToString(), out session);

        [HttpGet("GetScreens")]
        public async Task<IActionResult> GetScreens()
        {
            if (!TryGetSession(out var session))
                return Unauthorized("انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد.");

            // كتالوج إدارة الصلاحيات لا يفتح إلا لمدير النظام.
            if (!session.Is_System_Admin)
                return Forbid();

            var screens = await _context.SystemScreens
                .AsNoTracking()
                .Where(x => x.Is_Active)
                .OrderBy(x => x.Sort_Order)
                .Select(x => new
                {
                    x.Screen_ID,
                    x.Screen_Code,
                    x.Screen_Name,
                    x.Module_Name
                })
                .ToListAsync();

            return Ok(screens);
        }

        [HttpGet("GetRolePermissions/{roleId:int}")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            if (!TryGetSession(out var session))
                return Unauthorized("انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد.");

            // المستخدم العادي يقرأ صلاحيات دوره فقط؛ مدير النظام يستطيع إدارتها كلها.
            if (!session.Is_System_Admin && session.Role_ID != roleId)
                return Forbid();

            var permissions = await _context.RolePermissions
                .AsNoTracking()
                .Where(x => x.Role_ID == roleId)
                .ToListAsync();

            return Ok(permissions);
        }

        [HttpPost("SaveRolePermissions")]
        public async Task<IActionResult> SaveRolePermissions([FromBody] List<SaveRolePermissionDto> permissions)
        {
            if (!TryGetSession(out var session))
                return Unauthorized("انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد.");

            if (!session.Is_System_Admin)
                return Forbid();

            if (permissions == null || permissions.Count == 0)
                return BadRequest("لا توجد صلاحيات للحفظ.");

            var roleId = permissions.First().Role_ID;
            if (roleId <= 0 || permissions.Any(x => x.Role_ID != roleId || x.Screen_ID <= 0))
                return BadRequest("بيانات الصلاحيات غير صالحة أو تخص أكثر من دور.");

            // منع الصلاحيات المتناقضة: لا توجد عملية إضافة/تعديل/حذف دون حق العرض للشاشة.
            if (permissions.Any(x => !x.Can_View &&
                (x.Can_Add || x.Can_Edit || x.Can_Delete || x.Can_Print ||
                 x.Can_Export || x.Can_Import || x.Can_Approve || x.Can_UnApprove)))
            {
                return BadRequest("لا يمكن منح عملية على شاشة ليس لها حق العرض.");
            }

            var roleExists = await _context.Roles.AnyAsync(x => x.Role_ID == roleId && x.Is_Active);
            if (!roleExists)
                return BadRequest("الدور المحدد غير موجود أو غير فعال.");

            var requestedScreenIds = permissions.Select(x => x.Screen_ID).Distinct().ToList();
            var activeScreens = await _context.SystemScreens
                .Where(x => x.Is_Active && requestedScreenIds.Contains(x.Screen_ID))
                .Select(x => x.Screen_ID)
                .ToListAsync();

            if (activeScreens.Count != requestedScreenIds.Count)
                return BadRequest("تحتوي العملية على شاشة غير موجودة أو غير فعالة.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldPermissions = await _context.RolePermissions
                    .Where(x => x.Role_ID == roleId)
                    .ToListAsync();

                _context.RolePermissions.RemoveRange(oldPermissions);

                var rows = permissions.Select(item => new RolePermission
                {
                    Role_ID = roleId,
                    Screen_ID = item.Screen_ID,
                    Can_View = item.Can_View,
                    Can_Add = item.Can_Add,
                    Can_Edit = item.Can_Edit,
                    Can_Delete = item.Can_Delete,
                    Can_Print = item.Can_Print,
                    Can_Export = item.Can_Export,
                    Can_Import = item.Can_Import,
                    Can_Approve = item.Can_Approve,
                    Can_UnApprove = item.Can_UnApprove
                }).ToList();

                await _context.RolePermissions.AddRangeAsync(rows);
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
