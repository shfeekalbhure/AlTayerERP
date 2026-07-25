using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// ضبط الاستجابة النهائية لشاشة الشركات المبنية داخل CompanyForm.Designer.cs.
/// لا يعاد إنشاء عناصر الشاشة وقت التشغيل حتى لا يختفي الرأس المؤسسي
/// أو القائمة الجانبية أو لوحة التدقيق المعتمدة.
/// </summary>
public partial class CompanyForm
{
    private bool _approvedLayoutApplied;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (_approvedLayoutApplied)
            return;

        _approvedLayoutApplied = true;
        ApplyApprovedCompanyLayout();
        BeginInvoke(new Action(ApplyCompanyResponsiveSizing));
        Resize += (_, _) => ApplyCompanyResponsiveSizing();
    }

    /// <summary>
    /// يثبت إعدادات العناصر الحالية فقط دون Controls.Clear أو إنشاء تخطيط بديل.
    /// </summary>
    private void ApplyApprovedCompanyLayout()
    {
        SuspendLayout();

        Text = "إدارة الشركات";
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        MinimumSize = new Size(1180, 700);

        btnSaveCompany.Text = "حفظ";
        btnDelete.Text = "إيقاف";
        btnApprove.Text = "إعادة تفعيل";
        btnUnApprove.Visible = false;

        cmbGroups.DropDownStyle = ComboBoxStyle.DropDownList;
        chkIsActive.Enabled = false;
        chkIsActive.TabStop = false;

        pnlHeader.BringToFront();
        pnlToolbar.BringToFront();
        splitMain.BringToFront();
        pnlAudit.BringToFront();

        ResumeLayout(true);
    }

    /// <summary>
    /// يحافظ على القائمة الجانبية والحقول والتدقيق في العرض العادي والمضغوط.
    /// </summary>
    private void ApplyCompanyResponsiveSizing()
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        bool insideWorkspace = !TopLevel || Parent is TabPage;
        bool compact = ClientSize.Width < 1180 || ClientSize.Height < 720;

        if (insideWorkspace)
        {
            MinimumSize = Size.Empty;
            MaximumSize = Size.Empty;
            Dock = DockStyle.Fill;
        }

        splitMain.SplitterDistance = compact
            ? Math.Max(320, Math.Min(390, ClientSize.Width / 3))
            : Math.Max(400, Math.Min(460, ClientSize.Width / 3));

        pnlHeader.Height = compact ? 66 : 74;
        pnlToolbar.Height = compact ? 52 : 58;

        foreach (Control control in pnlToolbar.Controls)
        {
            if (control is not Button button)
                continue;

            button.Width = compact ? 80 : 88;
            button.Height = compact ? 34 : 40;
            button.Margin = new Padding(3, 0, 3, 0);
        }

        grpBasic.Height = compact ? 210 : 228;
        grpContact.Height = compact ? 150 : 165;
        grpLogo.Height = compact ? 170 : 190;
        pnlAudit.Height = 50;

        Invalidate(true);
    }
}
