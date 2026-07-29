using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// خدمة الاتصال المركزية بالـ API. تمر جميع الاستجابات عبر حارس يختصر
    /// رسائل الخطأ للمستخدم ولا يسمح بتمرير JSON أو تفاصيل تقنية حساسة للشاشات.
    /// </summary>
    public static class ApiService
    {
        /// <summary>رابط API المحلي لبيئة التطوير وينتهي بشرطة مائلة.</summary>
        public static readonly string BaseUrl = "http://localhost:5021/api/";

        /// <summary>عميل الاتصال المشترك بجميع الشاشات.</summary>
        public static readonly HttpClient Client = new(new SafeApiErrorHandler(new HttpClientHandler()))
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        /// <summary>يطبق رمز الجلسة في الذاكرة فقط ولا يكتبه في الرسائل أو السجلات.</summary>
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

        /// <summary>
        /// يستخرج الحقل message فقط من أخطاء JSON، ويستبدل أي محتوى تقني
        /// برسالة عربية عامة قبل أن تقرأه واجهات سطح المكتب.
        /// </summary>
        private sealed class SafeApiErrorHandler : DelegatingHandler
        {
            public SafeApiErrorHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode || response.Content is null)
                    return response;

                var safeMessage = "تعذر إكمال العملية. راجع مسؤول النظام.";
                try
                {
                    var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                    using var document = JsonDocument.Parse(raw);
                    if (document.RootElement.ValueKind == JsonValueKind.Object &&
                        document.RootElement.TryGetProperty("message", out var messageElement))
                    {
                        var candidate = messageElement.GetString();
                        if (!string.IsNullOrWhiteSpace(candidate))
                            safeMessage = candidate.Trim();
                    }
                    else if (document.RootElement.ValueKind == JsonValueKind.String)
                    {
                        var candidate = document.RootElement.GetString();
                        if (!string.IsNullOrWhiteSpace(candidate))
                            safeMessage = candidate.Trim();
                    }
                }
                catch
                {
                    // لا نمرر النص الخام لأنه قد يحتوي Stack Trace أو أسرار جلسة.
                }

                response.Content.Dispose();
                response.Content = new StringContent(safeMessage, Encoding.UTF8, "text/plain");
                return response;
            }
        }
    }
}
