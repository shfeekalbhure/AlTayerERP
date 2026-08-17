using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// حارس أخطاء سطح المكتب: يعرض للمستخدم رسالة عربية مختصرة،
    /// ويسجل تفاصيل منقحة محلياً دون أي أسرار أو رؤوس مصادقة.
    /// </summary>
    internal static class AppExceptionHandler
    {
        private static bool _showingError;

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

        public static void HandleUiException(Exception exception, string operation)
        {
            WriteLog(exception, operation);

            if (_showingError)
                return;

            _showingError = true;
            try
            {
                MessageBox.Show(
                    "تعذر إكمال العملية. تم تسجيل الخطأ فنياً دون عرض أي بيانات حساسة.\n" +
                    "أعد المحاولة، وإن استمرت المشكلة راجع مسؤول النظام.",
                    "خطأ في الشاشة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _showingError = false;
            }
        }

        /// <summary>
        /// يسجل الاستثناء بعد تنقية أي بيانات مصادقة أو اتصال قد تكون موجودة في النص.
        /// لا يسجل رؤوس الطلبات ولا محتوى الطلبات ولا إعدادات الاتصال.
        /// </summary>
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
                var safeDetails = Sanitize(exception.ToString());
                var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {operation}{Environment.NewLine}" +
                            $"{safeDetails}{Environment.NewLine}{new string('-', 80)}{Environment.NewLine}";
                File.AppendAllText(file, entry);
            }
            catch
            {
                // التسجيل لا يجب أن يسبب خطأ إضافياً أو يمنع استمرار الواجهة.
            }
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "لا توجد تفاصيل إضافية.";

            var result = value;
            var secretNames = new[]
            {
                "Authorization",
                "X-Session-Token",
                "Cookie",
                "Set-Cookie",
                "Password",
                "Pwd",
                "ConnectionString",
                "DefaultConnection"
            };

            foreach (var name in secretNames)
            {
                result = Regex.Replace(
                    result,
                    $@"(?im)({Regex.Escape(name)}\s*[:=]\s*)([^\r\n;]+)",
                    "$1[REDACTED]");
            }

            result = Regex.Replace(
                result,
                @"(?im)Bearer\s+[A-Za-z0-9\-\._~\+\/]+=*",
                "Bearer [REDACTED]");

            result = Regex.Replace(
                result,
                @"(?im)(Server|Host|Database|User Id|Uid|Password|Pwd)\s*=\s*[^;\r\n]+",
                "$1=[REDACTED]");

            return result;
        }
    }
}
