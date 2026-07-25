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
        // إعادة ترتيب الـDock تحجز مساحة العمل فعلياً ولا تسمح للشجرة بتغطية النموذج.
        Controls.SetChildIndex(pnlTopBar, 0);
        Controls.SetChildIndex(pnlStatusBar, 1);
        Controls.SetChildIndex(pnlSideMenu, 2);
        Controls.SetChildIndex(pnlWorkspace, 3);
        EnsureMenuSearchBox();
    }
}
