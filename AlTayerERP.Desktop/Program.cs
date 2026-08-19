using System;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    internal static class Program
    {
        /// <summary>
        /// نقطة تشغيل تطبيق سطح المكتب.
        /// شاشة الدخول هي النافذة الأساسية دائماً.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // حماية موحدة: خطأ شاشة واحدة لا ينهي النظام بصمت، ويسجل التفاصيل للمراجعة.
            AppExceptionHandler.Initialize();

            try
            {
                Application.Run(new FrmLogin());
            }
            catch (Exception ex)
            {
                AppExceptionHandler.HandleUiException(ex, "بدء شاشة الدخول");
            }
        }
    }
}
