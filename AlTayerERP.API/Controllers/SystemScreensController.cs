using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// كتالوج شاشات النظام. يحدد الشاشات التي يمكن منحها للأدوار
    /// ويشكل المصدر المرئي لشجرة النظام.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SystemScreensController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ScreenAuthorizationService _authorization;
        private readonly AuditTrailService _audit;

        public SystemScreensController(AppDbContext context, ScreenAuthorizationService authorization, AuditTrailService audit)
        {
            _context = context;
            _authorization = authorization;
            _audit = audit;
        }

        private ServerSession? GetSession() =>
            HttpContext.Items["ServerSession"] as ServerSession;

        private IActionResult? RequireSystemAdmin()
        {
            var session = GetSession();
            if (session == null)
                return Unauthorized(new { success = false, message = "انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد." });

            if (!session.Is_System_Admin)
                return Forbid();

            return null;
        }

        /// <summary>
        /// يعيد الشاشات المسموحة للدور الحالي. مدير النظام يرى أيضاً الشاشات
        /// الموقفة حتى يستطيع إدارتها، أما بقية المستخدمين فلا يرون إلا النشطة المصرح بها.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetScreens()
        {
            var session = GetSession();
            if (session == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "انتهت الجلسة أو أنها غير صالحة. سجل الدخول من جديد."
                });
            }

            var candidates = await _context.SystemScreens.AsNoTracking()
                .Where(x => session.Is_System_Admin || x.Is_Active)
                .OrderBy(screen => screen.Module_Name)
                .ThenBy(screen => screen.Sort_Order)
                .ThenBy(screen => screen.Screen_Name)
                .ToListAsync();

            // تمر كل شاشة عبر محرك التفويض نفسه حتى تنعكس استثناءات المستخدم
            // في القائمة، لا في الـ API فقط.
            if (!session.Is_System_Admin)
            {
                var allowed = new List<SystemScreen>();
                foreach (var screen in candidates)
                {
                    if (await _authorization.IsAllowedAsync(session, screen.Screen_Code, ScreenOperation.View))
                        allowed.Add(screen);
                }
                candidates = allowed;
            }

            return Ok(candidates);
        }

        /// <summary>
        /// إضافة أو تعديل شاشة في الكتالوج. لا يسمح بالحذف الفعلي لأن
        /// صلاحيات الأدوار تعتمد على معرّف الشاشة؛ الإيقاف هو البديل الآمن.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveSystemScreenRequest request)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null)
                return accessError;

            if (request == null ||
                string.IsNullOrWhiteSpace(request.Screen_Code) ||
                string.IsNullOrWhiteSpace(request.Screen_Name) ||
                string.IsNullOrWhiteSpace(request.Module_Name))
            {
                return BadRequest(new { message = "كود الشاشة واسمها والوحدة مطلوبة." });
            }

            var code = request.Screen_Code.Trim();
            var name = request.Screen_Name.Trim();
            var module = request.Module_Name.Trim();

            if (code.Length > 100 || name.Length > 200 || module.Length > 150)
                return BadRequest(new { message = "أحد الحقول تجاوز الحد المسموح به." });

            var duplicate = await _context.SystemScreens.AnyAsync(x =>
                x.Screen_Code == code && x.Screen_ID != request.Screen_ID);
            if (duplicate)
                return BadRequest(new { message = "كود الشاشة مستخدم مسبقاً. استخدم كوداً فريداً." });

            SystemScreen screen;
            if (request.Screen_ID > 0)
            {
                var existingScreen = await _context.SystemScreens
                    .FirstOrDefaultAsync(x => x.Screen_ID == request.Screen_ID);
                if (existingScreen == null)
                    return NotFound(new { message = "الشاشة المطلوب تعديلها غير موجودة." });

                screen = existingScreen;
            }
            else
            {
                screen = new SystemScreen { Created_At = DateTime.Now };
                _context.SystemScreens.Add(screen);
            }

            screen.Screen_Code = code;
            screen.Screen_Name = name;
            screen.Module_Name = module;
            screen.Sort_Order = request.Sort_Order;
            screen.Is_Active = request.Is_Active;

            var session = GetSession()!;
            _audit.Add(session, HttpContext, "system_screens", request.Screen_ID > 0 ? screen.Screen_ID.ToString() : "new",
                request.Screen_ID > 0 ? "UPDATE" : "CREATE",
                newValues: new { screen.Screen_Code, screen.Screen_Name, screen.Module_Name, screen.Sort_Order, screen.Is_Active });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = request.Screen_ID > 0 ? "تم تعديل شاشة النظام." : "تمت إضافة شاشة النظام.",
                screen.Screen_ID
            });
        }

        /// <summary>
        /// إيقاف الشاشة بدلاً من حذفها، مع منع إيقاف شاشة ما زالت مفعلة
        /// في صلاحيات الأدوار حتى لا تنقطع شجرة النظام دون قرار إداري واضح.
        /// </summary>
        [HttpPost("{screenId:int}/Deactivate")]
        public async Task<IActionResult> Deactivate(int screenId)
        {
            var accessError = RequireSystemAdmin();
            if (accessError != null)
                return accessError;

            var screen = await _context.SystemScreens.FindAsync(screenId);
            if (screen == null)
                return NotFound(new { message = "الشاشة غير موجودة." });

            var usedByRole = await _context.RolePermissions
                .AnyAsync(x => x.Screen_ID == screenId && x.Can_View);
            if (usedByRole)
            {
                return BadRequest(new
                {
                    message = "لا يمكن إيقاف الشاشة لأنها ما زالت ممنوحة لأحد الأدوار. أزل صلاحياتها أولاً."
                });
            }

            screen.Is_Active = false;
            _audit.Add(GetSession()!, HttpContext, "system_screens", screenId.ToString(), "DEACTIVATE");
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إيقاف الشاشة." });
        }
    }

    /// <summary>
    /// عقد حفظ كتالوج الشاشة؛ لا يسمح للواجهة بإرسال خصائص تدقيقية أو صلاحيات مباشرة.
    /// </summary>
    public sealed class SaveSystemScreenRequest
    {
        public int Screen_ID { get; set; }
        public string Screen_Code { get; set; } = string.Empty;
        public string Screen_Name { get; set; } = string.Empty;
        public string Module_Name { get; set; } = string.Empty;
        public int Sort_Order { get; set; }
        public bool Is_Active { get; set; } = true;
    }
}
