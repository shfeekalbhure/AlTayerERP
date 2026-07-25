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
        pnlSideMenu.Dock = DockStyle.Right;
        pnlSideMenu.BringToFront();
        pnlTopBar.BringToFront();
        pnlStatusBar.BringToFront();
        EnsureMenuSearchBox();
    }
}
