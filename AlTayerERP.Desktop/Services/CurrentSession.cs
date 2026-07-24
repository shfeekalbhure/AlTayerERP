using System;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// نسخة ذاكرة فقط من الجلسة التي صدّقها API. لا تعد بديلاً عن التحقق الخادمي.
    /// لا تحفظ Access Token أو Refresh Token على القرص.
    /// </summary>
    public static class CurrentSession
    {
        public static string Company_ID { get; set; } = "";
        public static string Company_Name { get; set; } = "";
        public static int Branch_ID { get; set; }
        public static string Branch_Name { get; set; } = "";
        public static int Year_ID { get; set; }
        public static string Year_Name { get; set; } = "";
        public static DateTime? FiscalYear_StartDate { get; set; }
        public static DateTime? FiscalYear_EndDate { get; set; }
        public static int User_ID { get; set; }
        public static int Role_ID { get; set; }
        public static string Username { get; set; } = "";
        public static string Full_Name { get; set; } = "";
        public static bool Is_System_Admin { get; set; }

        // بيانات الجلسة والتوكنات مصدرها الخادم فقط.
        public static string Session_ID { get; set; } = "";
        public static string Access_Token { get; set; } = "";
        public static DateTime? Access_Token_Expires_At { get; set; }
        public static string Refresh_Token { get; set; } = "";
        public static DateTime? Refresh_Token_Expires_At { get; set; }

        public static string Currency_Code { get; set; } = "YER";
        public static string Language { get; set; } = "AR";
        public static DateTime Login_Time { get; set; } = DateTime.Now;
        public static string Device_Name { get; } = Environment.MachineName;

        // لا تعتبر الجلسة محلية صالحة إلا إذا اكتملت الهوية والسياق ورمز الوصول غير المنتهي.
        public static bool IsLoggedIn =>
            User_ID > 0 &&
            !string.IsNullOrWhiteSpace(Session_ID) &&
            !string.IsNullOrWhiteSpace(Access_Token) &&
            Access_Token_Expires_At > DateTime.UtcNow &&
            !string.IsNullOrWhiteSpace(Company_ID) &&
            Branch_ID > 0 &&
            Year_ID > 0;

        /// <summary>يمسح الرموز والنطاق بالكامل عند الخروج أو انتهاء الجلسة.</summary>
        public static void Clear()
        {
            Company_ID = "";
            Company_Name = "";
            Branch_ID = 0;
            Branch_Name = "";
            Year_ID = 0;
            Year_Name = "";
            FiscalYear_StartDate = null;
            FiscalYear_EndDate = null;
            User_ID = 0;
            Role_ID = 0;
            Username = "";
            Full_Name = "";
            Is_System_Admin = false;
            Session_ID = "";
            Access_Token = "";
            Access_Token_Expires_At = null;
            Refresh_Token = "";
            Refresh_Token_Expires_At = null;
            ApiService.ClearSessionToken();
            Currency_Code = "YER";
            Language = "AR";
            Login_Time = DateTime.Now;
        }
    }
}