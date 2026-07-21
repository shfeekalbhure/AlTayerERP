using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// خدمة الاتصال المركزية بالـ API والجلسة الرمزية الحالية.
    /// لا تحفظ الرمز على القرص؛ ينتهي بانتهاء التطبيق أو تسجيل الخروج.
    /// </summary>
    public static class ApiService
    {
        public static readonly string BaseUrl = "https://localhost:7011/api/";

        public static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        public static void SetAccessToken(string accessToken)
        {
            Client.DefaultRequestHeaders.Authorization =
                string.IsNullOrWhiteSpace(accessToken)
                    ? null
                    : new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public static void ClearAccessToken()
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }
}