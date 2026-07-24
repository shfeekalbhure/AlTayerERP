using System;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

public partial class FrmMain
{
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // لا نعكس ترتيب Dock على مستوى النافذة؛ يبقى اتجاه النص عربيًا وتبقى الشجرة يمين الشاشة.
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = false;
        pnlSideMenu.Dock = DockStyle.Right;
        pnlSideMenu.BringToFront();
        pnlTopBar.BringToFront();
        pnlStatusBar.BringToFront();
    }
}
