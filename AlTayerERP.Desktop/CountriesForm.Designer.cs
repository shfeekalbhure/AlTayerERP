namespace AlTayerERP.Desktop.Forms;

partial class CountriesForm
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
        btnNew = new Button(); btnSave = new Button(); btnEdit = new Button(); btnDeactivate = new Button(); btnReactivate = new Button(); btnCancel = new Button(); btnReset = new Button(); btnRefresh = new Button(); btnSearch = new Button(); btnPrint = new Button(); btnClose = new Button();
        grpDataCard = new GroupBox(); tblDataCard = new TableLayoutPanel();
        lblCountryCode = new Label(); txtCountryCode = new TextBox(); lblCountryNameAr = new Label(); txtCountryNameAr = new TextBox();
        lblCountryNameEn = new Label(); txtCountryNameEn = new TextBox(); lblDisplayOrder = new Label(); numDisplayOrder = new NumericUpDown();
        lblIso2 = new Label(); txtIso2 = new TextBox(); lblIso3 = new Label(); txtIso3 = new TextBox();
        lblPhoneKey = new Label(); txtPhoneKey = new TextBox(); lblCurrency = new Label(); cmbCurrency = new ComboBox(); lblNationality = new Label(); txtNationality = new TextBox();
        lblNotes = new Label(); txtNotes = new TextBox(); grpSearchFilter = new GroupBox(); pnlSearchFilter = new FlowLayoutPanel();
        lblSearch = new Label(); txtSearch = new TextBox(); lblFilterStatus = new Label(); cmbFilterStatus = new ComboBox(); btnApplyFilter = new Button();
        dgvCountries = new DataGridView(); tblAuditSummary = new TableLayoutPanel(); grpCreationData = new GroupBox(); lblCreatedBy = new Label(); lblCreatedAt = new Label();
        grpModificationData = new GroupBox(); lblModifiedBy = new Label(); lblModifiedAt = new Label(); grpCounters = new GroupBox(); lblEditCount = new Label(); lblPrintCount = new Label();
        ((System.ComponentModel.ISupportInitialize)numDisplayOrder).BeginInit(); ((System.ComponentModel.ISupportInitialize)dgvCountries).BeginInit(); SuspendLayout();

        RightToLeft = RightToLeft.Yes; RightToLeftLayout = true; ClientSize = new Size(1024, 720); MinimumSize = new Size(900, 650); Text = "نظام الطائر السعيد - إدارة الدول";
        mainTableLayout.Dock = DockStyle.Fill; mainTableLayout.ColumnCount = 1; mainTableLayout.RowCount = 5; mainTableLayout.Padding = new Padding(8); mainTableLayout.BackColor = Color.FromArgb(244, 247, 251);
        mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 215F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 95F));

        panelButtons.Dock = DockStyle.Fill; panelButtons.FlowDirection = FlowDirection.RightToLeft; panelButtons.WrapContents = false; panelButtons.Padding = new Padding(3); panelButtons.BackColor = Color.White;
        Button[] buttons = { btnNew, btnSave, btnEdit, btnDeactivate, btnReactivate, btnCancel, btnReset, btnRefresh, btnSearch, btnPrint, btnClose };
        string[] titles = { "جديد", "حفظ", "تعديل", "إيقاف", "إعادة تفعيل", "إلغاء", "إعادة", "تحديث", "بحث", "طباعة", "إغلاق" };
        for (int i = 0; i < buttons.Length; i++) { buttons[i].Text = titles[i]; buttons[i].Size = new Size(85, 34); buttons[i].Margin = new Padding(3); buttons[i].FlatStyle = FlatStyle.Flat; panelButtons.Controls.Add(buttons[i]); }

        grpDataCard.Dock = DockStyle.Fill; grpDataCard.Text = "بيانات الدولة"; grpDataCard.Controls.Add(tblDataCard);
        tblDataCard.Dock = DockStyle.Fill; tblDataCard.ColumnCount = 4; tblDataCard.RowCount = 6; tblDataCard.Padding = new Padding(8);
        tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        for (var r = 0; r < 6; r++) tblDataCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        AddDataRow(0, lblCountryCode, "كود الدولة *:", txtCountryCode, lblCountryNameAr, "الاسم بالعربية *:", txtCountryNameAr);
        AddDataRow(1, lblCountryNameEn, "الاسم بالإنجليزي:", txtCountryNameEn, lblDisplayOrder, "ترتيب الظهور:", numDisplayOrder);
        AddDataRow(2, lblIso2, "رمز ISO2:", txtIso2, lblIso3, "رمز ISO3:", txtIso3);
        AddDataRow(3, lblPhoneKey, "مفتاح الاتصال:", txtPhoneKey, lblCurrency, "العملة الرسمية:", cmbCurrency);
        lblNationality.Text = "اسم الجنسية بالعربية:"; ConfigureLabel(lblNationality); txtNationality.Dock = DockStyle.Fill; tblDataCard.Controls.Add(lblNationality, 0, 4); tblDataCard.Controls.Add(txtNationality, 1, 4); tblDataCard.SetColumnSpan(txtNationality, 3);
        lblNotes.Text = "ملاحظات:"; ConfigureLabel(lblNotes); txtNotes.Dock = DockStyle.Fill; txtNotes.Multiline = true; tblDataCard.Controls.Add(lblNotes, 0, 5); tblDataCard.Controls.Add(txtNotes, 1, 5); tblDataCard.SetColumnSpan(txtNotes, 3);

        grpSearchFilter.Dock = DockStyle.Fill; grpSearchFilter.Text = "البحث والتصفية"; grpSearchFilter.Controls.Add(pnlSearchFilter);
        pnlSearchFilter.Dock = DockStyle.Fill; pnlSearchFilter.FlowDirection = FlowDirection.RightToLeft; pnlSearchFilter.Padding = new Padding(6, 3, 6, 3);
        lblSearch.Text = "بحث سريع:"; ConfigureLabel(lblSearch); txtSearch.Size = new Size(220, 25); lblFilterStatus.Text = "الحالة:"; ConfigureLabel(lblFilterStatus); cmbFilterStatus.Size = new Size(130, 25); btnApplyFilter.Text = "تطبيق"; btnApplyFilter.Size = new Size(80, 26);
        pnlSearchFilter.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblFilterStatus, cmbFilterStatus, btnApplyFilter });

        dgvCountries.Dock = DockStyle.Fill; dgvCountries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dgvCountries.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize; dgvCountries.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgvCountries.MultiSelect = false; dgvCountries.ReadOnly = true; dgvCountries.AllowUserToAddRows = false; dgvCountries.AllowUserToDeleteRows = false; dgvCountries.RowHeadersVisible = false;

        tblAuditSummary.Dock = DockStyle.Fill; tblAuditSummary.ColumnCount = 3; tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        SetupAuditGroup(grpCreationData, "بيانات الإنشاء", lblCreatedBy, "أنشئ بواسطة: -", lblCreatedAt, "تاريخ الإنشاء: -"); SetupAuditGroup(grpModificationData, "بيانات التعديل", lblModifiedBy, "عدل بواسطة: -", lblModifiedAt, "تاريخ التعديل: -"); SetupAuditGroup(grpCounters, "العدادات", lblEditCount, "عدد التعديلات: 0", lblPrintCount, "عدد مرات الطباعة: 0");
        tblAuditSummary.Controls.Add(grpCreationData, 0, 0); tblAuditSummary.Controls.Add(grpModificationData, 1, 0); tblAuditSummary.Controls.Add(grpCounters, 2, 0);
        mainTableLayout.Controls.Add(panelButtons, 0, 0); mainTableLayout.Controls.Add(grpDataCard, 0, 1); mainTableLayout.Controls.Add(grpSearchFilter, 0, 2); mainTableLayout.Controls.Add(dgvCountries, 0, 3); mainTableLayout.Controls.Add(tblAuditSummary, 0, 4);
        Controls.Add(mainTableLayout); ((System.ComponentModel.ISupportInitialize)numDisplayOrder).EndInit(); ((System.ComponentModel.ISupportInitialize)dgvCountries).EndInit(); ResumeLayout(false); PerformLayout();
    }

    private void AddDataRow(int row, Label firstLabel, string firstText, Control firstInput, Label secondLabel, string secondText, Control secondInput)
    { firstLabel.Text = firstText; secondLabel.Text = secondText; ConfigureLabel(firstLabel); ConfigureLabel(secondLabel); firstInput.Dock = DockStyle.Fill; secondInput.Dock = DockStyle.Fill; tblDataCard.Controls.Add(firstLabel, 0, row); tblDataCard.Controls.Add(firstInput, 1, row); tblDataCard.Controls.Add(secondLabel, 2, row); tblDataCard.Controls.Add(secondInput, 3, row); }
    private static void ConfigureLabel(Label label) { label.Dock = DockStyle.Fill; label.TextAlign = ContentAlignment.MiddleRight; label.Font = new Font("Segoe UI", 9F, FontStyle.Bold); }
    private static void SetupAuditGroup(GroupBox box, string title, Label first, string firstText, Label second, string secondText) { box.Dock = DockStyle.Fill; box.Text = title; first.Text = firstText; second.Text = secondText; first.Dock = DockStyle.Top; second.Dock = DockStyle.Top; first.TextAlign = ContentAlignment.MiddleRight; second.TextAlign = ContentAlignment.MiddleRight; box.Controls.Add(second); box.Controls.Add(first); }

    private TableLayoutPanel mainTableLayout = null!; private FlowLayoutPanel panelButtons = null!; private Button btnNew = null!, btnSave = null!, btnEdit = null!, btnDeactivate = null!, btnReactivate = null!, btnCancel = null!, btnReset = null!, btnRefresh = null!, btnSearch = null!, btnPrint = null!, btnClose = null!;
    private GroupBox grpDataCard = null!; private TableLayoutPanel tblDataCard = null!; private Label lblCountryCode = null!, lblCountryNameAr = null!, lblCountryNameEn = null!, lblDisplayOrder = null!, lblIso2 = null!, lblIso3 = null!, lblPhoneKey = null!, lblCurrency = null!, lblNotes = null!;
    private TextBox txtCountryCode = null!, txtCountryNameAr = null!, txtCountryNameEn = null!, txtIso2 = null!, txtIso3 = null!, txtPhoneKey = null!, txtNationality = null!, txtNotes = null!; private Label lblNationality = null!; private NumericUpDown numDisplayOrder = null!; private ComboBox cmbCurrency = null!;
    private GroupBox grpSearchFilter = null!; private FlowLayoutPanel pnlSearchFilter = null!; private Label lblSearch = null!, lblFilterStatus = null!; private TextBox txtSearch = null!; private ComboBox cmbFilterStatus = null!; private Button btnApplyFilter = null!; private DataGridView dgvCountries = null!;
    private TableLayoutPanel tblAuditSummary = null!; private GroupBox grpCreationData = null!, grpModificationData = null!, grpCounters = null!; private Label lblCreatedBy = null!, lblCreatedAt = null!, lblModifiedBy = null!, lblModifiedAt = null!, lblEditCount = null!, lblPrintCount = null!;
}
