using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// يحتفظ بجلسات سطح المكتب النشطة داخل الخادم.
    /// الغرض: عدم قبول هوية مستخدم يرسلها العميل وحده عند استدعاء واجهات حساسة.
    /// الجلسة قصيرة العمر وتُلغى عند تسجيل الخروج أو انتهاء المدة.
    /// </summary>
    public sealed class ServerSessionService
    {
        private static readonly ConcurrentDictionary<string, ServerSession> Sessions = new();
        private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

        public ServerSession Create(int userId, int roleId, bool isSystemAdmin, string companyId, int branchId, int yearId)
        {
            RemoveExpired();

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var session = new ServerSession(
                token,
                userId,
                roleId,
                isSystemAdmin,
                companyId,
                branchId,
                yearId,
                DateTime.UtcNow.Add(SessionLifetime));

            Sessions[token] = session;
            return session;
        }

        public bool TryGet(string? token, out ServerSession session)
        {
            session = default!;

            if (string.IsNullOrWhiteSpace(token) || !Sessions.TryGetValue(token, out var stored))
                return false;

            if (stored.Expires_At <= DateTime.UtcNow)
            {
                Sessions.TryRemove(token, out _);
                return false;
            }

            session = stored;
            return true;
        }

        public void Remove(string? token)
        {
            if (!string.IsNullOrWhiteSpace(token))
                Sessions.TryRemove(token, out _);
        }

        private static void RemoveExpired()
        {
            foreach (var item in Sessions)
            {
                if (item.Value.Expires_At <= DateTime.UtcNow)
                    Sessions.TryRemove(item.Key, out _);
            }
        }
    }

    public sealed record ServerSession(
        string Access_Token,
        int User_ID,
        int Role_ID,
        bool Is_System_Admin,
        string Company_ID,
        int Branch_ID,
        int Year_ID,
        DateTime Expires_At);
}
