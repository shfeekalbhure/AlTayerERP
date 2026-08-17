using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AlTayerERP.API.Security
{
    /// <summary>
    /// يتحقق من JWT ثم من جلسة الخادم وسياق الشركة/الفرع/السنة في كل طلب محمي.
    /// لا تُقبل هوية أو نطاق عمل يرسله تطبيق سطح المكتب في جسم الطلب.
    /// </summary>
    public sealed class ServerSessionAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>اسم مخطط المصادقة الافتراضي للتطبيق.</summary>
        public const string SchemeName = "ServerSession";

        private readonly TokenService _tokens;
        private readonly ServerSessionService _sessions;
        private readonly AppDbContext _context;

        public ServerSessionAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            TokenService tokens,
            ServerSessionService sessions,
            AppDbContext context)
            : base(options, logger, encoder)
        {
            _tokens = tokens;
            _sessions = sessions;
            _context = context;
        }

        /// <summary>
        /// يقرأ Bearer Token أولاً، ثم ترويسة X-Session-Token توافقياً لتطبيق سطح المكتب.
        /// </summary>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var accessToken = ReadAccessToken();
            if (string.IsNullOrWhiteSpace(accessToken))
                return AuthenticateResult.NoResult();

            if (!_tokens.TryValidateAccessToken(accessToken, out var tokenClaims) ||
                !_sessions.TryGet(tokenClaims.Session_ID, out var session) ||
                session.User_ID != tokenClaims.User_ID)
            {
                return AuthenticateResult.Fail("انتهت الجلسة أو رمز الوصول غير صالح.");
            }

            // تحقق نطاق العمل لكل طلب يمنع استمرار الجلسة إذا أوقف المستخدم أو الدور أو السياق.
            // الاستعلامات المنفصلة متعمدة؛ وهي أوضح وأضمن من استعلام متداخل على كل موفر بيانات.
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.User_ID == session.User_ID && x.Is_Active);
            var roleIsValid = user != null &&
                user.Role_ID == session.Role_ID &&
                await _context.Roles.AsNoTracking().AnyAsync(x =>
                    x.Role_ID == session.Role_ID &&
                    x.Is_Active &&
                    x.Is_System_Admin == session.Is_System_Admin);
            var companyIsValid = await _context.Companies.AsNoTracking()
                .AnyAsync(x => x.Company_ID == session.Company_ID && x.Is_Active);
            var branchIsValid = await _context.Tenant_Branches.AsNoTracking()
                .AnyAsync(x => x.Branch_ID == session.Branch_ID &&
                               x.Company_ID == session.Company_ID &&
                               x.Is_Active);
            var yearIsValid = await _context.Fiscal_Years.AsNoTracking()
                .AnyAsync(x => x.Fiscal_Year_ID == session.Year_ID &&
                               x.Company_ID == session.Company_ID &&
                               x.Is_Active &&
                               !x.Is_Closed);
            var userScopeIsValid = session.Is_System_Admin ||
                (user != null &&
                 string.Equals(user.Company_ID?.Trim(), session.Company_ID, StringComparison.Ordinal) &&
                 user.Branch_ID == session.Branch_ID);
            var isValidContext = roleIsValid && companyIsValid && branchIsValid &&
                                 yearIsValid && userScopeIsValid;

            if (!isValidContext)
            {
                _sessions.Remove(session.Session_ID);
                return AuthenticateResult.Fail("لم يعد سياق الجلسة صالحاً.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, session.User_ID.ToString()),
                new Claim(ClaimTypes.Name, session.User_ID.ToString()),
                new Claim(ClaimTypes.Role, session.Role_ID.ToString()),
                new Claim("session_id", session.Session_ID),
                new Claim("company_id", session.Company_ID),
                new Claim("branch_id", session.Branch_ID.ToString()),
                new Claim("fiscal_year_id", session.Year_ID.ToString()),
                new Claim("is_system_admin", session.Is_System_Admin.ToString())
            };

            Context.Items["ServerSession"] = session;
            var identity = new ClaimsIdentity(claims, SchemeName);
            return AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName));
        }

        private string? ReadAccessToken()
        {
            var authorization = Request.Headers.Authorization.ToString();
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return authorization["Bearer ".Length..].Trim();

            // التوافق المؤقت مع العميل الحالي إلى أن ينقل كل الاستدعاءات إلى Authorization Bearer.
            return Request.Headers["X-Session-Token"].ToString();
        }
    }
}