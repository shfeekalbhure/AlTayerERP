using AlTayerERP.Desktop.Common;

namespace AlTayerERP.Desktop;

partial class FrmCities : BaseForm
{
    private System.ComponentModel.IContainer? components;
    protected override void Dispose(bool disposing) { if (disposing) components?.Dispose(); base.Dispose(disposing); }

    private void InitializeComponent()
    {
        mainTableLayout = new TableLayoutPanel(); panelButtons = new FlowLayoutPanel();
        btnNew = new Button(); btnSave = new Button(); btnEdit = new Button(); btnDeactivate = new Button(); btnPrint = new Button(); btnSearch = new Button(); btnRefresh = new Button(); btnCancel = new Button(); btnReset = new Button(); btnClose = new Button();
        grpDataCard = new GroupBox(); tblDataCard = new TableLayoutPanel();
        lblCityCode = new Label(); txtCityCode = new TextBox(); lblCityNameAr = new Label(); txtCityNameAr = new TextBox(); lblCityNameEn = new Label(); txtCityNameEn = new TextBox(); lblGovernorate = new Label(); cmbGovernorate = new ComboBox(); lblPostalCode = new Label(); txtPostalCode = new TextBox(); lblDisplayOrder = new Label(); numDisplayOrder = new NumericUpDown(); lblStatus = new Label(); chkIsActive = new CheckBox(); lblNotes = new Label(); txtNotes = new TextBox();
        grpSearchContainer = new GroupBox(); pnlSearchFilter = new TableLayoutPanel(); lblFilterStatus = new Label(); cmbFilterStatus = new ComboBox(); lblFilterGovernorate = new Label(); cmbFilterGovernorate = new ComboBox(); lblSearch = new Label(); txtSearch = new TextBox(); btnApplyFilter = new Button();
        dgvCities = new DataGridView(); tblAuditSummary = new TableLayoutPanel(); grpCreationData = new GroupBox(); grpModificationData = new GroupBox(); grpCounters = new GroupBox(); lblCreatedBy = new Label(); lblCreatedAt = new Label(); lblModifiedBy = new Label(); lblModifiedAt = new Label(); lblEditCount = new Label(); lblPrintCount = new Label();
        mainTableLayout.SuspendLayout(); panelButtons.SuspendLayout(); grpDataCard.SuspendLayout(); tblDataCard.SuspendLayout(); ((System.ComponentModel.ISupportInitialize)numDisplayOrder).BeginInit(); grpSearchContainer.SuspendLayout(); pnlSearchFilter.SuspendLayout(); ((System.ComponentModel.ISupportInitialize)dgvCities).BeginInit(); tblAuditSummary.SuspendLayout(); SuspendLayout();
        RightToLeft = System.Windows.Forms.RightToLeft.Yes; RightToLeftLayout = true; ClientSize = new Size(1001, 661); MinimumSize = new Size(741, 501); Text = "نظام الطائر السعيد - إدارة المدن"; BackColor = Color.FromArgb(248, 250, 252); Font = new Font("Segoe UI", 9F);

        mainTableLayout.Dock = DockStyle.Fill; mainTableLayout.ColumnCount = 1; mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); mainTableLayout.RowCount = 5;
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 271F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 81F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 97F));

        panelButtons.Dock = DockStyle.Fill; panelButtons.RightToLeft = System.Windows.Forms.RightToLeft.No; panelButtons.FlowDirection = FlowDirection.RightToLeft; panelButtons.Padding = new Padding(7, 7, 7, 5); panelButtons.WrapContents = false;
        Button[] buttons = { btnNew, btnSave, btnEdit, btnDeactivate, btnPrint, btnSearch, btnRefresh, btnCancel, btnReset, btnClose };
        string[] titles = { "+ جديد", "✔ حفظ", "✎ تعديل", "⏸ إيقاف", "🖨 طباعة", "🔍 بحث", "↻ تحديث", "✖ إلغاء", "↺ إعادة", "🚪 إغلاق" };
        Color[] colors = { Color.FromArgb(13, 148, 136), Color.FromArgb(37, 99, 235), Color.FromArgb(217, 119, 6), Color.FromArgb(220, 38, 38), Color.FromArgb(124, 58, 237), Color.FromArgb(5, 150, 105), Color.FromArgb(2, 132, 199), Color.FromArgb(100, 116, 139), Color.FromArgb(79, 70, 229), Color.FromArgb(71, 85, 105) };
        for (var i = 0; i < buttons.Length; i++) { buttons[i].Text = titles[i]; buttons[i].Size = new Size(92, 38); buttons[i].Margin = new Padding(3); buttons[i].FlatStyle = FlatStyle.Flat; buttons[i].FlatAppearance.BorderSize = 0; buttons[i].BackColor = colors[i]; buttons[i].ForeColor = Color.White; buttons[i].Font = new Font("Segoe UI", 9F, FontStyle.Bold); buttons[i].Cursor = Cursors.Hand; panelButtons.Controls.Add(buttons[i]); }

        grpDataCard.Dock = DockStyle.Fill; grpDataCard.Text = "بيانات المدينة الأساسية"; grpDataCard.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold); grpDataCard.Padding = new Padding(10, 19, 10, 7); grpDataCard.Controls.Add(tblDataCard);
        tblDataCard.Dock = DockStyle.Fill; tblDataCard.ColumnCount = 5; tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); tblDataCard.RowCount = 5; for (var i = 0; i < 5; i++) tblDataCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
        Label[] labels = { lblCityCode, lblCityNameAr, lblCityNameEn, lblGovernorate, lblPostalCode, lblDisplayOrder, lblStatus, lblNotes }; foreach (var label in labels) { label.Dock = DockStyle.Fill; label.TextAlign = ContentAlignment.MiddleRight; label.Font = new Font("Segoe UI", 9F, FontStyle.Bold); }
        lblCityCode.Text = "كود المدينة *:"; lblCityNameAr.Text = "اسم المدينة (عربي) *:"; lblCityNameEn.Text = "اسم المدينة (إنجليزي):"; lblGovernorate.Text = "المحافظة التابعة *:"; lblPostalCode.Text = "الرمز البريدي:"; lblDisplayOrder.Text = "ترتيب الظهور:"; lblStatus.Text = "حالة المدينة:"; lblNotes.Text = "ملاحظات:";
        Control[] inputs = { txtCityCode, txtCityNameAr, txtCityNameEn, cmbGovernorate, txtPostalCode, numDisplayOrder, chkIsActive, txtNotes }; foreach (var input in inputs) { input.Margin = new Padding(4, 5, 12, 4); input.Font = new Font("Segoe UI", 9F); }
        chkIsActive.Text = "مدينة نشطة"; chkIsActive.Checked = true; chkIsActive.AutoSize = true; txtNotes.Multiline = true; txtNotes.ScrollBars = ScrollBars.Vertical;
        tblDataCard.Controls.Add(lblCityCode, 0, 0); tblDataCard.Controls.Add(txtCityCode, 1, 0); tblDataCard.Controls.Add(lblCityNameAr, 3, 0); tblDataCard.Controls.Add(txtCityNameAr, 4, 0);
        tblDataCard.Controls.Add(lblCityNameEn, 0, 1); tblDataCard.Controls.Add(txtCityNameEn, 1, 1); tblDataCard.Controls.Add(lblGovernorate, 3, 1); tblDataCard.Controls.Add(cmbGovernorate, 4, 1);
        tblDataCard.Controls.Add(lblPostalCode, 0, 2); tblDataCard.Controls.Add(txtPostalCode, 1, 2); tblDataCard.Controls.Add(lblDisplayOrder, 3, 2); tblDataCard.Controls.Add(numDisplayOrder, 4, 2);
        tblDataCard.Controls.Add(lblStatus, 0, 3); tblDataCard.Controls.Add(chkIsActive, 1, 3); tblDataCard.Controls.Add(lblNotes, 3, 3); tblDataCard.Controls.Add(txtNotes, 4, 3); tblDataCard.SetRowSpan(txtNotes, 2);
        foreach (var input in new Control[] { txtCityCode, txtCityNameAr, txtCityNameEn, cmbGovernorate, txtPostalCode, numDisplayOrder, txtNotes }) input.Dock = DockStyle.Fill;

        grpSearchContainer.Dock = DockStyle.Fill; grpSearchContainer.Text = "البحث والتصفية السريعة"; grpSearchContainer.Font = new Font("Segoe UI", 9F, FontStyle.Bold); grpSearchContainer.Padding = new Padding(10, 19, 10, 7); grpSearchContainer.Controls.Add(pnlSearchFilter);
        pnlSearchFilter.Dock = DockStyle.Fill; pnlSearchFilter.ColumnCount = 7; pnlSearchFilter.RowCount = 1; pnlSearchFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F)); pnlSearchFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F)); pnlSearchFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 68F)); pnlSearchFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185F)); pnlSearchFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65F)); pnlSearchFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); pnlSearchFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F)); pnlSearchFilter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        lblFilterStatus.Text = "الحالة:"; lblFilterGovernorate.Text = "المحافظة:"; lblSearch.Text = "بحث:";
        foreach (var label in new[] { lblFilterStatus, lblFilterGovernorate, lblSearch }) { label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; label.Height = 32; label.Margin = new Padding(2, 2, 2, 0); label.TextAlign = ContentAlignment.MiddleRight; label.Font = new Font("Segoe UI", 9F, FontStyle.Bold); }
        foreach (var input in new Control[] { cmbFilterStatus, cmbFilterGovernorate, txtSearch }) { input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; input.Height = 30; input.Margin = new Padding(2, 3, 8, 0); }
        txtSearch.TextAlign = HorizontalAlignment.Right; btnApplyFilter.Text = "تطبيق"; btnApplyFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; btnApplyFilter.Height = 32; btnApplyFilter.Margin = new Padding(3, 2, 2, 0); btnApplyFilter.BackColor = Color.FromArgb(37, 99, 235); btnApplyFilter.ForeColor = Color.White; btnApplyFilter.FlatStyle = FlatStyle.Flat; btnApplyFilter.FlatAppearance.BorderSize = 0; btnApplyFilter.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        pnlSearchFilter.Controls.Add(lblFilterStatus, 0, 0); pnlSearchFilter.Controls.Add(cmbFilterStatus, 1, 0); pnlSearchFilter.Controls.Add(lblFilterGovernorate, 2, 0); pnlSearchFilter.Controls.Add(cmbFilterGovernorate, 3, 0); pnlSearchFilter.Controls.Add(lblSearch, 4, 0); pnlSearchFilter.Controls.Add(txtSearch, 5, 0); pnlSearchFilter.Controls.Add(btnApplyFilter, 6, 0);

        dgvCities.Dock = DockStyle.Fill; dgvCities.AutoGenerateColumns = false; dgvCities.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvCities.BackgroundColor = Color.White; dgvCities.BorderStyle = BorderStyle.Fixed3D; dgvCities.ColumnHeadersHeight = 32; dgvCities.RowTemplate.Height = 26; dgvCities.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgvCities.MultiSelect = false; dgvCities.ReadOnly = true; dgvCities.AllowUserToAddRows = false; dgvCities.AllowUserToDeleteRows = false; dgvCities.RowHeadersVisible = false; dgvCities.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249); dgvCities.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235); dgvCities.DefaultCellStyle.SelectionForeColor = Color.White;

        tblAuditSummary.Dock = DockStyle.Fill; tblAuditSummary.Padding = new Padding(8, 4, 8, 4); tblAuditSummary.ColumnCount = 3; tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        ConfigureAuditGroup(grpCreationData, "بيانات الإنشاء", lblCreatedBy, "أنشئ بواسطة: -", lblCreatedAt, "تاريخ الإنشاء: -"); ConfigureAuditGroup(grpModificationData, "بيانات التعديل", lblModifiedBy, "عدل بواسطة: -", lblModifiedAt, "تاريخ التعديل: -"); ConfigureAuditGroup(grpCounters, "العدادات", lblEditCount, "عدد التعديلات: -", lblPrintCount, "عدد مرات الطباعة: -");
        tblAuditSummary.Controls.Add(grpCreationData, 0, 0); tblAuditSummary.Controls.Add(grpModificationData, 1, 0); tblAuditSummary.Controls.Add(grpCounters, 2, 0);
        mainTableLayout.Controls.Add(panelButtons, 0, 0); mainTableLayout.Controls.Add(grpDataCard, 0, 1); mainTableLayout.Controls.Add(grpSearchContainer, 0, 2); mainTableLayout.Controls.Add(dgvCities, 0, 3); mainTableLayout.Controls.Add(tblAuditSummary, 0, 4); Controls.Add(mainTableLayout);
        mainTableLayout.ResumeLayout(false); panelButtons.ResumeLayout(false); grpDataCard.ResumeLayout(false); tblDataCard.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)numDisplayOrder).EndInit(); grpSearchContainer.ResumeLayout(false); pnlSearchFilter.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)dgvCities).EndInit(); tblAuditSummary.ResumeLayout(false); ResumeLayout(false);
    }

    private static void ConfigureAuditGroup(GroupBox group, string title, Label first, string firstText, Label second, string secondText)
    {
        group.Dock = DockStyle.Fill; group.Text = title; group.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold); group.Padding = new Padding(10, 18, 10, 3);
        var content = new TableLayoutPanel { Dock = DockStyle.Top, Height = 50, ColumnCount = 1, Padding = new Padding(2, 0, 2, 0) }; content.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
        foreach (var label in new[] { first, second }) { label.Dock = DockStyle.Fill; label.TextAlign = ContentAlignment.MiddleRight; label.Font = new Font("Segoe UI", 8.5F); }
        first.Text = firstText; second.Text = secondText; content.Controls.Add(first, 0, 0); content.Controls.Add(second, 0, 1); group.Controls.Add(content);
    }

    private TableLayoutPanel mainTableLayout = null!, tblDataCard = null!, pnlSearchFilter = null!, tblAuditSummary = null!; private FlowLayoutPanel panelButtons = null!;
    private Button btnNew = null!, btnSave = null!, btnEdit = null!, btnDeactivate = null!, btnPrint = null!, btnSearch = null!, btnRefresh = null!, btnCancel = null!, btnReset = null!, btnClose = null!, btnApplyFilter = null!;
    private GroupBox grpDataCard = null!, grpSearchContainer = null!, grpCreationData = null!, grpModificationData = null!, grpCounters = null!;
    private Label lblCityCode = null!, lblCityNameAr = null!, lblCityNameEn = null!, lblGovernorate = null!, lblPostalCode = null!, lblDisplayOrder = null!, lblStatus = null!, lblNotes = null!, lblSearch = null!, lblFilterGovernorate = null!, lblFilterStatus = null!, lblCreatedBy = null!, lblCreatedAt = null!, lblModifiedBy = null!, lblModifiedAt = null!, lblEditCount = null!, lblPrintCount = null!;
    private TextBox txtCityCode = null!, txtCityNameAr = null!, txtCityNameEn = null!, txtPostalCode = null!, txtNotes = null!, txtSearch = null!; private ComboBox cmbGovernorate = null!, cmbFilterGovernorate = null!, cmbFilterStatus = null!; private NumericUpDown numDisplayOrder = null!; private CheckBox chkIsActive = null!; private DataGridView dgvCities = null!;
}
