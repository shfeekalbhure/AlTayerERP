using AlTayerERP.Infrastructure.Data;
using AlTayerERP.Core.Entities; // جلب موديل الـ User الفعلي
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (request == null)
                return BadRequest("بيانات الطلب غير مكتملة.");

            try
            {
                // 1. التحقق من معرّف المستخدم ونشاطه أولاً
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.User_ID == request.User_ID && x.Is_Active);

                if (user == null)
                    return Unauthorized("المستخدم غير موجود.");

                // جلب بيانات الدور لمعرفة هل هو مدير نظام أم لا
                var role = await _context.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Role_ID == user.Role_ID);

                bool isSystemAdmin = role?.Is_System_Admin ?? false;

                // استخدام .Trim() لمنع فشل الدخول بسبب المسافات الفارغة المخفية في قاعدة البيانات
                if (!isSystemAdmin && user.Company_ID.Trim() != request.Company_ID.Trim())
                    return Unauthorized("المستخدم ليس لديه صلاحية على هذه الشركة.");

                // 2. التحقق من كلمة المرور باستخدام الحقل الفعلي Password_Hash
                if (user.Password_Hash != request.Password)
                    return Unauthorized("كلمة المرور غير صحيحة.");

                // 3. إرجاع البيانات بنجاح متطابقة مع نموذج الـ Desktop مع إسناد الشركة والفرع من الـ request لمرونة مدير النظام
                return Ok(new
                {
                    user.User_ID,
                    user.Full_Name,
                    user.Login_Name,
                    user.Role_ID,

                    Branch_ID = request.Branch_ID,
                    Company_ID = request.Company_ID.Trim(),

                    Year_ID = request.Year_ID, // تمرير السنة المالية المختارة للجلسة
                    Is_System_Admin = isSystemAdmin
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطأ داخلي في السيرفر: {ex.Message}");
            }
        }
    }

    public class LoginRequestDto
    {
        public string Company_ID { get; set; } = string.Empty;
        public int Branch_ID { get; set; }
        public int Year_ID { get; set; }
        public int User_ID { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}