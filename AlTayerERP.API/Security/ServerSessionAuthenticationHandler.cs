using AlTayerERP.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AlTayerERP.API.Security
{
    /// <summary>
    /// مخطط مصادقة يعتمد جلسة الخادم الصادرة بعد تسجيل الدخول.
    /// يمنع هذا المخطط تحوّل نتائج 401/403 إلى استثناء 500 عند استخدام Forbid().
    /// </summary>
    public sealed class ServerSessionAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>اسم مخطط المصادقة الافتراضي للتطبيق.</summary>
        public const string SchemeName = "ServerSession";

        public ServerSessionAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        /// <summary>
        /// يحوّل الجلسة الموثوقة التي وضعها Middleware إلى هوية Claims رسمية.
        /// لا يقرأ أي User_ID أو Role_ID مرسل من تطبيق سطح المكتب.
        /// </summary>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Context.Items["ServerSession"] is not ServerSession session)
                return Task.FromResult(AuthenticateResult.NoResult());

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, session.User_ID.ToString()),
                new Claim(ClaimTypes.Name, session.User_ID.ToString()),
                new Claim(ClaimTypes.Role, session.Role_ID.ToString()),
                new Claim("company_id", session.Company_ID),
                new Claim("branch_id", session.Branch_ID.ToString()),
                new Claim("fiscal_year_id", session.Year_ID.ToString()),
                new Claim("is_system_admin", session.Is_System_Admin.ToString())
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
