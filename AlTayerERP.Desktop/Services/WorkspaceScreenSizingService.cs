using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// يضبط مساحة الشاشات المستضافة داخل تبويبات FrmMain.
    /// يحافظ على مساحة العمل بجانب شجرة النظام ويظهر أشرطة تمرير عند الحاجة،
    /// بدلاً من قص أعلى أو أسفل أو يمين الشاشة.
    /// </summary>
    internal static class WorkspaceScreenSizingService
    {
        private const string ViewportName = "pnlWorkspaceScreenViewport";

        public static Panel CreateViewport() =>
            new()
            {
                Name = ViewportName,
                Dock = DockStyle.Fill,
                AutoScroll = true,
                AutoScrollMargin = new Size(8, 8),
                BackColor = Color.FromArgb(249, 250, 252),
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };

        public static bool IsViewport(Control? control) =>
            control is Panel panel &&
            string.Equals(panel.Name, ViewportName, StringComparison.Ordinal);

        public static Size GetDesignSize(Form form)
        {
            var width = Math.Max(900, form.Width);
            var height = Math.Max(620, form.Height);
            return new Size(width, height);
        }

        public static void Prepare(Form form, Panel viewport, Size designSize)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.StartPosition = FormStartPosition.Manual;
            form.WindowState = FormWindowState.Normal;
            form.AutoScroll = false;
            form.MinimumSize = Size.Empty;
            form.MaximumSize = Size.Empty;
            form.Margin = Padding.Empty;
            form.Padding = Padding.Empty;
            form.Dock = DockStyle.None;
            form.Location = Point.Empty;
            FitToViewport(form, viewport, designSize);
        }

        public static void FitToViewport(Form form, Panel viewport, Size designSize)
        {
            if (form.IsDisposed || viewport.IsDisposed)
                return;

            var available = viewport.ClientSize;
            if (available.Width <= 0 || available.Height <= 0)
                return;

            // لا تصغّر الشاشة تحت مقاسها التصميمي؛ عند عدم كفاية المساحة يظهر التمرير.
            var target = new Size(
                Math.Max(available.Width, designSize.Width),
                Math.Max(available.Height, designSize.Height));

            form.Size = target;
            form.Location = Point.Empty;
            viewport.AutoScrollMinSize = target;
            viewport.PerformLayout();
        }
    }
}
