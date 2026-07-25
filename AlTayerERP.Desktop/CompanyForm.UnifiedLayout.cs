using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// الضبط النهائي لتخطيط شاشة الشركات داخل مساحة العمل الفعلية.
/// الهدف منع ظهور القائمة ضيقة جداً، وتقليل الفراغات، وتثبيت الحقول والجدول
/// بشكل واضح على أجهزة العرض الصغيرة والمتوسطة.
/// </summary>
public partial class CompanyForm
{
    private bool _approvedLayoutApplied;
    private bool _layoutReflowed;

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
    /// يثبت إعدادات العناصر الحالية ويعيد توزيع الشاشة لتناسب مساحة العمل داخل الشاشة الرئيسية.
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
        btnImport.Visible = false;
        btnExport.Visible = false;
        btnPreview.Visible = false;

        cmbGroups.DropDownStyle = ComboBoxStyle.DropDownList;
        chkIsActive.Enabled = false;
        chkIsActive.TabStop = false;

        ReflowMainAreaForWorkspace();

        pnlHeader.BringToFront();
        pnlToolbar.BringToFront();
        splitMain.BringToFront();
        pnlAudit.BringToFront();

        ResumeLayout(true);
    }

    /// <summary>
    /// يحول توزيع القائمة من عمود جانبي ضيق إلى جدول سفلي عريض،
    /// مع بقاء تفاصيل الشركة في الأعلى، لأن هذا أوضح داخل شاشة ERP الرئيسية.
    /// </summary>
    private void ReflowMainAreaForWorkspace()
    {
        if (_layoutReflowed)
            return;

        _layoutReflowed = true;

        splitMain.SuspendLayout();
        splitMain.Panel1.SuspendLayout();
        splitMain.Panel2.SuspendLayout();

        pnlDetails.Parent?.Controls.Remove(pnlDetails);
        pnlListHeader.Parent?.Controls.Remove(pnlListHeader);
        dgvCompanies.Parent?.Controls.Remove(dgvCompanies);

        splitMain.Orientation = Orientation.Horizontal;
        splitMain.FixedPanel = FixedPanel.Panel2;
        splitMain.Panel1.Padding = new Padding(8);
        splitMain.Panel2.Padding = new Padding(8);
        splitMain.Panel1.Controls.Add(pnlDetails);
        splitMain.Panel2.Controls.Add(dgvCompanies);
        splitMain.Panel2.Controls.Add(pnlListHeader);

        pnlDetails.Dock = DockStyle.Fill;
        pnlListHeader.Dock = DockStyle.Top;
        dgvCompanies.Dock = DockStyle.Fill;

        pnlListHeader.Height = 58;
        lblListTitle.Dock = DockStyle.Right;
        lblListTitle.TextAlign = ContentAlignment.MiddleRight;
        lblListTitle.Width = 160;

        dgvCompanies.RowHeadersVisible = false;
        dgvCompanies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvCompanies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvCompanies.MultiSelect = false;

        splitMain.Panel2.ResumeLayout(true);
        splitMain.Panel1.ResumeLayout(true);
        splitMain.ResumeLayout(true);
    }

    /// <summary>
    /// يحافظ على الحقول والتدقيق والجدول في العرض العادي والمضغوط.
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

        pnlHeader.Height = compact ? 62 : 70;
        pnlToolbar.Height = compact ? 46 : 52;

        foreach (Control control in pnlToolbar.Controls)
        {
            if (control is not Button button)
                continue;

            button.Width = compact ? 76 : 88;
            button.Height = compact ? 32 : 36;
            button.Margin = new Padding(3, 0, 3, 0);
        }

        int availableHeight = Math.Max(520, splitMain.Height);
        int tableHeight = compact ? 210 : 240;
        splitMain.SplitterDistance = Math.Max(300, availableHeight - tableHeight);

        grpBasic.Height = compact ? 168 : 178;
        grpContact.Height = compact ? 118 : 128;
        grpLogo.Height = compact ? 136 : 150;
        pnlAudit.Height = 48;
        pnlListHeader.Height = compact ? 48 : 54;

        AdjustLogoArea(compact);
        Invalidate(true);
    }

    private void AdjustLogoArea(bool compact)
    {
        picCompanyLogo.SizeMode = PictureBoxSizeMode.Zoom;
        picCompanyLogo.Width = compact ? 120 : 145;
        picCompanyLogo.Height = compact ? 80 : 95;

        btnBrowseLogo.Width = compact ? 100 : 118;
        btnRemoveLogo.Width = compact ? 100 : 118;
        btnBrowseLogo.Height = 30;
        btnRemoveLogo.Height = 30;
    }
}
