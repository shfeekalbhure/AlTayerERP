using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// يجدد Access Token من Refresh Token الذاكري قبل انتهائه. لا يخزن أي رمز على القرص.
    /// </summary>
    public static class SessionService
    {
        /// <summary>يحاول تدوير الرموز للجهاز الحالي ويعيد false عند فشل التجديد.</summary>
        public static async Task<bool> RefreshAccessTokenAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentSession.Refresh_Token) ||
                CurrentSession.Refresh_Token_Expires_At <= DateTime.UtcNow)
                return false;

            try
            {
                var response = await ApiService.Client.PostAsJsonAsync("Auth/Refresh", new
                {
                    Refresh_Token = CurrentSession.Refresh_Token,
                    Device_ID = CurrentSession.Device_Name
                });

                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadFromJsonAsync<RefreshResult>();
                if (result == null ||
                    string.IsNullOrWhiteSpace(result.Access_Token) ||
                    string.IsNullOrWhiteSpace(result.Refresh_Token))
                {
                    return false;
                }

                // الخادم هو المصدر الوحيد لمعرف الجلسة والرموز وأوقات الانتهاء.
                CurrentSession.Session_ID = result.Session_ID;
                CurrentSession.Access_Token = result.Access_Token;
                CurrentSession.Access_Token_Expires_At = result.Access_Token_Expires_At;
                CurrentSession.Refresh_Token = result.Refresh_Token;
                CurrentSession.Refresh_Token_Expires_At = result.Refresh_Token_Expires_At;
                ApiService.ApplySessionToken(result.Access_Token);
                return true;
            }
            catch
            {
                // لا تعرض الخدمة استثناءات الشبكة؛ الشاشة تقرر متى تنهي الجلسة.
                return false;
            }
        }

        private sealed class RefreshResult
        {
            public string Session_ID { get; set; } = string.Empty;
            public string Access_Token { get; set; } = string.Empty;
            public DateTime Access_Token_Expires_At { get; set; }
            public string Refresh_Token { get; set; } = string.Empty;
            public DateTime Refresh_Token_Expires_At { get; set; }
        }
    }
}