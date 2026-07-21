using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace AlTayerERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private static readonly PasswordHasher<User> PasswordHasher = new();

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

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
                return BadRequest(new { success = false, message = "بيانات الدخول غير مكتملة." });
            }

            string companyId = request.Company_ID.Trim();

            try
            {
                User? user = await _context.Users
                    .FirstOrDefaultAsync(x => x.User_ID == request.User_ID && x.Is_Active);

                if (user == null)
                    return Unauthorized(new { success = false, message = "بيانات الدخول غير صحيحة." });

                Role? role = await _context.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Role_ID == user.Role_ID && x.Is_Active);

                if (role == null)
                    return Unauthorized(new { success = false, message = "دور المستخدم غير نشط أو غير موجود." });

                bool isSystemAdmin = role.Is_System_Admin;

                bool companyExists = await _context.Companies
                    .AsNoTracking()
                    .AnyAsync(x => x.Company_ID == companyId && x.Is_Active);

                if (!companyExists)
                    return Unauthorized(new { success = false, message = "الشركة المختارة غير نشطة أو غير موجودة." });

                if (!isSystemAdmin &&
                    !string.Equals(user.Company_ID?.Trim(), companyId, StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new { success = false, message = "المستخدم ليس مصرحاً له بهذه الشركة." });
                }

                bool branchExists = await _context.Tenant_Branches
                    .AsNoTracking()
                    .AnyAsync(x => x.Branch_ID == request.Branch_ID &&
                                   x.Company_ID == companyId &&
                                   x.Is_Active);

                if (!branchExists)
                    return Unauthorized(new { success = false, message = "الفرع المختار لا يتبع الشركة أو غير نشط." });

                if (!isSystemAdmin && user.Branch_ID > 0 && user.Branch_ID != request.Branch_ID)
                {
                    return Unauthorized(new { success = false, message = "المستخدم ليس مصرحاً له بالفرع المختار." });
                }

                bool fiscalYearExists = await _context.Fiscal_Years
                    .AsNoTracking()
                    .AnyAsync(x => x.Fiscal_Year_ID == request.Year_ID &&
                                   x.Company_ID == companyId &&
                                   x.Is_Active &&
                                   !x.Is_Closed);

                if (!fiscalYearExists)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "السنة المالية المختارة غير صالحة أو مغلقة."
                    });
                }

                if (!VerifyPassword(user, request.Password, out bool upgradedLegacyPassword))
                    return Unauthorized(new { success = false, message = "بيانات الدخول غير صحيحة." });

                if (upgradedLegacyPassword)
                    await _context.SaveChangesAsync();

                List<ScreenPermissionDto> screenPermissions = isSystemAdmin
                    ? new List<ScreenPermissionDto>()
                    : await (
                        from permission in _context.RolePermissions.AsNoTracking()
                        join screen in _context.SystemScreens.AsNoTracking()
                            on permission.Screen_ID equals screen.Screen_ID
                        where permission.Role_ID == user.Role_ID &&
                              screen.Is_Active &&
                              permission.Can_View
                        orderby screen.Sort_Order
                        select new ScreenPermissionDto
                        {
                            Screen_Code = screen.Screen_Code,
                            Can_View = permission.Can_View,
                            Can_Add = permission.Can_Add,
                            Can_Edit = permission.Can_Edit,
                            Can_Delete = permission.Can_Delete,
                            Can_Print = permission.Can_Print,
                            Can_Export = permission.Can_Export,
                            Can_Import = permission.Can_Import,
                            Can_Approve = permission.Can_Approve,
                            Can_UnApprove = permission.Can_UnApprove
                        }).ToListAsync();

                return Ok(new LoginResultDto
                {
                    User_ID = user.User_ID,
                    Full_Name = user.Full_Name,
                    Login_Name = user.Login_Name,
                    Role_ID = user.Role_ID,
                    Branch_ID = request.Branch_ID,
                    Company_ID = companyId,
                    Year_ID = request.Year_ID,
                    Is_System_Admin = isSystemAdmin,
                    Access_Token = CreateAccessToken(user, role, request, companyId),
                    Screen_Permissions = screenPermissions
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "تعذر إتمام تسجيل الدخول حالياً."
                });
            }
        }

        private string CreateAccessToken(User user, Role role, LoginRequestDto request, string companyId)
        {
            string issuer = _configuration["Jwt:Issuer"] ?? "AlTayerERP.API";
            string audience = _configuration["Jwt:Audience"] ?? "AlTayerERP.Desktop";
            string signingKey = _configuration["Jwt:SigningKey"]
                ?? throw new InvalidOperationException("مفتاح JWT غير مضبوط.");
            int lifetimeMinutes = int.TryParse(_configuration["Jwt:LifetimeMinutes"], out int value)
                ? Math.Clamp(value, 5, 480)
                : 60;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.User_ID.ToString()),
                new(ClaimTypes.NameIdentifier, user.User_ID.ToString()),
                new(ClaimTypes.Name, user.Login_Name),
                new("company_id", companyId),
                new("branch_id", request.Branch_ID.ToString()),
                new("year_id", request.Year_ID.ToString()),
                new("role_id", role.Role_ID.ToString())
            };

            if (role.Is_System_Admin)
                claims.Add(new Claim(ClaimTypes.Role, "SystemAdmin"));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddMinutes(lifetimeMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        private static bool VerifyPassword(User user, string suppliedPassword, out bool upgradedLegacyPassword)
        {
            upgradedLegacyPassword = false;

            PasswordVerificationResult result = PasswordHasher.VerifyHashedPassword(
                user,
                user.Password_Hash ?? string.Empty,
                suppliedPassword);

            if (result == PasswordVerificationResult.Success ||
                result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.Password_Hash = PasswordHasher.HashPassword(user, suppliedPassword);
                    upgradedLegacyPassword = true;
                }

                return true;
            }

            // توافق انتقالي: الحسابات القديمة التي خزنت كلمة المرور كنص يتم ترحيلها
            // إلى Hash فور أول دخول ناجح، دون إبقاء مقارنة النص كسياسة دائمة.
            byte[] stored = Encoding.UTF8.GetBytes(user.Password_Hash ?? string.Empty);
            byte[] supplied = Encoding.UTF8.GetBytes(suppliedPassword);

            if (stored.Length == 0 ||
                stored.Length != supplied.Length ||
                !CryptographicOperations.FixedTimeEquals(stored, supplied))
            {
                return false;
            }

            user.Password_Hash = PasswordHasher.HashPassword(user, suppliedPassword);
            user.Updated_At = DateTime.UtcNow;
            upgradedLegacyPassword = true;
            return true;
        }
    }

    public sealed class LoginRequestDto
    {
        public string Company_ID { get; set; } = string.Empty;
        public int Branch_ID { get; set; }
        public int Year_ID { get; set; }
        public int User_ID { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginResultDto
    {
        public int User_ID { get; set; }
        public string Full_Name { get; set; } = string.Empty;
        public string Login_Name { get; set; } = string.Empty;
        public int Role_ID { get; set; }
        public int Branch_ID { get; set; }
        public string Company_ID { get; set; } = string.Empty;
        public int Year_ID { get; set; }
        public bool Is_System_Admin { get; set; }
        public string Access_Token { get; set; } = string.Empty;
        public List<ScreenPermissionDto> Screen_Permissions { get; set; } = new();
    }

    public sealed class ScreenPermissionDto
    {
        public string Screen_Code { get; set; } = string.Empty;
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