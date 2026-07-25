using System;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

public partial class FrmMain
{
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // تبقى شجرة النظام في يمين الشاشة مع اتجاه عربي للنصوص.
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = false;
        // تطبق الحواف أولاً ثم مساحة العمل المتبقية؛ لا نستخدم BringToFront كي لا تغطي الشجرة الشاشة.
        pnlTopBar.Dock = DockStyle.Top;
        pnlStatusBar.Dock = DockStyle.Bottom;
        pnlSideMenu.Dock = DockStyle.Right;
        pnlWorkspace.Dock = DockStyle.Fill;
        PerformLayout();
        EnsureMenuSearchBox();
    }
}
