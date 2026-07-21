using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemScreensController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemScreensController(AppDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // جلب شاشات النظام المسموح بها للدور الحالي
        // ======================================================
        [HttpGet]
        public async Task<IActionResult> GetScreens()
        {
            var session = HttpContext.Items["ServerSession"] as ServerSession;
            if (session == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد."
                });
            }

            var screensQuery = _context.SystemScreens
                .AsNoTracking()
                .Where(screen => screen.Is_Active);

            // مدير النظام يرى جميع الشاشات الفعالة؛ أما بقية المستخدمين فيرون الشاشات
            // التي يحمل دورهم حق عرضها فقط.
            if (!session.Is_System_Admin)
            {
                screensQuery =
                    from screen in screensQuery
                    join permission in _context.RolePermissions.AsNoTracking()
                        on screen.Screen_ID equals permission.Screen_ID
                    where permission.Role_ID == session.Role_ID && permission.Can_View
                    select screen;
            }

            var screens = await screensQuery
                .OrderBy(screen => screen.Sort_Order)
                .ToListAsync();

            return Ok(screens);
        }
    }
}