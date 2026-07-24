using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// يصدر ويتحقق من JWT قصير العمر، ويدير تدوير Refresh Token المخزن كبصمة فقط.
    /// </summary>
    public sealed class TokenService
    {
        private const string Issuer = "AlTayerERP";
        private const string Audience = "AlTayerERP.Desktop";
        private static readonly TimeSpan AccessLifetime = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(7);

        private readonly AppDbContext _context;
        private readonly ILogger<TokenService> _logger;
        private readonly byte[] _signingKey;

        public TokenService(
            AppDbContext context,
            IConfiguration configuration,
            IHostEnvironment environment,
            ILogger<TokenService> logger)
        {
            _context = context;
            _logger = logger;

            // الإنتاج يتطلب مفتاحاً خارج المستودع. في التطوير ينشأ مفتاح مؤقت كي لا يتعطل
            // التشغيل المحلي، وبالتالي تبطل الرموز تلقائياً عند إعادة تشغيل الخادم.
            var configuredKey = configuration["Jwt:SigningKey"];
            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                if (!environment.IsDevelopment())
                    throw new InvalidOperationException("يجب ضبط Jwt__SigningKey بطول 32 حرفاً على الأقل في بيئة الإنتاج.");

                configuredKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
                _logger.LogWarning("يستخدم الخادم مفتاح JWT مؤقتاً لبيئة التطوير؛ ستبطل الجلسات عند إعادة التشغيل.");
            }

            _signingKey = Encoding.UTF8.GetBytes(configuredKey);
            if (_signingKey.Length < 32)
                throw new InvalidOperationException("Jwt__SigningKey يجب أن يكون بطول 32 بايت على الأقل.");
        }

        /// <summary>ينشئ Access Token مرتبطاً بجلسة خادم محددة.</summary>
        public AccessTokenResult CreateAccessToken(ServerSession session)
        {
            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.Add(AccessLifetime);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, session.User_ID.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim("sid", session.Session_ID),
                new Claim(ClaimTypes.Role, session.Role_ID.ToString()),
                new Claim("company_id", session.Company_ID),
                new Claim("branch_id", session.Branch_ID.ToString()),
                new Claim("fiscal_year_id", session.Year_ID.ToString()),
                new Claim("is_system_admin", session.Is_System_Admin.ToString())
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(_signingKey),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                notBefore: issuedAt,
                expires: expiresAt,
                signingCredentials: credentials);

            return new AccessTokenResult(
                new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt);
        }

        /// <summary>
        /// يتحقق من التوقيع والعمر والجمهور، ثم يعيد Session_ID فقط دون الثقة في حقول العميل.
        /// </summary>
        public bool TryValidateAccessToken(string? accessToken, out AccessTokenClaims claims)
        {
            claims = default!;
            if (string.IsNullOrWhiteSpace(accessToken))
                return false;

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(accessToken,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(_signingKey),
                        ValidateIssuer = true,
                        ValidIssuer = Issuer,
                        ValidateAudience = true,
                        ValidAudience = Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    },
                    out _);

                var userIdText = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                var sessionId = principal.FindFirstValue("sid");
                if (!int.TryParse(userIdText, out var userId) || string.IsNullOrWhiteSpace(sessionId))
                    return false;

                claims = new AccessTokenClaims(userId, sessionId);
                return true;
            }
            catch (SecurityTokenException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>ينشئ رمز تجديد عشوائياً، ويحفظ بصمته فقط داخل المعاملة الحالية.</summary>
        public async Task<RefreshTokenResult> IssueRefreshTokenAsync(
            ServerSession session,
            string deviceId,
            CancellationToken cancellationToken = default)
        {
            var value = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var hash = Hash(value);
            var expiresAt = DateTime.UtcNow.Add(RefreshLifetime);

            _context.Refresh_Tokens.Add(new RefreshToken
            {
                Token_Hash = hash,
                Session_ID = session.Session_ID,
                User_ID = session.User_ID,
                Company_ID = session.Company_ID,
                Branch_ID = session.Branch_ID,
                Fiscal_Year_ID = session.Year_ID,
                Device_ID = deviceId,
                Expires_At = expiresAt
            });

            await _context.SaveChangesAsync(cancellationToken);
            return new RefreshTokenResult(value, expiresAt, hash);
        }

        /// <summary>يبطل كل رموز التجديد التابعة لجلسة خروج واحدة.</summary>
        public async Task RevokeSessionRefreshTokensAsync(
            string sessionId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var tokens = await _context.Refresh_Tokens
                .Where(x => x.Session_ID == sessionId && x.Revoked_At == null)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
            {
                token.Revoked_At = DateTime.UtcNow;
                token.Revoked_Reason = reason;
            }

            if (tokens.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>تطابق البصمة دون إعادة كشف قيمة الرمز الأصلية.</summary>
        public static string Hash(string value) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

        public sealed record AccessTokenResult(string Access_Token, DateTime Expires_At);
        public sealed record RefreshTokenResult(string Refresh_Token, DateTime Expires_At, string Token_Hash);
        public sealed record AccessTokenClaims(int User_ID, string Session_ID);
    }
}