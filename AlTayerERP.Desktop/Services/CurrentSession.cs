using System;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// يحتفظ ببيانات جلسة المستخدم الحالية.
    /// جميع شاشات النظام تعتمد على هذا الكلاس.
    /// </summary>
    public static class CurrentSession
    {
        #region بيانات الشركة

        public static string Company_ID { get; set; } = "";

        public static string Company_Name { get; set; } = "";

        #endregion

        #region بيانات الفرع

        public static int Branch_ID { get; set; }

        public static string Branch_Name { get; set; } = "";

        #endregion

        #region السنة المالية

        public static int Year_ID { get; set; }

        public static string Year_Name { get; set; } = "";

        public static DateTime? FiscalYear_StartDate { get; set; }

        public static DateTime? FiscalYear_EndDate { get; set; }

        #endregion

        #region المستخدم

        public static int User_ID { get; set; }

        public static int Role_ID { get; set; }

        public static string Username { get; set; } = "";

        public static string Full_Name { get; set; } = "";

        public static bool Is_System_Admin { get; set; }

        #endregion

        #region إعدادات عامة

        public static string Currency_Code { get; set; } = "YER";

        public static string Language { get; set; } = "AR";

        public static DateTime Login_Time { get; set; } = DateTime.Now;

        public static string Device_Name { get; set; }
            = Environment.MachineName;

        #endregion

        /// <summary>
        /// هل المستخدم سجل الدخول بنجاح؟
        /// </summary>
        public static bool IsLoggedIn =>
            User_ID > 0 &&
            !string.IsNullOrWhiteSpace(Company_ID);

        /// <summary>
        /// مسح بيانات الجلسة عند تسجيل الخروج.
        /// </summary>
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

            Currency_Code = "YER";
            Language = "AR";

            Is_System_Admin = false;

            Login_Time = DateTime.Now;
        }
    }
}