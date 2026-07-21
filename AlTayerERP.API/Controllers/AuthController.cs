using AlTayerERP.API.Security;
using AlTayerERP.Infrastructure.Data;
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

        public AuthController(AppDbContext context) => _context = context;

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Company_ID) ||
                request.Branch_ID <= 0 ||
                request.Year_ID <= 0 ||
                request.User_ID <= 0 ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("يجب تحديد الشركة والفرع والسنة المالية والمستخدم وكلمة المرور.");
            }

            try
            {
                var companyId = request.Company_ID.Trim();

                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.User_ID == request.User_ID && x.Is_Active);

                if (user == null)
                    return Unauthorized("بيانات الدخول غير صحيحة.");

                var role = await _context.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Role_ID == user.Role_ID && x.Is_Active);

                if (role == null)
                    return Unauthorized("دور المستخدم غير فعال.");

                var isSystemAdmin = role.Is_System_Admin;

                var companyExists = await _context.Companies
                    .AsNoTracking()
                    .AnyAsync(x => x.Company_ID == companyId && x.Is_Active);

                if (!companyExists)
                    return BadRequest("الشركة المختارة غير موجودة أو غير فعالة.");

                var branch = await _context.Tenant_Branches
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Branch_ID == request.Branch_ID &&
                        x.Company_ID == companyId &&
                        x.Is_Active);

                if (branch == null)
                    return BadRequest("الفرع المختار لا يتبع الشركة أو غير فعال.");

                var fiscalYear = await _context.Fiscal_Years
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Fiscal_Year_ID == request.Year_ID &&
                        x.Company_ID == companyId &&
                        x.Is_Active &&
                        !x.Is_Closed);

                if (fiscalYear == null)
                    return BadRequest("السنة المالية المختارة لا تتبع الشركة أو أنها مقفلة/غير فعالة.");

                if (!isSystemAdmin &&
                    (!string.Equals(user.Company_ID?.Trim(), companyId, StringComparison.Ordinal) ||
                     user.Branch_ID != request.Branch_ID))
                {
                    return Unauthorized("المستخدم غير مخول للشركة أو الفرع المختار.");
                }

                if (!PasswordProtector.Verify(user.Password_Hash, request.Password, out var needsUpgrade))
                    return Unauthorized("بيانات الدخول غير صحيحة.");

                if (needsUpgrade)
                {
                    user.Password_Hash = PasswordProtector.Hash(request.Password);
                    user.Updated_At = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    user.User_ID,
                    user.Full_Name,
                    user.Login_Name,
                    user.Role_ID,
                    Branch_ID = branch.Branch_ID,
                    Company_ID = companyId,
                    Year_ID = fiscalYear.Fiscal_Year_ID,
                    Is_System_Admin = isSystemAdmin,
                    Must_Change_Password = user.Must_Change_Password
                });
            }
            catch
            {
                // لا تُرسل تفاصيل الاستثناء للعميل لأنها قد تكشف معلومات عن الخادم أو قاعدة البيانات.
                return StatusCode(500, "تعذر إتمام عملية تسجيل الدخول حالياً.");
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