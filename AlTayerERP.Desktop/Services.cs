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
#if DEBUG
        // يسمح بـ HTTP فقط لخادم التطوير المحلي.
        private const string DefaultBaseUrl = "http://localhost:5021/api/";
#else
        // إصدار التشغيل يفرض HTTPS ولا يرجع إلى HTTP عند فشل الاتصال.
        private const string DefaultBaseUrl = "https://localhost:5021/api/";
#endif

        /// <summary>رابط API من إعداد التشغيل أو الافتراضي الآمن، وينتهي بشرطة مائلة.</summary>
        public static readonly string BaseUrl = ResolveBaseUrl();

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

        private static string ResolveBaseUrl()
        {
            var configured = Environment.GetEnvironmentVariable("ALTAYER_API_BASE_URL");
            var value = string.IsNullOrWhiteSpace(configured) ? DefaultBaseUrl : configured.Trim();

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
                throw new InvalidOperationException("عنوان خادم AlTayerERP غير صالح.");

#if !DEBUG
            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("يتطلب إصدار التشغيل عنوان API يعمل عبر HTTPS.");
#endif

            return uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
                ? uri.AbsoluteUri
                : uri.AbsoluteUri + "/";
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

                // قوائم الفروع تستخدم GetFromJsonAsync الذي يولد رسالة .NET إنجليزية
                // عند 404 أو 409. نرمي هنا النص العربي الآمن فقط لهذا المسار.
                if (IsBranchReferenceRequest(request))
                {
                    var statusCode = response.StatusCode;
                    response.Dispose();
                    throw new HttpRequestException(safeMessage, null, statusCode);
                }

                response.Content.Dispose();
                response.Content = new StringContent(safeMessage, Encoding.UTF8, "text/plain");
                return response;
            }

            /// <summary>يتحقق من أن الطلب يخص أنواع الفروع أو عملات الشركة.</summary>
            private static bool IsBranchReferenceRequest(HttpRequestMessage request)
            {
                var path = request.RequestUri?.AbsolutePath ?? string.Empty;
                return path.Contains("branch-reference-lookups", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
