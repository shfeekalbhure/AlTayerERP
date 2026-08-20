using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// مخزن الجلسات القابلة للإبطال على الخادم. JWT وحده لا يكفي لأن الخروج يجب أن يبطل
    /// الوصول فوراً؛ لذلك تربط كل مطالبة JWT بمعرف Session_ID موجود هنا.
    /// </summary>
    public sealed class ServerSessionService
    {
        private static readonly ConcurrentDictionary<string, ServerSession> Sessions = new();
        private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(30);

        /// <summary>ينشئ جلسة مرتبطة بنطاق شركة وفرع وسنة من الخادم فقط.</summary>
        public ServerSession Create(
            int userId,
            int roleId,
            bool isSystemAdmin,
            string companyId,
            int branchId,
            int yearId,
            string deviceId)
        {
            RemoveExpired();

            var issuedAt = DateTime.UtcNow;
            var session = new ServerSession(
                Guid.NewGuid().ToString("N"),
                userId,
                roleId,
                isSystemAdmin,
                companyId,
                branchId,
                yearId,
                deviceId,
                issuedAt,
                issuedAt.Add(SessionLifetime));

            Sessions[session.Session_ID] = session;
            return session;
        }

        /// <summary>يعيد الجلسة إذا كانت موجودة وغير منتهية الصلاحية.</summary>
        public bool TryGet(string? sessionId, out ServerSession session)
        {
            session = default!;
            if (string.IsNullOrWhiteSpace(sessionId) || !Sessions.TryGetValue(sessionId, out var stored))
                return false;

            if (stored.Expires_At <= DateTime.UtcNow)
            {
                Sessions.TryRemove(sessionId, out _);
                return false;
            }

            session = stored;
            return true;
        }

        /// <summary>يبطل جلسة محددة عند الخروج أو كشف استخدام غير صالح.</summary>
        public void Remove(string? sessionId)
        {
            if (!string.IsNullOrWhiteSpace(sessionId))
                Sessions.TryRemove(sessionId, out _);
        }

        /// <summary>
        /// يبطل جلسات المستخدم الأخرى بعد تغيير كلمة المرور. لا تُمس الجلسة التي
        /// نفذت التغيير حتى يتلقى العميل نتيجة العملية بصورة سليمة.
        /// </summary>
        public int RemoveOtherSessionsForUser(int userId, string currentSessionId)
        {
            RemoveExpired();
            var removed = 0;
            foreach (var item in Sessions)
            {
                if (item.Value.User_ID == userId &&
                    !string.Equals(item.Key, currentSessionId, StringComparison.Ordinal))
                {
                    if (Sessions.TryRemove(item.Key, out _))
                        removed++;
                }
            }

            return removed;
        }

        /// <summary>يعيد لقائمة الرقابة جلسات حية فقط؛ لا يعيد رموز وصول أو تجديد.</summary>
        public IReadOnlyCollection<ServerSession> GetActiveSessions()
        {
            RemoveExpired();
            return Sessions.Values.OrderBy(x => x.Issued_At).ToArray();
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

    /// <summary>السياق الموثوق الذي يستعمله API بدلاً من أي معرفات يرسلها العميل.</summary>
    public sealed record ServerSession(
        string Session_ID,
        int User_ID,
        int Role_ID,
        bool Is_System_Admin,
        string Company_ID,
        int Branch_ID,
        int Year_ID,
        string Device_ID,
        DateTime Issued_At,
        DateTime Expires_At)
    {
        /// <summary>
        /// توافق مؤقت مع نقاط الحماية القديمة التي كانت تنشئ جلسة فارغة للتحقق فقط.
        /// لا يصدر هذا المُنشئ JWT ولا يضيف الجلسة إلى مخزن الجلسات.
        /// </summary>
        [Obsolete("استخدم ServerSessionService.Create لإنشاء جلسة موثقة.")]
        public ServerSession(
            string legacySessionId,
            int userId,
            int roleId,
            bool isSystemAdmin,
            string companyId,
            int branchId,
            int yearId,
            DateTime expiresAt)
            : this(
                legacySessionId,
                userId,
                roleId,
                isSystemAdmin,
                companyId,
                branchId,
                yearId,
                "legacy",
                expiresAt == DateTime.MinValue ? DateTime.MinValue : expiresAt.AddMinutes(-30),
                expiresAt)
        {
        }
    }
}
