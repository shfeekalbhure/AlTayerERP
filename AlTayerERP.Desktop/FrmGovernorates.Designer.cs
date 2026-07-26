using AlTayerERP.Desktop.Common;

namespace AlTayerERP.Desktop;

partial class FrmGovernorates : BaseForm
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        mainTableLayout = new TableLayoutPanel(); panelButtons = new FlowLayoutPanel();
        btnNew = new Button(); btnSave = new Button(); btnEdit = new Button(); btnCancel = new Button(); btnReset = new Button(); btnRefresh = new Button(); btnSearch = new Button(); btnPrint = new Button(); btnClose = new Button();
        grpDataCard = new GroupBox(); tblDataCard = new TableLayoutPanel();
        lblGovCode = new Label(); txtGovCode = new TextBox(); lblGovNameAr = new Label(); txtGovNameAr = new TextBox(); lblGovNameEn = new Label(); txtGovNameEn = new TextBox(); lblCountry = new Label(); cmbCountry = new ComboBox(); lblDisplayOrder = new Label(); numDisplayOrder = new NumericUpDown(); lblStatus = new Label(); chkIsActive = new CheckBox(); lblNotes = new Label(); txtNotes = new TextBox();
        grpSearchContainer = new GroupBox(); pnlSearchFilter = new FlowLayoutPanel(); lblSearch = new Label(); txtSearch = new TextBox(); lblFilterCountry = new Label(); cmbFilterCountry = new ComboBox(); lblFilterStatus = new Label(); cmbFilterStatus = new ComboBox(); btnApplyFilter = new Button();
        dgvGovernorates = new DataGridView(); tblAuditSummary = new TableLayoutPanel(); grpCreationData = new GroupBox(); lblCreatedBy = new Label(); lblCreatedAt = new Label(); grpModificationData = new GroupBox(); lblModifiedBy = new Label(); lblModifiedAt = new Label(); grpCounters = new GroupBox(); lblEditCount = new Label(); lblPrintCount = new Label();
        mainTableLayout.SuspendLayout(); panelButtons.SuspendLayout(); grpDataCard.SuspendLayout(); tblDataCard.SuspendLayout(); ((System.ComponentModel.ISupportInitialize)numDisplayOrder).BeginInit(); grpSearchContainer.SuspendLayout(); pnlSearchFilter.SuspendLayout(); ((System.ComponentModel.ISupportInitialize)dgvGovernorates).BeginInit(); tblAuditSummary.SuspendLayout(); grpCreationData.SuspendLayout(); grpModificationData.SuspendLayout(); grpCounters.SuspendLayout(); SuspendLayout();

        RightToLeft = System.Windows.Forms.RightToLeft.Yes; RightToLeftLayout = true; ClientSize = new Size(1080, 720); MinimumSize = new Size(900, 620); Text = "نظام الطائر السعيد - إدارة المحافظات"; BackColor = Color.FromArgb(248, 250, 252); Font = new Font("Segoe UI", 9F);
        mainTableLayout.Dock = DockStyle.Fill; mainTableLayout.ColumnCount = 1; mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); mainTableLayout.RowCount = 5;
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 245F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));

        panelButtons.Dock = DockStyle.Fill; panelButtons.RightToLeft = System.Windows.Forms.RightToLeft.Yes; panelButtons.FlowDirection = FlowDirection.RightToLeft; panelButtons.Padding = new Padding(4); panelButtons.WrapContents = false;
        Button[] buttons = { btnNew, btnSave, btnEdit, btnCancel, btnReset, btnRefresh, btnSearch, btnPrint, btnClose };
        string[] titles = { "+ جديد", "✔ حفظ", "✎ تعديل", "✖ إلغاء", "↺ إعادة", "↻ تحديث", "🔍 بحث", "🖨 طباعة", "🚪 إغلاق" };
        Color[] colors = { Color.FromArgb(13, 148, 136), Color.FromArgb(37, 99, 235), Color.FromArgb(217, 119, 6), Color.FromArgb(100, 116, 139), Color.FromArgb(79, 70, 229), Color.FromArgb(2, 132, 199), Color.FromArgb(5, 150, 105), Color.FromArgb(124, 58, 237), Color.FromArgb(220, 38, 38) };
        for (var i = 0; i < buttons.Length; i++) { buttons[i].Text = titles[i]; buttons[i].Size = new Size(90, 35); buttons[i].Margin = new Padding(3); buttons[i].FlatStyle = FlatStyle.Flat; buttons[i].FlatAppearance.BorderSize = 0; buttons[i].BackColor = colors[i]; buttons[i].ForeColor = Color.White; buttons[i].Font = new Font("Segoe UI", 9F, FontStyle.Bold); buttons[i].Cursor = Cursors.Hand; panelButtons.Controls.Add(buttons[i]); }

        grpDataCard.Dock = DockStyle.Fill; grpDataCard.Text = "بيانات المحافظة الأساسية"; grpDataCard.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold); grpDataCard.Controls.Add(tblDataCard);
        tblDataCard.Dock = DockStyle.Fill; tblDataCard.Padding = new Padding(8); tblDataCard.ColumnCount = 5; tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); tblDataCard.RowCount = 5;
        for (var i = 0; i < 5; i++) tblDataCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        Label[] labels = { lblGovCode, lblGovNameAr, lblGovNameEn, lblCountry, lblDisplayOrder, lblStatus, lblNotes };
        foreach (var label in labels) { label.Dock = DockStyle.Fill; label.TextAlign = ContentAlignment.MiddleRight; label.Font = new Font("Segoe UI", 9F, FontStyle.Bold); }
        Control[] inputs = { txtGovCode, txtGovNameAr, txtGovNameEn, cmbCountry, numDisplayOrder, chkIsActive, txtNotes };
        foreach (var input in inputs) { input.Margin = new Padding(4, 4, 12, 4); input.Font = new Font("Segoe UI", 9F); }
        lblGovCode.Text = "كود المحافظة *:"; lblGovNameAr.Text = "اسم المحافظة (عربي) *:"; lblGovNameEn.Text = "اسم المحافظة (إنجليزي):"; lblCountry.Text = "الدولة التابعة *:"; lblDisplayOrder.Text = "ترتيب الظهور:"; lblStatus.Text = "حالة المحافظة:"; lblNotes.Text = "ملاحظات:"; chkIsActive.Text = "محافظة نشطة"; chkIsActive.Checked = true; chkIsActive.AutoSize = true; txtNotes.Multiline = true;
        tblDataCard.Controls.Add(lblGovCode, 0, 0); tblDataCard.Controls.Add(txtGovCode, 1, 0); tblDataCard.Controls.Add(lblGovNameAr, 3, 0); tblDataCard.Controls.Add(txtGovNameAr, 4, 0);
        tblDataCard.Controls.Add(lblGovNameEn, 0, 1); tblDataCard.Controls.Add(txtGovNameEn, 1, 1); tblDataCard.Controls.Add(lblCountry, 3, 1); tblDataCard.Controls.Add(cmbCountry, 4, 1);
        tblDataCard.Controls.Add(lblDisplayOrder, 0, 2); tblDataCard.Controls.Add(numDisplayOrder, 1, 2); tblDataCard.Controls.Add(lblStatus, 3, 2); tblDataCard.Controls.Add(chkIsActive, 4, 2);
        tblDataCard.Controls.Add(lblNotes, 0, 3); tblDataCard.Controls.Add(txtNotes, 1, 3); tblDataCard.SetColumnSpan(txtNotes, 4); tblDataCard.SetRowSpan(txtNotes, 2);
        foreach (var input in new Control[] { txtGovCode, txtGovNameAr, txtGovNameEn, cmbCountry, numDisplayOrder, txtNotes }) input.Dock = DockStyle.Fill;

        grpSearchContainer.Dock = DockStyle.Fill; grpSearchContainer.Text = "البحث والتصفية السريعة"; grpSearchContainer.Font = new Font("Segoe UI", 9F, FontStyle.Bold); grpSearchContainer.Controls.Add(pnlSearchFilter);
        pnlSearchFilter.Dock = DockStyle.Fill; pnlSearchFilter.RightToLeft = System.Windows.Forms.RightToLeft.Yes; pnlSearchFilter.FlowDirection = FlowDirection.RightToLeft; pnlSearchFilter.Padding = new Padding(10, 10, 10, 6);
        lblSearch.Text = "بحث سريع:"; lblFilterCountry.Text = "الدولة:"; lblFilterStatus.Text = "الحالة:";
        foreach (var label in new[] { lblSearch, lblFilterCountry, lblFilterStatus }) { label.AutoSize = true; label.Margin = new Padding(6, 6, 4, 0); label.Font = new Font("Segoe UI", 9F, FontStyle.Bold); }
        txtSearch.Size = new Size(180, 28); cmbFilterCountry.Size = new Size(150, 28); cmbFilterStatus.Size = new Size(110, 28); btnApplyFilter.Text = "تطبيق التصفية"; btnApplyFilter.Size = new Size(110, 30); btnApplyFilter.BackColor = Color.FromArgb(37, 99, 235); btnApplyFilter.ForeColor = Color.White; btnApplyFilter.FlatStyle = FlatStyle.Flat; btnApplyFilter.FlatAppearance.BorderSize = 0;
        pnlSearchFilter.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblFilterCountry, cmbFilterCountry, lblFilterStatus, cmbFilterStatus, btnApplyFilter });

        dgvGovernorates.Dock = DockStyle.Fill; dgvGovernorates.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvGovernorates.BackgroundColor = Color.White; dgvGovernorates.BorderStyle = BorderStyle.Fixed3D; dgvGovernorates.ColumnHeadersHeight = 32; dgvGovernorates.RowTemplate.Height = 26; dgvGovernorates.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgvGovernorates.MultiSelect = false; dgvGovernorates.ReadOnly = true; dgvGovernorates.AllowUserToAddRows = false; dgvGovernorates.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249); dgvGovernorates.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);

        tblAuditSummary.Dock = DockStyle.Fill; tblAuditSummary.ColumnCount = 3; tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        ConfigureAuditGroup(grpCreationData, "بيانات الإنشاء", lblCreatedBy, "أنشئ بواسطة: -", lblCreatedAt, "تاريخ الإنشاء: -"); ConfigureAuditGroup(grpModificationData, "بيانات التعديل", lblModifiedBy, "عدل بواسطة: -", lblModifiedAt, "تاريخ التعديل: -"); ConfigureAuditGroup(grpCounters, "العدادات", lblEditCount, "عدد التعديلات: -", lblPrintCount, "عدد مرات الطباعة: -");
        tblAuditSummary.Controls.Add(grpCreationData, 0, 0); tblAuditSummary.Controls.Add(grpModificationData, 1, 0); tblAuditSummary.Controls.Add(grpCounters, 2, 0);
        mainTableLayout.Controls.Add(panelButtons, 0, 0); mainTableLayout.Controls.Add(grpDataCard, 0, 1); mainTableLayout.Controls.Add(grpSearchContainer, 0, 2); mainTableLayout.Controls.Add(dgvGovernorates, 0, 3); mainTableLayout.Controls.Add(tblAuditSummary, 0, 4); Controls.Add(mainTableLayout);
        mainTableLayout.ResumeLayout(false); panelButtons.ResumeLayout(false); grpDataCard.ResumeLayout(false); tblDataCard.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)numDisplayOrder).EndInit(); grpSearchContainer.ResumeLayout(false); pnlSearchFilter.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)dgvGovernorates).EndInit(); tblAuditSummary.ResumeLayout(false); grpCreationData.ResumeLayout(false); grpModificationData.ResumeLayout(false); grpCounters.ResumeLayout(false); ResumeLayout(false);
    }

    private static void ConfigureAuditGroup(GroupBox group, string title, Label first, string firstText, Label second, string secondText)
    {
        group.Text = title; group.Dock = DockStyle.Fill; group.Font = new Font("Segoe UI", 8F); first.Text = firstText; first.Dock = DockStyle.Top; second.Text = secondText; second.Dock = DockStyle.Top; group.Controls.Add(second); group.Controls.Add(first);
    }

    private TableLayoutPanel mainTableLayout = null!; private FlowLayoutPanel panelButtons = null!;
    private Button btnNew = null!, btnSave = null!, btnEdit = null!, btnCancel = null!, btnReset = null!, btnRefresh = null!, btnSearch = null!, btnPrint = null!, btnClose = null!;
    private GroupBox grpDataCard = null!; private TableLayoutPanel tblDataCard = null!; private Label lblGovCode = null!, lblGovNameAr = null!, lblGovNameEn = null!, lblCountry = null!, lblDisplayOrder = null!, lblStatus = null!, lblNotes = null!; private TextBox txtGovCode = null!, txtGovNameAr = null!, txtGovNameEn = null!, txtNotes = null!; private ComboBox cmbCountry = null!; private NumericUpDown numDisplayOrder = null!; private CheckBox chkIsActive = null!;
    private GroupBox grpSearchContainer = null!; private FlowLayoutPanel pnlSearchFilter = null!; private Label lblSearch = null!, lblFilterCountry = null!, lblFilterStatus = null!; private TextBox txtSearch = null!; private ComboBox cmbFilterCountry = null!, cmbFilterStatus = null!; private Button btnApplyFilter = null!;
    private DataGridView dgvGovernorates = null!; private TableLayoutPanel tblAuditSummary = null!; private GroupBox grpCreationData = null!, grpModificationData = null!, grpCounters = null!; private Label lblCreatedBy = null!, lblCreatedAt = null!, lblModifiedBy = null!, lblModifiedAt = null!, lblEditCount = null!, lblPrintCount = null!;
}
