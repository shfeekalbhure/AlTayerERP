using AlTayerERP.Desktop.Common;

namespace AlTayerERP.Desktop;

partial class FrmGovernorates
{
    // حاوية المكونات والأدوات المرئية
    private System.ComponentModel.IContainer? components = null;

    // دالة التخلص من الموارد غير المستخدمة لإخلاء ذاكرة النظام عند إغلاق الشاشة
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// دالة بناء وتنسيق عناصر الواجهة لشاشة المحافظات (InitializeComponent)
    /// </summary>
    private void InitializeComponent()
    {
        // =========================================================================
        // 1. إنشاء وتعيين الحاويات الرئيسية (Layout Containers)
        // =========================================================================

        // الجدول الرئيسي المقسم رأسياً إلى 5 أسطر
        mainTableLayout = new TableLayoutPanel();

        // لوحة أزرار العمليات العلوية
        panelButtons = new FlowLayoutPanel();

        // أداة إطار وبطاقة البيانات الأساسية
        grpDataCard = new GroupBox();
        tblDataCard = new TableLayoutPanel();

        // أدوات الإدخال والعناوين الأساسية
        lblGovCode = new Label();
        txtGovCode = new TextBox();
        lblGovNameAr = new Label();
        txtGovNameAr = new TextBox();
        lblGovNameEn = new Label();
        txtGovNameEn = new TextBox();
        lblCountry = new Label();
        cmbCountry = new ComboBox();
        lblDisplayOrder = new Label();
        numDisplayOrder = new NumericUpDown();
        lblStatus = new Label();
        chkIsActive = new CheckBox();
        lblNotes = new Label();
        txtNotes = new TextBox();

        // أدوات شريط البحث والتصفية السريعة
        grpSearchContainer = new GroupBox();
        pnlSearchFilter = new FlowLayoutPanel();
        lblSearch = new Label();
        txtSearch = new TextBox();
        lblFilterCountry = new Label();
        cmbFilterCountry = new ComboBox();
        lblFilterStatus = new Label();
        cmbFilterStatus = new ComboBox();
        btnApplyFilter = new Button();

        // جدول العرض الرئيسي وبطاقات التذييل
        dgvGovernorates = new DataGridView();
        tblAuditSummary = new TableLayoutPanel();
        grpCreationData = new GroupBox();
        lblCreatedBy = new Label();
        lblCreatedAt = new Label();
        grpModificationData = new GroupBox();
        lblModifiedBy = new Label();
        lblModifiedAt = new Label();
        grpCounters = new GroupBox();
        lblEditCount = new Label();
        lblPrintCount = new Label();

        // =========================================================================
        // 2. إيقاف التحديث البصري المؤقت لتسريع عملية البناء
        // =========================================================================
        mainTableLayout.SuspendLayout();
        panelButtons.SuspendLayout();
        grpDataCard.SuspendLayout();
        tblDataCard.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numDisplayOrder).BeginInit();
        grpSearchContainer.SuspendLayout();
        pnlSearchFilter.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvGovernorates).BeginInit();
        tblAuditSummary.SuspendLayout();
        grpCreationData.SuspendLayout();
        grpModificationData.SuspendLayout();
        grpCounters.SuspendLayout();
        SuspendLayout();

        // -------------------------------------------------------------------------
        // [إعدادات الشاشة الرئيسية - Form Config]
        // -------------------------------------------------------------------------
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        ClientSize = new Size(1080, 720);
        MinimumSize = new Size(900, 620);
        Text = "نظام الطائر السعيد - إدارة المحافظات";
        BackColor = Color.FromArgb(248, 250, 252);
        Font = new Font("Segoe UI", 9F);

        // -------------------------------------------------------------------------
        // [تقسيم الهيكل الرئيسي - Main Layout Structure]
        // أزرار (48px) | بيانات (+1سم = 245px) | بحث (+1سم = 85px) | جدول (-2سم) | تذييل (75px)
        // -------------------------------------------------------------------------
        mainTableLayout.Dock = DockStyle.Fill;
        mainTableLayout.ColumnCount = 1;
        mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainTableLayout.RowCount = 5;
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));  // شريط الأزرار العلوية
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 252F)); // بطاقة البيانات الأساسية
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));  // إطار البحث والتصفية
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // جدول البيانات المتمدد (-2سم)
        mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));  // بطاقات التذييل والتدقيق

        // -------------------------------------------------------------------------
        // [تنسيق لوحة الأزرار العلوية - Top Action Buttons]
        // -------------------------------------------------------------------------
        panelButtons.Dock = DockStyle.Fill;
        // FlowDirection وحده يحدد موضع العناصر من أقصى اليمين؛
        // تفعيل RTL هنا يعكس ترتيب FlowLayout مرة أخرى عند العرض.
        panelButtons.RightToLeft = RightToLeft.No;
        panelButtons.FlowDirection = FlowDirection.RightToLeft;
        panelButtons.Padding = new Padding(8, 6, 8, 4);
        panelButtons.WrapContents = false;

        // الأزرار التسعة بالهوية الموحدة
        btnNew = new Button(); btnSave = new Button(); btnEdit = new Button(); btnDeactivate = new Button();
        btnCancel = new Button(); btnReset = new Button(); btnRefresh = new Button();
        btnSearch = new Button(); btnPrint = new Button(); btnClose = new Button();

        Button[] buttons = { btnNew, btnSave, btnEdit, btnDeactivate, btnPrint, btnSearch, btnRefresh, btnCancel, btnReset, btnClose };
        string[] titles = { "+ جديد", "✔ حفظ", "✎ تعديل", "⏸ إيقاف", "🖨 طباعة", "🔍 بحث", "↻ تحديث", "✖ إلغاء", "↺ إعادة", "🚪 إغلاق" };
        Color[] colors = {
            Color.FromArgb(13, 148, 136),  // جديد (تركوازي)
            Color.FromArgb(37, 99, 235),   // حفظ (أزرق)
            Color.FromArgb(217, 119, 6),   // تعديل (برتقالي)
            Color.FromArgb(220, 38, 38),   // إيقاف / إعادة تفعيل
            Color.FromArgb(124, 58, 237),  // طباعة (بنفسجي غامق)
            Color.FromArgb(5, 150, 105),   // بحث (أخضر)
            Color.FromArgb(2, 132, 199),   // تحديث (أزرق سماوي)
            Color.FromArgb(100, 116, 139), // إلغاء (رمادي)
            Color.FromArgb(79, 70, 229),   // إعادة (بنفسجي)
            Color.FromArgb(71, 85, 105)     // إغلاق (رمادي داكن)
        };

        for (var i = 0; i < buttons.Length; i++)
        {
            buttons[i].Text = titles[i];
            buttons[i].Size = new Size(102, 38);
            buttons[i].Margin = new Padding(3, 0, 3, 0);
            buttons[i].FlatStyle = FlatStyle.Flat;
            buttons[i].FlatAppearance.BorderSize = 0;
            buttons[i].BackColor = colors[i];
            buttons[i].ForeColor = Color.White;
            buttons[i].Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttons[i].Cursor = Cursors.Hand;
            panelButtons.Controls.Add(buttons[i]);
        }

        // -------------------------------------------------------------------------
        // [بطاقة البيانات الأساسية - Main Data Card]
        // -------------------------------------------------------------------------
        grpDataCard.Dock = DockStyle.Fill;
        grpDataCard.Text = "بيانات المحافظة الأساسية";
        grpDataCard.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grpDataCard.Controls.Add(tblDataCard);

        tblDataCard.Dock = DockStyle.Fill;
        tblDataCard.Padding = new Padding(12, 10, 12, 8);
        tblDataCard.ColumnCount = 5;
        tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 138F)); // عناوين العمود 1
        tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));  // حقول العمود 1
        tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));  // عمود فاصل
        tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 138F)); // عناوين العمود 2
        tblDataCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));  // حقول العمود 2
        tblDataCard.RowCount = 5;

        // توسعة ارتفاع الصفوف إلى 42px لتوفير مساحة مريحة للحقول
        for (var i = 0; i < 5; i++)
            tblDataCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));

        // عناوين عريضة وبارزة (Bold Labels)
        Label[] labels = { lblGovCode, lblGovNameAr, lblGovNameEn, lblCountry, lblDisplayOrder, lblStatus, lblNotes };
        foreach (var label in labels)
        {
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleRight;
            label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }

        // ضبط الهوامش والخطوط الموحدة للمدخلات
        Control[] inputs = { txtGovCode, txtGovNameAr, txtGovNameEn, cmbCountry, numDisplayOrder, chkIsActive, txtNotes };
        foreach (var input in inputs)
        {
            input.Margin = new Padding(8, 6, 8, 6);
            input.Font = new Font("Segoe UI", 9.5F);
        }

        // ضبط نصوص الحقول وتخصيص الملاحظات
        lblGovCode.Text = "كود المحافظة *:";
        lblGovNameAr.Text = "اسم المحافظة (عربي) *:";
        lblGovNameEn.Text = "اسم المحافظة (إنجليزي):";
        lblCountry.Text = "الدولة التابعة *:";
        lblDisplayOrder.Text = "ترتيب الظهور:";
        lblStatus.Text = "حالة المحافظة:";
        lblNotes.Text = "ملاحظات:";
        chkIsActive.Text = "محافظة نشطة";
        chkIsActive.Checked = true;
        chkIsActive.AutoSize = true;
        txtNotes.Multiline = true;

        // توزيع العناصر داخل الجدول الشبكي
        tblDataCard.Controls.Add(lblGovCode, 0, 0); tblDataCard.Controls.Add(txtGovCode, 1, 0);
        tblDataCard.Controls.Add(lblGovNameAr, 3, 0); tblDataCard.Controls.Add(txtGovNameAr, 4, 0);

        tblDataCard.Controls.Add(lblGovNameEn, 0, 1); tblDataCard.Controls.Add(txtGovNameEn, 1, 1);
        tblDataCard.Controls.Add(lblCountry, 3, 1); tblDataCard.Controls.Add(cmbCountry, 4, 1);

        tblDataCard.Controls.Add(lblDisplayOrder, 0, 2); tblDataCard.Controls.Add(numDisplayOrder, 1, 2);
        tblDataCard.Controls.Add(lblStatus, 3, 2); tblDataCard.Controls.Add(chkIsActive, 4, 2);

        tblDataCard.Controls.Add(lblNotes, 0, 3); tblDataCard.Controls.Add(txtNotes, 1, 3);
        tblDataCard.SetColumnSpan(txtNotes, 4);
        tblDataCard.SetRowSpan(txtNotes, 2);

        foreach (var input in new Control[] { txtGovCode, txtGovNameAr, txtGovNameEn, cmbCountry, numDisplayOrder, txtNotes })
            input.Dock = DockStyle.Fill;

        // -------------------------------------------------------------------------
        // [إطار البحث والتصفية السريعة - Search Bar Container]
        // -------------------------------------------------------------------------
        grpSearchContainer.Dock = DockStyle.Fill;
        grpSearchContainer.Text = "البحث والتصفية السريعة";
        grpSearchContainer.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpSearchContainer.Controls.Add(pnlSearchFilter);

        pnlSearchFilter.Dock = DockStyle.Fill;
        pnlSearchFilter.RightToLeft = RightToLeft.Yes;
        pnlSearchFilter.FlowDirection = FlowDirection.RightToLeft;
        pnlSearchFilter.Padding = new Padding(14, 12, 14, 8);

        lblSearch.Text = "بحث سريع:";
        lblFilterCountry.Text = "الدولة:";
        lblFilterStatus.Text = "الحالة:";

        foreach (var label in new[] { lblSearch, lblFilterCountry, lblFilterStatus })
        {
            label.AutoSize = true;
            label.Margin = new Padding(6, 6, 4, 0);
            label.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        txtSearch.Size = new Size(230, 32);
        cmbFilterCountry.Size = new Size(180, 32);
        cmbFilterStatus.Size = new Size(130, 32);

        btnApplyFilter.Text = "تطبيق التصفية";
        btnApplyFilter.Size = new Size(124, 32);
        btnApplyFilter.BackColor = Color.FromArgb(37, 99, 235);
        btnApplyFilter.ForeColor = Color.White;
        btnApplyFilter.FlatStyle = FlatStyle.Flat;
        btnApplyFilter.FlatAppearance.BorderSize = 0;

        pnlSearchFilter.Controls.AddRange(new Control[] {
            lblSearch, txtSearch, lblFilterCountry, cmbFilterCountry, lblFilterStatus, cmbFilterStatus, btnApplyFilter
        });

        // -------------------------------------------------------------------------
        // [جدول عرض البيانات - DataGridView dgvGovernorates]
        // -------------------------------------------------------------------------
        dgvGovernorates.Dock = DockStyle.Fill;
        dgvGovernorates.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvGovernorates.BackgroundColor = Color.White;
        dgvGovernorates.BorderStyle = BorderStyle.Fixed3D;
        dgvGovernorates.ColumnHeadersHeight = 32;
        dgvGovernorates.RowTemplate.Height = 26;
        dgvGovernorates.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvGovernorates.MultiSelect = false;
        dgvGovernorates.ReadOnly = true;
        dgvGovernorates.AllowUserToAddRows = false;
        dgvGovernorates.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        dgvGovernorates.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);

        // -------------------------------------------------------------------------
        // [بطاقات التدقيق والتذييل السفلي - Audit Summary]
        // -------------------------------------------------------------------------
        tblAuditSummary.Dock = DockStyle.Fill;
        tblAuditSummary.Padding = new Padding(8, 4, 8, 4);
        tblAuditSummary.ColumnCount = 3;
        tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        tblAuditSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

        ConfigureAuditGroup(grpCreationData, "بيانات الإنشاء", lblCreatedBy, "أنشئ بواسطة: -", lblCreatedAt, "تاريخ الإنشاء: -");
        ConfigureAuditGroup(grpModificationData, "بيانات التعديل", lblModifiedBy, "عدل بواسطة: -", lblModifiedAt, "تاريخ التعديل: -");
        ConfigureAuditGroup(grpCounters, "العدادات", lblEditCount, "عدد التعديلات: -", lblPrintCount, "عدد مرات الطباعة: -");

        tblAuditSummary.Controls.Add(grpCreationData, 0, 0);
        tblAuditSummary.Controls.Add(grpModificationData, 1, 0);
        tblAuditSummary.Controls.Add(grpCounters, 2, 0);

        // -------------------------------------------------------------------------
        // [تجميع المكونات النهائية داخل الجدول الرئيسي للنافذة]
        // -------------------------------------------------------------------------
        mainTableLayout.Controls.Add(panelButtons, 0, 0);
        mainTableLayout.Controls.Add(grpDataCard, 0, 1);
        mainTableLayout.Controls.Add(grpSearchContainer, 0, 2);
        mainTableLayout.Controls.Add(dgvGovernorates, 0, 3);
        mainTableLayout.Controls.Add(tblAuditSummary, 0, 4);
        Controls.Add(mainTableLayout);

        // إعادة تشغيل التحديث البصري وتطبيق الرسم
        mainTableLayout.ResumeLayout(false);
        panelButtons.ResumeLayout(false);
        grpDataCard.ResumeLayout(false);
        tblDataCard.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)numDisplayOrder).EndInit();
        grpSearchContainer.ResumeLayout(false);
        pnlSearchFilter.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvGovernorates).EndInit();
        tblAuditSummary.ResumeLayout(false);
        grpCreationData.ResumeLayout(false);
        grpModificationData.ResumeLayout(false);
        grpCounters.ResumeLayout(false);
        ResumeLayout(false);
    }

    /// <summary>
    /// دالة مساعدة لتنسيق وتجهيز بطاقات التدقيق والتذييل
    /// </summary>
    private static void ConfigureAuditGroup(GroupBox group, string title, Label first, string firstText, Label second, string secondText)
    {
        group.Text = title;
        group.Dock = DockStyle.Fill;
        group.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        group.Padding = new Padding(8, 4, 8, 4);
        first.Text = firstText;
        first.Dock = DockStyle.Top;
        first.Height = 30;
        first.TextAlign = ContentAlignment.MiddleRight;
        second.Text = secondText;
        second.Dock = DockStyle.Top;
        second.Height = 30;
        second.TextAlign = ContentAlignment.MiddleRight;
        group.Controls.Add(second);
        group.Controls.Add(first);
    }

    // =========================================================================
    // [تعريف كافة الأدوات والمتغيرات الخاصة بالشاشة]
    // =========================================================================
    private TableLayoutPanel mainTableLayout = null!;
    private FlowLayoutPanel panelButtons = null!;
    private Button btnNew = null!, btnSave = null!, btnEdit = null!, btnDeactivate = null!, btnCancel = null!, btnReset = null!, btnRefresh = null!, btnSearch = null!, btnPrint = null!, btnClose = null!;
    private GroupBox grpDataCard = null!;
    private TableLayoutPanel tblDataCard = null!;
    private Label lblGovCode = null!, lblGovNameAr = null!, lblGovNameEn = null!, lblCountry = null!, lblDisplayOrder = null!, lblStatus = null!, lblNotes = null!;
    private TextBox txtGovCode = null!, txtGovNameAr = null!, txtGovNameEn = null!, txtNotes = null!;
    private ComboBox cmbCountry = null!;
    private NumericUpDown numDisplayOrder = null!;
    private CheckBox chkIsActive = null!;
    private GroupBox grpSearchContainer = null!;
    private FlowLayoutPanel pnlSearchFilter = null!;
    private Label lblSearch = null!, lblFilterCountry = null!, lblFilterStatus = null!;
    private TextBox txtSearch = null!;
    private ComboBox cmbFilterCountry = null!, cmbFilterStatus = null!;
    private Button btnApplyFilter = null!;
    private DataGridView dgvGovernorates = null!;
    private TableLayoutPanel tblAuditSummary = null!;
    private GroupBox grpCreationData = null!, grpModificationData = null!, grpCounters = null!;
    private Label lblCreatedBy = null!, lblCreatedAt = null!, lblModifiedBy = null!, lblModifiedAt = null!, lblEditCount = null!, lblPrintCount = null!;

    #endregion

}
