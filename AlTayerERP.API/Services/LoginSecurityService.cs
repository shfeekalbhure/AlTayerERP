using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// يطبق قفل الحساب وتسجيل محاولات الدخول من الخادم فقط.
    /// </summary>
    public sealed class LoginSecurityService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private readonly AppDbContext _context;

        public LoginSecurityService(AppDbContext context) => _context = context;

        /// <summary>يفحص القفل الحالي دون كشف سببه للعميل.</summary>
        public bool IsLocked(User user) =>
            user.Locked_Until.HasValue && user.Locked_Until.Value > DateTime.UtcNow;

        /// <summary>يسجل نجاح الدخول ويصفر العداد داخل نفس حفظ المستخدم.</summary>
        public async Task RecordSuccessAsync(
            User user,
            string loginName,
            string companyId,
            int branchId,
            int yearId,
            string? ipAddress,
            string? userAgent,
            string deviceId,
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            user.Failed_Login_Count = 0;
            user.Last_Failed_Login_At = null;
            user.Locked_Until = null;
            user.Last_Login_At = DateTime.UtcNow;
            user.Last_Login_IP = Trim(ipAddress, 64);

            _context.Login_Attempts.Add(CreateAttempt(
                user.User_ID, loginName, companyId, branchId, yearId, true, null,
                ipAddress, userAgent, deviceId, sessionId, null));

            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>يسجل الفشل ويقفل الحساب بعد خمس محاولات متتالية.</summary>
        public async Task<DateTime?> RecordFailureAsync(
            User? user,
            string loginName,
            string? companyId,
            int? branchId,
            int? yearId,
            string reason,
            string? ipAddress,
            string? userAgent,
            string? deviceId,
            CancellationToken cancellationToken = default)
        {
            DateTime? lockoutUntil = null;
            if (user != null)
            {
                user.Failed_Login_Count++;
                user.Last_Failed_Login_At = DateTime.UtcNow;
                if (user.Failed_Login_Count >= MaxFailedAttempts)
                {
                    lockoutUntil = DateTime.UtcNow.Add(LockoutDuration);
                    user.Locked_Until = lockoutUntil;
                    user.Failed_Login_Count = 0;
                }
            }

            _context.Login_Attempts.Add(CreateAttempt(
                user?.User_ID, loginName, companyId, branchId, yearId, false, reason,
                ipAddress, userAgent, deviceId, null, lockoutUntil));

            await _context.SaveChangesAsync(cancellationToken);
            return lockoutUntil;
        }

        private static LoginAttempt CreateAttempt(
            int? userId, string loginName, string? companyId, int? branchId, int? yearId,
            bool isSuccess, string? reason, string? ipAddress, string? userAgent,
            string? deviceId, string? sessionId, DateTime? lockoutUntil) =>
            new()
            {
                User_ID = userId,
                Login_Name = Trim(loginName, 100) ?? string.Empty,
                Company_ID = Trim(companyId, 50),
                Branch_ID = branchId,
                Fiscal_Year_ID = yearId,
                Is_Success = isSuccess,
                Failure_Reason = Trim(reason, 100),
                IP_Address = Trim(ipAddress, 64),
                User_Agent = Trim(userAgent, 512),
                Device_ID = Trim(deviceId, 128),
                Session_ID = Trim(sessionId, 64),
                Lockout_Until = lockoutUntil
            };

        private static string? Trim(string? value, int length) =>
            string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().Length <= length ? value.Trim() : value.Trim()[..length];
    }
}