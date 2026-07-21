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

            Application.ThreadException += (_, e) => ShowStartupError(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception exception)
                    ShowStartupError(exception);
            };

            try
            {
                Application.Run(new FrmLogin());
            }
            catch (Exception ex)
            {
                // لا نسمح بأن يختفي البرنامج عند فشل إنشاء شاشة الدخول.
                ShowStartupError(ex);
            }
        }

        private static void ShowStartupError(Exception ex)
        {
            MessageBox.Show(
                "تعذر بدء شاشة الدخول.\n\n" +
                "السبب الفني:\n" + ex.Message + "\n\n" +
                "تأكد من نجاح Build ومن تشغيل مشروع AlTayerERP.Desktop.",
                "خطأ بدء التشغيل",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
