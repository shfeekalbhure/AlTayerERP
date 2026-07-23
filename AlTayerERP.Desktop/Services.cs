using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// خدمة الاتصال المركزية بالـ API.
    /// </summary>
    public static class ApiService
    {
        /// <summary>
        /// رابط API المحلي لبيئة التطوير.
        /// يجب أن ينتهي بشرطة مائلة /.
        /// يجب أن يطابق منفذ HTTPS في launchSettings لمشروع AlTayerERP.API.
        /// </summary>
        public static readonly string BaseUrl =
            "https://localhost:7021/api/";

        /// <summary>
        /// كائن الاتصال المركزي بجميع شاشات النظام.
        /// </summary>
        public static readonly HttpClient Client =
            new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

        // يرسل الرمز في ترويسة موحدة لكل طلب بعد نجاح الدخول.
        public static void ApplySessionToken(string accessToken)
        {
            Client.DefaultRequestHeaders.Remove("X-Session-Token");
            if (!string.IsNullOrWhiteSpace(accessToken))
                Client.DefaultRequestHeaders.Add("X-Session-Token", accessToken);
        }

        public static void ClearSessionToken()
        {
            Client.DefaultRequestHeaders.Remove("X-Session-Token");
        }
    }
}
