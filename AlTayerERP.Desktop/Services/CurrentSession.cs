using System;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// الجلسة المحلية الحالية. لا تُعد بديلاً عن التحقق في الـ API.
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
        // رمز الجلسة يصدره الخادم بعد التحقق؛ لا يخزن على القرص.
        public static string Access_Token { get; set; } = "";
        public static string Currency_Code { get; set; } = "YER";
        public static string Language { get; set; } = "AR";
        public static DateTime Login_Time { get; set; } = DateTime.Now;
        public static string Device_Name { get; } = Environment.MachineName;

        // لا تعتبر الجلسة صالحة إلا إذا اكتمل المستخدم والشركة والفرع والسنة.
        public static bool IsLoggedIn =>
            User_ID > 0 &&
            !string.IsNullOrWhiteSpace(Company_ID) &&
            Branch_ID > 0 &&
            Year_ID > 0;

        // مسح جميع القيم عند تسجيل الخروج لمنع انتقال نطاق المستخدم للجلسة التالية.
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
            Access_Token = "";
            ApiService.ClearSessionToken();
            Currency_Code = "YER";
            Language = "AR";
            Login_Time = DateTime.Now;
        }
    }
}