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
    private TableLayoutPanel? _companyEntryLayout;
    private TableLayoutPanel? _companyAuditCards;
    private FlowLayoutPanel? _toolbarActions;

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

        // الهوية الموحدة للشاشات: شريط أزرار ملون يبدأ من اليمين بـ «جديد».
        pnlHeader.Visible = false;
        pnlToolbar.BackColor = Color.FromArgb(248, 250, 252);
        pnlToolbar.Padding = new Padding(6, 5, 6, 5);
        pnlToolbar.FlowDirection = FlowDirection.RightToLeft;
        btnNew.Text = "+ جديد";
        btnSaveCompany.Text = "✔ حفظ";
        btnEdit.Text = "✎ تعديل";
        btnDelete.Text = "إيقاف";
        btnApprove.Text = "إعادة تفعيل";
        btnUnApprove.Visible = false;
        btnImport.Visible = false;
        btnExport.Visible = false;
        btnPreview.Visible = false;

        cmbGroups.DropDownStyle = ComboBoxStyle.DropDownList;
        chkIsActive.Enabled = false;
        chkIsActive.TabStop = false;

        grpBasic.Text = "بيانات الشركة الأساسية";
        grpContact.Text = "بيانات التواصل والعنوان";
        grpLogo.Text = "شعار الشركة";
        label1.Text = "المجموعة التجارية *";
        label2.Text = "اسم الشركة بالعربية *";
        label3.Text = "اسم الشركة بالإنجليزية";
        label4.Text = "كود الشركة *";
        label8.Text = "الرقم الضريبي";
        label5.Text = "الهاتف";
        label6.Text = "البريد الإلكتروني";
        label7.Text = "العنوان";
        ApplyRequestedToolbarColors();
        ApplyRequestedCardStyle();
        ArrangeToolbarForRtl();

        ReflowMainAreaForWorkspace();
        ArrangePrimaryCompanyFields();
        ArrangeCompanyEntryLikeReference();
        ArrangeAuditFooter();
        MoveAuditBelowGrid();

        pnlHeader.BringToFront();
        pnlToolbar.BringToFront();
        splitMain.BringToFront();
        pnlAudit.BringToFront();

        ResumeLayout(true);
    }

    /// <summary>يثبت الترتيب المرئي للأزرار من اليمين: جديد ثم حفظ ثم تعديل.</summary>
    private void ArrangeToolbarForRtl()
    {
        pnlToolbar.SuspendLayout();
        pnlToolbar.Controls.Clear();
        pnlToolbar.RightToLeft = RightToLeft.Yes;

        _toolbarActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            RightToLeft = RightToLeft.No,
            WrapContents = false,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        // الترتيب من اليسار إلى اليمين هنا يجعل «جديد» عند أقصى اليمين.
        _toolbarActions.Controls.AddRange(new Control[]
        {
            btnClose, btnRefresh, btnSearch, btnPrint, btnApprove,
            btnDelete, btnEdit, btnSaveCompany, btnNew
        });
        pnlToolbar.Controls.Add(_toolbarActions);
        pnlToolbar.ResumeLayout(true);
    }

    /// <summary>يعرض بيانات الإنشاء والتعديل والعدادات كبطاقات قابلة للقراءة.</summary>
    private void ArrangeAuditFooter()
    {
        if (_companyAuditCards is not null)
            return;

        pnlAudit.SuspendLayout();
        pnlAudit.Controls.Clear();
        pnlAudit.Height = 78;
        pnlAudit.Padding = new Padding(5);

        _companyAuditCards = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            RightToLeft = RightToLeft.Yes,
            BackColor = Color.FromArgb(241, 245, 249)
        };
        _companyAuditCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        _companyAuditCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        _companyAuditCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        _companyAuditCards.Controls.Add(CreateAuditCard("بيانات الإنشاء", "أنشئ بواسطة:", lblCreatedBy, "تاريخ الإنشاء:", lblCreatedAt), 0, 0);
        _companyAuditCards.Controls.Add(CreateAuditCard("بيانات التعديل", "عدل بواسطة:", lblModifiedBy, "تاريخ التعديل:", lblModifiedAt), 1, 0);
        _companyAuditCards.Controls.Add(CreateAuditCard("العدادات", "عدد التعديلات:", lblEditCount, "عدد مرات الطباعة:", lblPrintCount), 2, 0);
        pnlAudit.Controls.Add(_companyAuditCards);
        pnlAudit.ResumeLayout(true);
    }

    /// <summary>يضع جدول الشركات قبل بطاقات التدقيق لتبقى البطاقات أسفل الشاشة.</summary>
    private void MoveAuditBelowGrid()
    {
        if (pnlAudit.Parent == splitMain.Panel2)
            return;

        pnlAudit.Parent?.Controls.Remove(pnlAudit);
        pnlAudit.Dock = DockStyle.Bottom;
        splitMain.Panel2.Controls.Add(pnlAudit);
        pnlAudit.BringToFront();
        pnlDetails.AutoScroll = false;
    }

    private static Control CreateAuditCard(string title, string firstCaption, Label firstValue,
        string secondCaption, Label secondValue)
    {
        var card = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Margin = new Padding(4, 0, 4, 0),
            Padding = new Padding(6, 2, 6, 2),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));

        var header = new Label { Text = title, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 78, 121), TextAlign = ContentAlignment.MiddleRight };
        card.Controls.Add(header, 0, 0);
        card.SetColumnSpan(header, 2);
        AddAuditRow(card, 1, firstCaption, firstValue);
        AddAuditRow(card, 2, secondCaption, secondValue);
        return card;
    }

    private static void AddAuditRow(TableLayoutPanel card, int row, string caption, Label value)
    {
        var captionLabel = new Label { Text = caption, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };
        value.Dock = DockStyle.Fill;
        value.AutoEllipsis = true;
        value.TextAlign = ContentAlignment.MiddleRight;
        value.Font = new Font("Segoe UI", 8F);
        value.ForeColor = Color.FromArgb(30, 64, 95);
        value.BorderStyle = BorderStyle.FixedSingle;
        card.Controls.Add(captionLabel, 0, row);
        card.Controls.Add(value, 1, row);
    }

    /// <summary>
    /// يجعل حقول تعريف الشركة في بطاقة واحدة، كما في النموذج المعتمد: المجموعة
    /// والاسم والرمز والاسم الإنجليزي والنشاط والرقم الضريبي. أما التواصل فيبقى
    /// بطاقة ثانوية مستقلة كي لا يزاحم بيانات التعريف.
    /// </summary>
    private void ArrangePrimaryCompanyFields()
    {
        tblBasic.SuspendLayout();
        tblContact.SuspendLayout();

        tblBasic.Controls.Clear();
        tblBasic.RowStyles.Clear();
        tblBasic.RowCount = 4;
        for (var row = 0; row < 4; row++)
            tblBasic.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

        AddCompanyFieldRow(tblBasic, 0, label1, cmbGroups, label2, txtCompanyNameAr);
        AddCompanyFieldRow(tblBasic, 1, label3, txtCompanyNameEn, label4, txtCompanyPrefix);
        AddCompanyFieldRow(tblBasic, 2, label11, txtActivityType, label8, txtTaxNumber);
        tblBasic.Controls.Add(chkIsActive, 3, 3);
        tblBasic.SetColumnSpan(chkIsActive, 1);
        chkIsActive.Margin = new Padding(5, 9, 5, 9);

        tblContact.Controls.Clear();
        tblContact.RowStyles.Clear();
        tblContact.RowCount = 2;
        for (var row = 0; row < 2; row++)
            tblContact.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        AddCompanyFieldRow(tblContact, 0, label5, txtPhone, label10, txtMobile);
        AddCompanyFieldRow(tblContact, 1, label6, txtEmail, label7, txtAddress);
        tblContact.SetColumnSpan(txtAddress, 1);

        ApplyReferenceInputStyle(cmbGroups, txtCompanyNameAr, txtCompanyNameEn,
            txtCompanyPrefix, txtActivityType, txtTaxNumber, txtPhone, txtMobile,
            txtEmail, txtAddress);

        tblContact.ResumeLayout(true);
        tblBasic.ResumeLayout(true);
    }

    private static void AddCompanyFieldRow(TableLayoutPanel table, int row,
        Label firstCaption, Control firstField, Label secondCaption, Control secondField)
    {
        table.Controls.Add(firstCaption, 0, row);
        table.Controls.Add(firstField, 1, row);
        table.Controls.Add(secondCaption, 2, row);
        table.Controls.Add(secondField, 3, row);
    }

    private static void ApplyReferenceInputStyle(params Control[] controls)
    {
        foreach (Control control in controls)
        {
            control.BackColor = Color.FromArgb(255, 253, 231);
            control.ForeColor = Color.FromArgb(25, 45, 65);
        }
    }

    /// <summary>
    /// يعيد توزيع بطاقة الإدخال على نمط شاشة الشركات المرجعية: البيانات المهمة
    /// في الجانب الأيمن، والشعار في الجانب الأيسر، ثم بيانات التواصل أسفلها.
    /// تبقى قائمة الشركات والتدقيق موجودين لأنهما جزء من متطلبات نظام الطائر.
    /// </summary>
    private void ArrangeCompanyEntryLikeReference()
    {
        if (_companyEntryLayout is not null)
            return;

        pnlDetails.SuspendLayout();

        pnlDetails.Controls.Remove(grpBasic);
        pnlDetails.Controls.Remove(grpContact);
        pnlDetails.Controls.Remove(grpLogo);

        _companyEntryLayout = new TableLayoutPanel
        {
            Name = "tblCompanyEntry",
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(8),
            ColumnCount = 2,
            RowCount = 2,
            RightToLeft = RightToLeft.Yes
        };
        _companyEntryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F));
        _companyEntryLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
        _companyEntryLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 214F));
        _companyEntryLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        foreach (GroupBox card in new[] { grpBasic, grpContact, grpLogo })
        {
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(5);
            card.Padding = new Padding(12, 12, 12, 10);
        }

        grpContact.Text = "بيانات التواصل والإدارة (اختيارية)";
        grpLogo.Text = "شعار الشركة";

        _companyEntryLayout.Controls.Add(grpBasic, 0, 0);
        _companyEntryLayout.Controls.Add(grpContact, 0, 1);
        _companyEntryLayout.Controls.Add(grpLogo, 1, 0);
        _companyEntryLayout.SetRowSpan(grpLogo, 2);

        // الشعار منفصل وواضح مثل المرجع، مع أزرار صغيرة تحته.
        picCompanyLogo.Anchor = AnchorStyles.Top;
        picCompanyLogo.Location = new Point(42, 34);
        picCompanyLogo.Size = new Size(170, 118);
        label9.Visible = false;
        pnlLogoButtons.Anchor = AnchorStyles.Top;
        pnlLogoButtons.FlowDirection = FlowDirection.RightToLeft;
        pnlLogoButtons.AutoScroll = false;
        pnlLogoButtons.WrapContents = false;
        pnlLogoButtons.Location = new Point(22, 164);
        pnlLogoButtons.Size = new Size(210, 42);

        pnlDetails.Controls.Add(_companyEntryLayout);
        _companyEntryLayout.BringToFront();
        pnlAudit.BringToFront();
        pnlDetails.ResumeLayout(true);
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

        pnlListHeader.Height = 70;
        lblListTitle.Dock = DockStyle.Top;
        lblListTitle.TextAlign = ContentAlignment.MiddleRight;
        lblListTitle.Height = 26;
        if (_txtQuickSearch is not null)
        {
            _txtQuickSearch.Dock = DockStyle.Bottom;
            _txtQuickSearch.Height = 30;
            _txtQuickSearch.Margin = new Padding(4, 0, 4, 2);
        }

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

        pnlToolbar.Height = compact ? 46 : 52;

        Control.ControlCollection toolbarControls = _toolbarActions?.Controls ?? pnlToolbar.Controls;
        foreach (Control control in toolbarControls)
        {
            if (control is not Button button)
                continue;

            button.Width = compact ? 84 : 94;
            button.Height = compact ? 32 : 36;
            button.Margin = new Padding(3, 0, 3, 0);
        }

        int availableHeight = Math.Max(520, splitMain.Height);
        int tableHeight = compact ? 300 : 340;
        splitMain.SplitterDistance = Math.Max(300, availableHeight - tableHeight);

        if (_companyEntryLayout is not null)
            _companyEntryLayout.RowStyles[0].Height = compact ? 190 : 214;
        pnlAudit.Height = compact ? 72 : 78;
        pnlListHeader.Height = compact ? 66 : 70;

        AdjustLogoArea(compact);
        Invalidate(true);
    }

    private void ApplyRequestedToolbarColors()
    {
        var buttons = new[] { btnNew, btnSaveCompany, btnEdit, btnDelete, btnApprove, btnSearch, btnRefresh, btnPrint, btnClose };
        var colors = new[]
        {
            Color.FromArgb(13, 148, 136), Color.FromArgb(37, 99, 235), Color.FromArgb(217, 119, 6),
            Color.FromArgb(220, 38, 38), Color.FromArgb(5, 150, 105), Color.FromArgb(5, 150, 105),
            Color.FromArgb(2, 132, 199), Color.FromArgb(124, 58, 237), Color.FromArgb(100, 116, 139)
        };
        for (var index = 0; index < buttons.Length; index++)
        {
            var button = buttons[index];
            button.BackColor = colors[index];
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
        }
    }

    private void ApplyRequestedCardStyle()
    {
        pnlDetails.BackColor = Color.FromArgb(248, 250, 252);
        foreach (var card in new[] { grpBasic, grpContact, grpLogo })
        {
            card.BackColor = Color.White;
            card.ForeColor = Color.FromArgb(15, 23, 42);
            card.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }
        pnlAudit.BackColor = Color.FromArgb(241, 245, 249);
        dgvCompanies.BackgroundColor = Color.White;
        dgvCompanies.BorderStyle = BorderStyle.Fixed3D;
        dgvCompanies.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        dgvCompanies.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
        dgvCompanies.DefaultCellStyle.SelectionForeColor = Color.White;
    }

    private void AdjustLogoArea(bool compact)
    {
        picCompanyLogo.SizeMode = PictureBoxSizeMode.Zoom;
        picCompanyLogo.Width = compact ? 120 : 170;
        picCompanyLogo.Height = compact ? 84 : 118;
        picCompanyLogo.Left = Math.Max(12, (grpLogo.ClientSize.Width - picCompanyLogo.Width) / 2);

        btnBrowseLogo.Width = compact ? 92 : 100;
        btnRemoveLogo.Width = compact ? 92 : 100;
        btnBrowseLogo.Height = 30;
        btnRemoveLogo.Height = 30;
        pnlLogoButtons.Left = Math.Max(8, (grpLogo.ClientSize.Width - pnlLogoButtons.Width) / 2);
    }
}
