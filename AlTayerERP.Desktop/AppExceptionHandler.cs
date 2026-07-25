using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// حارس أخطاء سطح المكتب: يسجل الاستثناءات ويمنع إغلاق واجهة WinForms بصمت.
    /// لا يعالج منطق الأعمال؛ وظيفته عرض الخطأ وتوفير سجل قابل للمراجعة.
    /// </summary>
    internal static class AppExceptionHandler
    {
        private static bool _showingError;

        /// <summary>يربط معالجات أخطاء الواجهة والمهام غير المراقبة مرة واحدة عند بدء التطبيق.</summary>
        public static void Initialize()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, eventArgs) =>
                HandleUiException(eventArgs.Exception, "واجهة المستخدم");

            TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
            {
                WriteLog(eventArgs.Exception, "مهمة خلفية غير مراقبة");
                eventArgs.SetObserved();
            };

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
            {
                if (eventArgs.ExceptionObject is Exception exception)
                    WriteLog(exception, "استثناء على مستوى التطبيق");
            };
        }

        /// <summary>
        /// يسجل الخطأ ويعرض رسالة واحدة للمستخدم بدلاً من ترك التطبيق يغلق أو يهتز.
        /// </summary>
        public static void HandleUiException(Exception exception, string operation)
        {
            WriteLog(exception, operation);

            // يمنع تكرار مربعات الخطأ عند حدوث سلسلة أخطاء من الشاشة نفسها.
            if (_showingError)
                return;

            _showingError = true;
            try
            {
                MessageBox.Show(
                    "تعذر إكمال العملية: " + operation + ".\n\n" +
                    "لم يتم إغلاق النظام. يمكن إغلاق الشاشة الحالية أو المحاولة لاحقاً.\n\n" +
                    "التفاصيل: " + exception.Message,
                    "خطأ في الشاشة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _showingError = false;
            }
        }

        /// <summary>يحفظ تفاصيل الاستثناء في ملف محلي قابل للإرسال للدعم الفني.</summary>
        private static void WriteLog(Exception exception, string operation)
        {
            try
            {
                var folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AlTayerERP",
                    "Logs");
                Directory.CreateDirectory(folder);

                var file = Path.Combine(folder, "desktop-errors.log");
                var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {operation}{Environment.NewLine}" +
                            $"{exception}{Environment.NewLine}{new string('-', 80)}{Environment.NewLine}";
                File.AppendAllText(file, entry);
            }
            catch
            {
                // التسجيل لا يجب أن يسبب خطأ إضافياً أو يمنع استمرار الواجهة.
            }
        }
    }
}
