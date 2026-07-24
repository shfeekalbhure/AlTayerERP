using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// خدمة الاتصال المركزية بالـ API؛ ترسل JWT في ترويسة Authorization.
    /// </summary>
    public static class ApiService
    {
        /// <summary>رابط API المحلي لبيئة التطوير وينتهي بشرطة مائلة.</summary>
        public static readonly string BaseUrl = "https://localhost:7021/api/";

        /// <summary>عميل الاتصال المشترك بجميع الشاشات.</summary>
        public static readonly HttpClient Client = new()
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        /// <summary>
        /// يطبق Access Token الذاكري. ترويسة X-Session-Token مؤقتة للتوافق مع
        /// النداءات الموجودة، بينما الاعتماد الرسمي هو Authorization: Bearer.
        /// </summary>
        public static void ApplySessionToken(string accessToken)
        {
            Client.DefaultRequestHeaders.Authorization = null;
            Client.DefaultRequestHeaders.Remove("X-Session-Token");

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
                Client.DefaultRequestHeaders.Add("X-Session-Token", accessToken);
            }
        }

        /// <summary>يمسح كل ترويسات الجلسة عند الخروج.</summary>
        public static void ClearSessionToken()
        {
            Client.DefaultRequestHeaders.Authorization = null;
            Client.DefaultRequestHeaders.Remove("X-Session-Token");
        }
    }
}