using System;
using System.Net.Http;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// خدمة الاتصال المركزية بالـ API.
    /// </summary>
    public static class ApiService
    {
        /// <summary>
        /// الرابط الأساسي للـ API.
        /// يجب أن ينتهي بشرطة مائلة /.
        /// </summary>
        public static readonly string BaseUrl =
            "https://localhost:7011/api/";

        /// <summary>
        /// كائن الاتصال المركزي بجميع شاشات النظام.
        /// </summary>
        public static readonly HttpClient Client =
            new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
    }
}