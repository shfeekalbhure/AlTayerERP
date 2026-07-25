using AlTayerERP.API.Services;
using AlTayerERP.API.Security;
using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// مصادقة النظام: دخول مؤمّن، تجديد رمز، خروج قابل للإبطال، وسجل محاولات الدخول.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ServerSessionService _sessions;
        private readonly TokenService _tokens;
        private readonly LoginSecurityService _loginSecurity;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDbContext context,
            ServerSessionService sessions,
            TokenService tokens,
            LoginSecurityService loginSecurity,
            ILogger<AuthController> logger)
        {
            _context = context;
            _sessions = sessions;
            _tokens = tokens;
            _loginSecurity = loginSecurity;
            _logger = logger;
        }

        /// <summary>
        /// يتحقق من الشركة والفرع والسنة والمستخدم وكلمة المرور قبل إنشاء JWT وجلسة خادم.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Company_ID) ||
                request.Branch_ID <= 0 ||
                request.Year_ID <= 0 ||
                (request.User_ID <= 0 && string.IsNullOrWhiteSpace(request.Login_Name)) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("يجب تحديد الشركة والفرع والسنة المالية والمستخدم وكلمة المرور.");
            }

            var companyId = request.Company_ID.Trim();
            var loginName = request.Login_Name?.Trim() ?? string.Empty;
            var deviceId = NormalizeDeviceId(request.Device_ID);
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers.UserAgent.ToString();

            try
            {
                // لا يشترط النشاط هنا حتى نسجل محاولة حساب موقوف من دون كشف سبب الرفض للعميل.
                var user = await _context.Users.FirstOrDefaultAsync(x =>
                    request.User_ID > 0
                        ? x.User_ID == request.User_ID
                        : x.Login_Name == loginName,
                    cancellationToken);

                if (user == null || !user.Is_Active)
                    return await LoginFailedAsync(user, loginName, companyId, request.Branch_ID, request.Year_ID,
                        "INVALID_CREDENTIALS", ipAddress, userAgent, deviceId, cancellationToken);

                if (_loginSecurity.IsLocked(user))
                    return await LoginFailedAsync(user, loginName, companyId, request.Branch_ID, request.Year_ID,
                        "ACCOUNT_LOCKED", ipAddress, userAgent, deviceId, cancellationToken);

                var role = await _context.Roles.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Role_ID == user.Role_ID && x.Is_Active, cancellationToken);
                if (role == null)
                    return await LoginFailedAsync(user, loginName, companyId, request.Branch_ID, request.Year_ID,
                        "ROLE_INACTIVE", ipAddress, userAgent, deviceId, cancellationToken);

                var companyExists = await _context.Companies.AsNoTracking()
                    .AnyAsync(x => x.Company_ID == companyId && x.Is_Active, cancellationToken);
                var branchExists = await _context.Tenant_Branches.AsNoTracking()
                    .AnyAsync(x => x.Branch_ID == request.Branch_ID &&
                                   x.Company_ID == companyId &&
                                   x.Is_Active, cancellationToken);
                var fiscalYearExists = await _context.Fiscal_Years.AsNoTracking()
                    .AnyAsync(x => x.Fiscal_Year_ID == request.Year_ID &&
                                   x.Company_ID == companyId &&
                                   x.Is_Active &&
                                   !x.Is_Closed, cancellationToken);

                if (!companyExists || !branchExists || !fiscalYearExists)
                    return await LoginFailedAsync(user, loginName, companyId, request.Branch_ID, request.Year_ID,
                        "INVALID_TENANT_CONTEXT", ipAddress, userAgent, deviceId, cancellationToken);

                // المستخدم العادي لا يستطيع تبديل شركته أو فرعه؛ المدير العام يتجاوز ذلك وفق دوره فقط.
                if (!role.Is_System_Admin &&
                    (!string.Equals(user.Company_ID?.Trim(), companyId, StringComparison.Ordinal) ||
                     user.Branch_ID != request.Branch_ID))
                {
                    return await LoginFailedAsync(user, loginName, companyId, request.Branch_ID, request.Year_ID,
                        "TENANT_ACCESS_DENIED", ipAddress, userAgent, deviceId, cancellationToken);
                }

                if (!PasswordProtector.Verify(user.Password_Hash, request.Password, out var needsUpgrade))
                    return await LoginFailedAsync(user, loginName, companyId, request.Branch_ID, request.Year_ID,
                        "INVALID_CREDENTIALS", ipAddress, userAgent, deviceId, cancellationToken);

                if (needsUpgrade)
                {
                    // ترقية كلمة المرور القديمة بعد تحقق ناجح فقط.
                    user.Password_Hash = PasswordProtector.Hash(request.Password);
                    user.Updated_At = DateTime.UtcNow;
                }

                var session = _sessions.Create(
                    user.User_ID, role.Role_ID, role.Is_System_Admin,
                    companyId, request.Branch_ID, request.Year_ID, deviceId);
                var access = _tokens.CreateAccessToken(session);

                try
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                    var refresh = await _tokens.IssueRefreshTokenAsync(session, deviceId, cancellationToken);
                    await _loginSecurity.RecordSuccessAsync(
                        user, loginName, companyId, request.Branch_ID, request.Year_ID,
                        ipAddress, userAgent, deviceId, session.Session_ID, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return Ok(CreateLoginResponse(user, session, access, refresh));
                }
                catch
                {
                    _sessions.Remove(session.Session_ID);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "فشل تسجيل الدخول للمستخدم {LoginName} ضمن الشركة {CompanyId} والفرع {BranchId}.",
                    loginName, companyId, request.Branch_ID);
                return StatusCode(500, "تعذر إتمام عملية تسجيل الدخول حالياً.");
            }
        }

        /// <summary>
        /// يدوّر Refresh Token: يبطل القديم ويصدر جلسة وAccess Token وRefresh Token جديدين.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Refresh_Token))
                return BadRequest("رمز التجديد مطلوب.");

            var suppliedHash = TokenService.Hash(request.Refresh_Token);
            var storedToken = await _context.Refresh_Tokens
                .FirstOrDefaultAsync(x => x.Token_Hash == suppliedHash, cancellationToken);

            if (storedToken == null ||
                storedToken.Revoked_At != null ||
                storedToken.Expires_At <= DateTime.UtcNow ||
                !string.Equals(storedToken.Device_ID, NormalizeDeviceId(request.Device_ID), StringComparison.Ordinal))
            {
                return Unauthorized("انتهت الجلسة أو رمز التجديد غير صالح.");
            }

            var context = await GetValidContextAsync(
                storedToken.User_ID, storedToken.Company_ID, storedToken.Branch_ID,
                storedToken.Fiscal_Year_ID, cancellationToken);
            if (context == null || _loginSecurity.IsLocked(context.User))
            {
                storedToken.Revoked_At = DateTime.UtcNow;
                storedToken.Revoked_Reason = "CONTEXT_INVALID";
                await _context.SaveChangesAsync(cancellationToken);
                return Unauthorized("انتهت الجلسة أو لم يعد نطاق العمل صالحاً.");
            }

            var session = _sessions.Create(
                context.User.User_ID, context.Role.Role_ID, context.Role.Is_System_Admin,
                storedToken.Company_ID, storedToken.Branch_ID, storedToken.Fiscal_Year_ID,
                storedToken.Device_ID);
            var access = _tokens.CreateAccessToken(session);

            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                storedToken.Revoked_At = DateTime.UtcNow;
                storedToken.Revoked_Reason = "ROTATED";
                var refresh = await _tokens.IssueRefreshTokenAsync(session, storedToken.Device_ID, cancellationToken);
                storedToken.Replaced_By_Hash = refresh.Token_Hash;
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Ok(CreateLoginResponse(context.User, session, access, refresh));
            }
            catch
            {
                _sessions.Remove(session.Session_ID);
                throw;
            }
        }

        /// <summary>يبطل Access/Refresh الخاصة بالجلسة، ثم يمسحها العميل محلياً.</summary>
        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            if (HttpContext.Items["ServerSession"] is ServerSession session)
            {
                await _tokens.RevokeSessionRefreshTokensAsync(
                    session.Session_ID, "USER_LOGOUT", cancellationToken);
                _sessions.Remove(session.Session_ID);
            }

            return Ok(new { message = "تم إنهاء الجلسة." });
        }

        /// <summary>
        /// يغير كلمة مرور المستخدم صاحب الجلسة فقط. لا يستقبل معرف مستخدم ولا يسجل
        /// أي كلمة مرور في سجل التدقيق أو في الاستجابة.
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request, CancellationToken cancellationToken)
        {
            if (HttpContext.Items["ServerSession"] is not ServerSession session)
                return Unauthorized();

            if (request is null || string.IsNullOrWhiteSpace(request.Current_Password) ||
                string.IsNullOrWhiteSpace(request.New_Password) || string.IsNullOrWhiteSpace(request.Confirm_New_Password))
                return BadRequest("كلمات المرور الحالية والجديدة والتأكيد مطلوبة.");

            if (!string.Equals(request.New_Password, request.Confirm_New_Password, StringComparison.Ordinal))
                return BadRequest("تأكيد كلمة المرور الجديدة غير مطابق.");

            var passwordRuleError = ValidateNewPassword(request.New_Password);
            if (passwordRuleError is not null)
                return BadRequest(passwordRuleError);

            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.User_ID == session.User_ID && x.Is_Active, cancellationToken);
            if (user is null)
                return Unauthorized();

            if (!PasswordProtector.Verify(user.Password_Hash, request.Current_Password, out _))
                return BadRequest("كلمة المرور الحالية غير صحيحة.");

            if (PasswordProtector.Verify(user.Password_Hash, request.New_Password, out _))
                return BadRequest("يجب أن تختلف كلمة المرور الجديدة عن كلمة المرور الحالية.");

            user.Password_Hash = PasswordProtector.Hash(request.New_Password);
            user.Must_Change_Password = false;
            user.Updated_At = DateTime.UtcNow;

            var audit = HttpContext.RequestServices.GetRequiredService<AuditTrailService>();
            audit.Add(session, HttpContext, "users", user.User_ID.ToString(), "PASSWORD_CHANGE",
                newValues: new { PasswordChanged = true, user.Must_Change_Password },
                notes: "تغيير ذاتي لكلمة المرور");

            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new { message = "تم تغيير كلمة المرور بنجاح." });
        }

        /// <summary>يعرض سياق الجلسة الموثوق للعميل؛ لا يقبل سياقاً من الجسم.</summary>
        [Authorize]
        [HttpGet("CurrentSession")]
        public IActionResult CurrentSession()
        {
            if (HttpContext.Items["ServerSession"] is not ServerSession session)
                return Unauthorized();

            return Ok(new
            {
                session.Session_ID,
                session.User_ID,
                session.Role_ID,
                session.Company_ID,
                session.Branch_ID,
                Year_ID = session.Year_ID,
                session.Is_System_Admin,
                session.Issued_At,
                session.Expires_At
            });
        }

        private async Task<IActionResult> LoginFailedAsync(
            User? user, string loginName, string? companyId, int? branchId, int? yearId,
            string reason, string? ipAddress, string? userAgent, string deviceId,
            CancellationToken cancellationToken)
        {
            await _loginSecurity.RecordFailureAsync(
                user, loginName, companyId, branchId, yearId, reason,
                ipAddress, userAgent, deviceId, cancellationToken);
            // رسالة عامة متعمدة لمنع كشف المستخدم أو سبب الرفض أو مدة القفل.
            return Unauthorized("بيانات الدخول غير صحيحة أو أن الحساب غير متاح حالياً.");
        }

        private async Task<LoginContext?> GetValidContextAsync(
            int userId, string companyId, int branchId, int yearId, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.User_ID == userId && x.Is_Active, cancellationToken);
            if (user == null)
                return null;

            var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(
                x => x.Role_ID == user.Role_ID && x.Is_Active, cancellationToken);
            if (role == null)
                return null;

            var companyExists = await _context.Companies.AsNoTracking()
                .AnyAsync(x => x.Company_ID == companyId && x.Is_Active, cancellationToken);
            var branchExists = await _context.Tenant_Branches.AsNoTracking()
                .AnyAsync(x => x.Branch_ID == branchId && x.Company_ID == companyId && x.Is_Active, cancellationToken);
            var yearExists = await _context.Fiscal_Years.AsNoTracking()
                .AnyAsync(x => x.Fiscal_Year_ID == yearId && x.Company_ID == companyId && x.Is_Active && !x.Is_Closed, cancellationToken);

            if (!companyExists || !branchExists || !yearExists ||
                (!role.Is_System_Admin &&
                 (!string.Equals(user.Company_ID?.Trim(), companyId, StringComparison.Ordinal) ||
                  user.Branch_ID != branchId)))
            {
                return null;
            }

            return new LoginContext(user, role);
        }

        private static object CreateLoginResponse(
            User user,
            ServerSession session,
            TokenService.AccessTokenResult access,
            TokenService.RefreshTokenResult refresh) =>
            new
            {
                user.User_ID,
                user.Full_Name,
                user.Login_Name,
                user.Role_ID,
                session.Branch_ID,
                session.Company_ID,
                Year_ID = session.Year_ID,
                session.Is_System_Admin,
                user.Must_Change_Password,
                session.Session_ID,
                Access_Token = access.Access_Token,
                Access_Token_Expires_At = access.Expires_At,
                Refresh_Token = refresh.Refresh_Token,
                Refresh_Token_Expires_At = refresh.Expires_At,
                Token_Type = "Bearer"
            };

        private string? GetClientIpAddress() =>
            HttpContext.Connection.RemoteIpAddress?.ToString();

        private static string? ValidateNewPassword(string password)
        {
            if (password.Length < 8)
                return "كلمة المرور الجديدة يجب ألا تقل عن 8 أحرف.";
            if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
                return "كلمة المرور الجديدة يجب أن تحتوي حرفاً واحداً ورقماً واحداً على الأقل.";
            return null;
        }

        private static string NormalizeDeviceId(string? deviceId) =>
            string.IsNullOrWhiteSpace(deviceId) ? "desktop-unknown" : deviceId.Trim()[..Math.Min(deviceId.Trim().Length, 128)];

        private sealed record LoginContext(User User, AlTayerERP.Core.Entities.Role Role);
    }

    /// <summary>بيانات الدخول؛ كلمة المرور تستخدم للنقل فقط ولا تسجل أو تعاد في الرد.</summary>
    public class LoginRequestDto
    {
        public string Company_ID { get; set; } = string.Empty;
        public int Branch_ID { get; set; }
        public int Year_ID { get; set; }
        public int User_ID { get; set; }
        public string Login_Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Device_ID { get; set; }
    }

    /// <summary>طلب تجديد الرمز لا يحمل هوية المستخدم أو الشركة؛ تستخرج من الرمز المخزن.</summary>
    public class RefreshRequestDto
    {
        public string Refresh_Token { get; set; } = string.Empty;
        public string? Device_ID { get; set; }
    }
}
