/*namespace AlTayerERP.Desktop.Forms;

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

*/

namespace AlTayerERP.Desktop.Forms
{
    partial class CountriesForm
    {
        // حاوية أجزاء مكونات النموذج (System.ComponentModel.IContainer)
        private System.ComponentModel.IContainer components = null;

        // دالة التخلص من الموارد غير المستخدمة لتخفيف العبء عن الذاكرة وإغلاق الأداة بسلاسة
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// دالة بناء وتنسيق عناصر الواجهة (InitializeComponent)
        /// يتم فيها تعريف وتهيئة وتنسيق كافة أدوات وشاشات نموذج الدول
        /// </summary>
        private void InitializeComponent()
        {
            // =========================================================================
            // 1. إنشاء الحاويات الرئيسية (Layout Containers & Panels)
            // =========================================================================

            // حاوية الجدول الرئيسي للنافذة المقسمة رأسياً إلى 5 صفوف مترابطة
            this.mainTableLayout = new System.Windows.Forms.TableLayoutPanel();

            // لوحة رصف الأزرار العلوية ذات التدفق التلقائي
            this.panelButtons = new System.Windows.Forms.FlowLayoutPanel();

            // =========================================================================
            // 2. تعريف أدوات أزرار العمليات العلوية (Toolbar Action Buttons)
            // =========================================================================
            this.btnNew = new System.Windows.Forms.Button();     // أداة زر: [+ جديد] لإنشاء سجل جديد
            this.btnSave = new System.Windows.Forms.Button();    // أداة زر: [✔ حفظ] لحفظ البيانات المدخلة
            this.btnEdit = new System.Windows.Forms.Button();    // أداة زر: [✎ تعديل] لتعديل السجل الحالي
            this.btnDeactivate = new System.Windows.Forms.Button(); // أداة زر: إيقاف أو إعادة تفعيل السجل الحالي
            this.btnCancel = new System.Windows.Forms.Button();  // أداة زر: [✖ إلغاء] للتراجع عن العمليات
            this.btnReset = new System.Windows.Forms.Button();   // أداة زر: [↺ إعادة] لتفريغ الحقول
            this.btnRefresh = new System.Windows.Forms.Button(); // أداة زر: [↻ تحديث] لجلب البيانات من القاعدة
            this.btnSearch = new System.Windows.Forms.Button();  // أداة زر: [🔍 بحث] لتفعيل نافذة البحث المتقدم
            this.btnPrint = new System.Windows.Forms.Button();   // أداة زر: [🖨 طباعة] لطباعة تقرير الدول
            this.btnClose = new System.Windows.Forms.Button();  // أداة زر: [🚪 إغلاق] لإغلاق شاشة الدول

            // =========================================================================
            // 3. تعريف بطاقة وحقول إدخال البيانات الأساسية للدولة (Data Entry Controls)
            // =========================================================================
            this.grpDataCard = new System.Windows.Forms.GroupBox();         // إطار (GroupBox) يحيد ببيانات الدولة الأساسية
            this.tblDataCard = new System.Windows.Forms.TableLayoutPanel();  // جدول شبكي (Table) لتنظيم حقول الإدخال بدقة

            // --- حقل: كود الدولة ---
            this.lblCountryCode = new System.Windows.Forms.Label();    // أداة عنوان النص: "كود الدولة *"
            this.txtCountryCode = new System.Windows.Forms.TextBox();  // أداة مربع نص إدخال: كود الدولة (مثل YE, SA)

            // --- حقل: الاسم بالعربية ---
            this.lblCountryNameAr = new System.Windows.Forms.Label();   // أداة عنوان النص: "الاسم بالعربية *"
            this.txtCountryNameAr = new System.Windows.Forms.TextBox(); // أداة مربع نص إدخال: اسم الدولة بالعربي

            // --- حقل: الاسم بالإنجليزي ---
            this.lblCountryNameEn = new System.Windows.Forms.Label();   // أداة عنوان النص: "الاسم بالإنجليزي"
            this.txtCountryNameEn = new System.Windows.Forms.TextBox(); // أداة مربع نص إدخال: اسم الدولة بالإنجليزي

            // --- حقل: مفتاح الاتصال الدولي ---
            this.lblPhoneKey = new System.Windows.Forms.Label();   // أداة عنوان النص: "مفتاح الاتصال"
            this.txtPhoneKey = new System.Windows.Forms.TextBox(); // أداة مربع نص إدخال: مفتاح الهاتف (مثل +967)

            // --- حقل: رمز ISO2 ---
            this.lblIso2 = new System.Windows.Forms.Label();   // أداة عنوان النص: "رمز ISO2"
            this.txtIso2 = new System.Windows.Forms.TextBox(); // أداة مربع نص إدخال: الرمز المكون من حرفين

            // --- حقل: رمز ISO3 ---
            this.lblIso3 = new System.Windows.Forms.Label();   // أداة عنوان النص: "رمز ISO3"
            this.txtIso3 = new System.Windows.Forms.TextBox(); // أداة مربع نص إدخال: الرمز المكون من 3 أحرف

            // --- حقل: العملة الرسمية ---
            this.lblCurrency = new System.Windows.Forms.Label();      // أداة عنوان النص: "العملة الرسمية"
            this.cmbCurrency = new System.Windows.Forms.ComboBox();   // أداة قائمة منسدلة لاختيار العملة (مثل YER, SAR)

            // --- حقل: اسم الجنسية بالعربية ---
            this.lblNationality = new System.Windows.Forms.Label();   // أداة عنوان النص: "اسم الجنسية بالعربية"
            this.txtNationality = new System.Windows.Forms.TextBox(); // أداة مربع نص لإدخال الجنسية المرتبطة بالدولة

            // --- حقل: ترتيب الظهور ---
            this.lblDisplayOrder = new System.Windows.Forms.Label();          // أداة عنوان النص: "ترتيب الظهور"
            this.numDisplayOrder = new System.Windows.Forms.NumericUpDown(); // أداة خانة أرقام تنازلية/تصاعدية للترتيب

            // --- حقل: الملاحظات ---
            this.lblNotes = new System.Windows.Forms.Label();   // أداة عنوان النص: "ملاحظات"
            this.txtNotes = new System.Windows.Forms.TextBox(); // أداة مربع نص ممتد لإدخال الملاحظات الإضافية

            // =========================================================================
            // 4. تعريف شريط وأدوات البحث والتصفية السريعة (Search & Filter Bar)
            // =========================================================================
            this.grpSearchContainer = new System.Windows.Forms.GroupBox();    // إطار حاوية شريط البحث
            this.pnlSearchFilter = new System.Windows.Forms.FlowLayoutPanel(); // لوحة رصف أدوات البحث أفقياً
            this.lblSearch = new System.Windows.Forms.Label();                // أداة عنوان النص: "بحث سريع"
            this.txtSearch = new System.Windows.Forms.TextBox();              // أداة مربع نص: إدخال كلمة للبحث في الجدول
            this.lblFilterStatus = new System.Windows.Forms.Label();          // أداة عنوان النص: "الحالة"
            this.cmbFilterStatus = new System.Windows.Forms.ComboBox();       // أداة قائمة منسدلة: تصفية حسب الحالة (نشط/غير نشط)
            this.btnApplyFilter = new System.Windows.Forms.Button();          // أداة زر: "تطبيق التصفية" لتنفيذ عملية الفرز

            // =========================================================================
            // 5. جدول عرض البيانات وبطاقات التدقيق والتذييل (Grid & Audit Footer)
            // =========================================================================
            this.dgvCountries = new System.Windows.Forms.DataGridView();      // أداة جدول عرض الدول (DataGridView)

            // بطاقات التدقيق والتذييل السفلي للنافذة
            this.tblAuditSummary = new System.Windows.Forms.TableLayoutPanel(); // جدول تنظيم بطاقات التدقيق (3 أعمدة)

            this.grpCreationData = new System.Windows.Forms.GroupBox();     // إطار: بطاقة بيانات الإنشاء
            this.lblCreatedBy = new System.Windows.Forms.Label();            // عنوان داخل البطاقة: اسم المستخدم المنشئ
            this.lblCreatedAt = new System.Windows.Forms.Label();            // عنوان داخل البطاقة: تاريخ ووقت الإنشاء

            this.grpModificationData = new System.Windows.Forms.GroupBox(); // إطار: بطاقة بيانات التعديل الأخير
            this.lblModifiedBy = new System.Windows.Forms.Label();          // عنوان داخل البطاقة: اسم آخر مُعدل
            this.lblModifiedAt = new System.Windows.Forms.Label();          // عنوان داخل البطاقة: تاريخ ووقت التعديل

            this.grpCounters = new System.Windows.Forms.GroupBox();         // إطار: بطاقة العدادات الإحصائية
            this.lblEditCount = new System.Windows.Forms.Label();           // عنوان داخل البطاقة: عدد مرات التعديل
            this.lblPrintCount = new System.Windows.Forms.Label();          // عنوان داخل البطاقة: عدد مرات الطباعة

            // بدء عملية تعليق التحديث البصري المؤقت أثناء البناء لزيادة السرعة
            this.mainTableLayout.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.grpDataCard.SuspendLayout();
            this.tblDataCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDisplayOrder)).BeginInit();
            this.grpSearchContainer.SuspendLayout();
            this.pnlSearchFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCountries)).BeginInit();
            this.tblAuditSummary.SuspendLayout();
            this.grpCreationData.SuspendLayout();
            this.grpModificationData.SuspendLayout();
            this.grpCounters.SuspendLayout();
            this.SuspendLayout();

            // -------------------------------------------------------------------------
            // [إعدادات وتنسيق الشاشة الرئيسية - Form Configuration]
            // -------------------------------------------------------------------------
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes; // ضبط اتجاه الواجهة بالكامل من اليمين إلى اليسار (عربي)
            this.RightToLeftLayout = true;
            this.ClientSize = new System.Drawing.Size(1080, 720);    // مساحة كافية للحقول والجدول وبطاقات التدقيق
            this.MinimumSize = new System.Drawing.Size(920, 650);    // يمنع قص حقول الإدخال والتذييل
            this.Text = "نظام الطائر السعيد - إدارة الدول";       // عنوان النافذة في الشريط العلوي
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252))))); // خلفية رمادي فاتح مريح
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            // -------------------------------------------------------------------------
            // [إعدادات الهيكل الرئيسي للنافذة - Main Table Layout]
            // تقسيم الشاشة رأسياً إلى 5 صفوف (أزرار، بيانات +1سم، بحث +1سم، جدول -2سم، تذييل)
            // -------------------------------------------------------------------------
            this.mainTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayout.ColumnCount = 1;
            this.mainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayout.RowCount = 5;
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));  // الصف 0: شريط الأزرار العلوية
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 285F)); // الصف 1: بطاقة البيانات الأساسية بما فيها الجنسية
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 85F));  // الصف 2: 🟢 إطار البحث والتصفية (+1 سم توسعة)
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)); // الصف 3: 🔴 جدول عرض الدول (تم تقليصه تلقائياً 2 سم)
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));  // الصف 4: بطاقات التدقيق والتذييل السفلي

            // -------------------------------------------------------------------------
            // [إعدادات وتنسيق لوحة الأزرار العلوية - Top Toolbar Panel]
            // -------------------------------------------------------------------------
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            // FlowDirection وحده يثبت أول زر عند أقصى اليمين داخل واجهة RTL.
            this.panelButtons.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelButtons.Padding = new System.Windows.Forms.Padding(8, 6, 8, 4);

            System.Windows.Forms.Button[] buttons = {
                btnNew, btnSave, btnEdit, btnDeactivate, btnPrint, btnSearch, btnRefresh, btnCancel, btnReset, btnClose
            };
            string[] titles = { "+ جديد", "✔ حفظ", "✎ تعديل", "إيقاف", "🖨 طباعة", "🔍 بحث", "↻ تحديث", "✖ إلغاء", "↺ إعادة", "🚪 إغلاق" };

            // تخصيص الألوان المعاصرة والحديثة (ERP Modern Palette) لكل زر على حدة
            System.Drawing.Color[] bgColors = {
                System.Drawing.Color.FromArgb(13, 148, 136),   // زر جديد (أخضر تركوازي Teal)
                System.Drawing.Color.FromArgb(37, 99, 235),    // زر حفظ (أزرق ملكي Royal Blue)
                System.Drawing.Color.FromArgb(217, 119, 6),    // زر تعديل (برتقالي دافئ Amber)
                System.Drawing.Color.FromArgb(220, 38, 38),    // زر إيقاف (أحمر)
                System.Drawing.Color.FromArgb(124, 58, 237),   // زر طباعة (بنفسجي غامق Purple)
                System.Drawing.Color.FromArgb(5, 150, 105),    // زر بحث (أخضر زمردي Emerald)
                System.Drawing.Color.FromArgb(2, 132, 199),    // زر تحديث (أزرق سماوي Sky Blue)
                System.Drawing.Color.FromArgb(100, 116, 139),  // زر إلغاء (رمادي Slate Gray)
                System.Drawing.Color.FromArgb(79, 70, 229),    // زر إعادة (بنفسجي Indigo)
                System.Drawing.Color.FromArgb(220, 38, 38)     // زر إغلاق (أحمر مميز Rose Red)
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Text = titles[i];
                buttons[i].Size = new System.Drawing.Size(86, 35);
                buttons[i].Margin = new System.Windows.Forms.Padding(3);
                buttons[i].FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                buttons[i].FlatAppearance.BorderSize = 0;
                buttons[i].BackColor = bgColors[i];
                buttons[i].ForeColor = System.Drawing.Color.White;
                buttons[i].Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
                buttons[i].Cursor = System.Windows.Forms.Cursors.Hand;
                this.panelButtons.Controls.Add(buttons[i]);
            }

            // -------------------------------------------------------------------------
            // [إعدادات وتنسيق بطاقة البيانات الأساسية للدولة - Data Card Layout]
            // -------------------------------------------------------------------------
            this.grpDataCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDataCard.Text = "بيانات الدولة الأساسية";
            this.grpDataCard.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpDataCard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.grpDataCard.Controls.Add(this.tblDataCard);

            this.tblDataCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblDataCard.Padding = new System.Windows.Forms.Padding(8, 8, 8, 6);

            // تقسيم شبكة الحقول إلى 5 أعمدة: (عنوان1 - حقل1 - عمود فاصل 35px - عنوان2 - حقل2) لمنع أي تزاحم
            this.tblDataCard.ColumnCount = 5;
            this.tblDataCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F)); // عناوين العمود الأول
            this.tblDataCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));  // حقول إدخال العمود الأول
            this.tblDataCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));  // 🟢 عمود فاصل مجوف لمنع الالتصاق
            this.tblDataCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F)); // عناوين العمود الثاني
            this.tblDataCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));  // حقول إدخال العمود الثاني
            this.tblDataCard.RowCount = 6;

            // إعطاء ارتفاع 42px لكل صف إدخال ليكون واسعاً ومريحاً جداً للمستخدم
            for (int r = 0; r < 6; r++)
                this.tblDataCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));

            // تنسيق جميع عناوين الحقول لتكون بارزة وغامقة (Bold Labels) وواضحة للعين
            System.Windows.Forms.Label[] labels = {
                lblCountryCode, lblCountryNameAr, lblCountryNameEn, lblPhoneKey,
                lblIso2, lblIso3, lblCurrency, lblDisplayOrder, lblNationality, lblNotes
            };
            foreach (var lbl in labels)
            {
                lbl.Dock = System.Windows.Forms.DockStyle.Fill;
                lbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight; // محاذاة النص لليمين
                lbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point); // خط غامق بارز
                lbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            }

            // ضبط هوامش مربعات النص والقوائم لتقليص أطوالها وجعلها متناسقة
            System.Windows.Forms.Control[] inputs = {
                txtCountryCode, txtCountryNameAr, txtCountryNameEn, txtPhoneKey,
                txtIso2, txtIso3, cmbCurrency, numDisplayOrder, txtNationality, txtNotes
            };
            foreach (var input in inputs)
            {
                input.Margin = new System.Windows.Forms.Padding(4, 4, 12, 4);
                input.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            }

            // --- تعبئة الصف الأول (0): كود الدولة + الاسم بالعربية ---
            lblCountryCode.Text = "كود الدولة *:";
            txtCountryCode.Dock = System.Windows.Forms.DockStyle.Fill;
            lblCountryNameAr.Text = "الاسم بالعربية *:";
            txtCountryNameAr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblDataCard.Controls.Add(lblCountryCode, 0, 0);   // إضافة عنوان كود الدولة
            this.tblDataCard.Controls.Add(txtCountryCode, 1, 0);   // إضافة حقل كود الدولة
            this.tblDataCard.Controls.Add(lblCountryNameAr, 3, 0); // إضافة عنوان الاسم بالعربية
            this.tblDataCard.Controls.Add(txtCountryNameAr, 4, 0); // إضافة حقل الاسم بالعربية

            // --- تعبئة الصف الثاني (1): الاسم بالإنجليزي + مفتاح الاتصال ---
            lblCountryNameEn.Text = "الاسم بالإنجليزي:";
            txtCountryNameEn.Dock = System.Windows.Forms.DockStyle.Fill;
            lblPhoneKey.Text = "مفتاح الاتصال:";
            txtPhoneKey.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblDataCard.Controls.Add(lblCountryNameEn, 0, 1); // إضافة عنوان الاسم بالإنجليزي
            this.tblDataCard.Controls.Add(txtCountryNameEn, 1, 1); // إضافة حقل الاسم بالإنجليزي
            this.tblDataCard.Controls.Add(lblPhoneKey, 3, 1);      // إضافة عنوان مفتاح الاتصال
            this.tblDataCard.Controls.Add(txtPhoneKey, 4, 1);     // إضافة حقل مفتاح الاتصال

            // --- تعبئة الصف الثالث (2): رمز ISO2 + رمز ISO3 ---
            lblIso2.Text = "رمز ISO2:";
            txtIso2.Dock = System.Windows.Forms.DockStyle.Fill;
            lblIso3.Text = "رمز ISO3:";
            txtIso3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblDataCard.Controls.Add(lblIso2, 0, 2);   // إضافة عنوان رمز ISO2
            this.tblDataCard.Controls.Add(txtIso2, 1, 2);   // إضافة حقل رمز ISO2
            this.tblDataCard.Controls.Add(lblIso3, 3, 2);   // إضافة عنوان رمز ISO3
            this.tblDataCard.Controls.Add(txtIso3, 4, 2);   // إضافة حقل رمز ISO3

            // --- تعبئة الصف الرابع (3): العملة الرسمية + ترتيب الظهور ---
            lblCurrency.Text = "العملة الرسمية:";
            cmbCurrency.Dock = System.Windows.Forms.DockStyle.Fill;
            lblDisplayOrder.Text = "ترتيب الظهور:";
            numDisplayOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblDataCard.Controls.Add(lblCurrency, 0, 3);     // إضافة عنوان العملة الرسمية
            this.tblDataCard.Controls.Add(cmbCurrency, 1, 3);     // إضافة قائمة العملة الرسمية
            this.tblDataCard.Controls.Add(lblDisplayOrder, 3, 3); // إضافة عنوان ترتيب الظهور
            this.tblDataCard.Controls.Add(numDisplayOrder, 4, 3); // إضافة حقل ترتيب الظهور

            // --- تعبئة الصف الخامس (4): اسم الجنسية بالعربية ---
            lblNationality.Text = "اسم الجنسية بالعربية:";
            txtNationality.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblDataCard.Controls.Add(lblNationality, 0, 4);
            this.tblDataCard.Controls.Add(txtNationality, 1, 4);
            this.tblDataCard.SetColumnSpan(txtNationality, 4);

            // --- تعبئة الصف السادس (5): الملاحظات (ممتدة أفقياً عبر 4 أعمدة) ---
            lblNotes.Text = "ملاحظات:";
            txtNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblDataCard.Controls.Add(lblNotes, 0, 5);  // إضافة عنوان الملاحظات
            this.tblDataCard.Controls.Add(txtNotes, 1, 5);  // إضافة حقل الملاحظات
            this.tblDataCard.SetColumnSpan(txtNotes, 4);    // مد مربع الملاحظات على عرض الأعمدة بالكامل

            // -------------------------------------------------------------------------
            // [إعدادات وتنسيق إطار البحث والتصفية - Search & Filter Bar]
            // -------------------------------------------------------------------------
            this.grpSearchContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSearchContainer.Text = "بيانات البحث والتصفية السريعة";
            this.grpSearchContainer.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpSearchContainer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.grpSearchContainer.Controls.Add(this.pnlSearchFilter);

            this.pnlSearchFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSearchFilter.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.pnlSearchFilter.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnlSearchFilter.Padding = new System.Windows.Forms.Padding(10, 10, 10, 6);

            this.pnlSearchFilter.Controls.Add(lblSearch);
            this.pnlSearchFilter.Controls.Add(txtSearch);
            this.pnlSearchFilter.Controls.Add(lblFilterStatus);
            this.pnlSearchFilter.Controls.Add(cmbFilterStatus);
            this.pnlSearchFilter.Controls.Add(btnApplyFilter);

            // أداة عنوان وحقل البحث السريع
            lblSearch.Text = "بحث سريع:";
            lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblSearch.Margin = new System.Windows.Forms.Padding(4, 6, 6, 0);

            txtSearch.Size = new System.Drawing.Size(220, 28); // صندوق نص البحث
            txtSearch.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtSearch.Margin = new System.Windows.Forms.Padding(0, 2, 20, 0);

            // أداة عنوان وقائمة تصفية الحالة
            lblFilterStatus.Text = "الحالة:";
            lblFilterStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblFilterStatus.Margin = new System.Windows.Forms.Padding(10, 6, 6, 0);

            cmbFilterStatus.Size = new System.Drawing.Size(130, 28); // قائمة تصفية الحالة (نشط/الكل)
            cmbFilterStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cmbFilterStatus.Margin = new System.Windows.Forms.Padding(0, 2, 20, 0);

            // أداة زر تطبيق الفرز والتصفية
            btnApplyFilter.Text = "تطبيق التصفية";
            btnApplyFilter.Size = new System.Drawing.Size(110, 30);
            btnApplyFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235))))); // أزرق مميز
            btnApplyFilter.ForeColor = System.Drawing.Color.White;
            btnApplyFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnApplyFilter.FlatAppearance.BorderSize = 0;
            btnApplyFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnApplyFilter.Cursor = System.Windows.Forms.Cursors.Hand;
            btnApplyFilter.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);

            // -------------------------------------------------------------------------
            // [إعدادات وتنسيق جدول عرض البيانات - DataGridView dgvCountries]
            // -------------------------------------------------------------------------
            this.dgvCountries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCountries.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill; // تمدد تلقائي للأعمدة
            this.dgvCountries.BackgroundColor = System.Drawing.Color.White;
            this.dgvCountries.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvCountries.ColumnHeadersHeight = 32; // ارتفاع رؤوس الأعمدة
            this.dgvCountries.RowTemplate.Height = 26;  // ارتفاع أسطر الجدول
            this.dgvCountries.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect; // تظليل السطر بالكامل عند النقر
            this.dgvCountries.MultiSelect = false;
            this.dgvCountries.ReadOnly = true;
            this.dgvCountries.AllowUserToAddRows = false;
            this.dgvCountries.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249))))); // ألوان صفوف متبادلة لسهولة القراءة
            this.dgvCountries.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));

            // -------------------------------------------------------------------------
            // [إعدادات وتنسيق بطاقات التدقيق والتذييل - Audit Summary Footer]
            // -------------------------------------------------------------------------
            this.tblAuditSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblAuditSummary.ColumnCount = 3;
            this.tblAuditSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F)); // عمود بيانات الإنشاء
            this.tblAuditSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F)); // عمود بيانات التعديل
            this.tblAuditSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F)); // عمود العدادات الإحصائية

            // --- بطاقة بيانات الإنشاء ---
            this.grpCreationData.Text = "بيانات الإنشاء";
            this.grpCreationData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCreationData.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.grpCreationData.Controls.Add(lblCreatedBy);
            this.grpCreationData.Controls.Add(lblCreatedAt);
            lblCreatedBy.Text = "أنشئ بواسطة: -"; lblCreatedBy.Dock = System.Windows.Forms.DockStyle.Top;
            lblCreatedAt.Text = "تاريخ الإنشاء: -"; lblCreatedAt.Dock = System.Windows.Forms.DockStyle.Top;

            // --- بطاقة بيانات التعديل ---
            this.grpModificationData.Text = "بيانات التعديل";
            this.grpModificationData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpModificationData.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.grpModificationData.Controls.Add(lblModifiedBy);
            this.grpModificationData.Controls.Add(lblModifiedAt);
            lblModifiedBy.Text = "عدل بواسطة: -"; lblModifiedBy.Dock = System.Windows.Forms.DockStyle.Top;
            lblModifiedAt.Text = "تاريخ التعديل: -"; lblModifiedAt.Dock = System.Windows.Forms.DockStyle.Top;

            // --- بطاقة العدادات الإحصائية ---
            this.grpCounters.Text = "العدادات";
            this.grpCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCounters.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.grpCounters.Controls.Add(lblEditCount);
            this.grpCounters.Controls.Add(lblPrintCount);
            lblEditCount.Text = "عدد التعديلات: 0"; lblEditCount.Dock = System.Windows.Forms.DockStyle.Top;
            lblPrintCount.Text = "عدد مرات الطباعة: 0"; lblPrintCount.Dock = System.Windows.Forms.DockStyle.Top;

            // إضافة البطاقات الثلاث إلى جدول التذييل السفلي
            this.tblAuditSummary.Controls.Add(this.grpCreationData, 0, 0);
            this.tblAuditSummary.Controls.Add(this.grpModificationData, 1, 0);
            this.tblAuditSummary.Controls.Add(this.grpCounters, 2, 0);

            // -------------------------------------------------------------------------
            // [التجميع النهائي لجميع أجزاء الواجهة - Assembly Controls]
            // -------------------------------------------------------------------------
            this.mainTableLayout.Controls.Add(this.panelButtons, 0, 0);        // السطر 0: إدراج لوحة الأزرار العلوية
            this.mainTableLayout.Controls.Add(this.grpDataCard, 0, 1);         // السطر 1: إدراج بطاقة البيانات الأساسية
            this.mainTableLayout.Controls.Add(this.grpSearchContainer, 0, 2); // السطر 2: إدراج إطار البحث والتصفية
            this.mainTableLayout.Controls.Add(this.dgvCountries, 0, 3);        // السطر 3: إدراج جدول العرض المتمدد
            this.mainTableLayout.Controls.Add(this.tblAuditSummary, 0, 4);    // السطر 4: إدراج بطاقات التدقيق والتذييل

            this.Controls.Add(this.mainTableLayout); // إضافة الجدول الرئيسي للنافذة

            // إنهاء عملية تعليق التحديث البصري وتفعيل الرسم النهائي
            this.mainTableLayout.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.grpDataCard.ResumeLayout(false);
            this.tblDataCard.ResumeLayout(false);
            this.tblDataCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDisplayOrder)).EndInit();
            this.grpSearchContainer.ResumeLayout(false);
            this.pnlSearchFilter.ResumeLayout(false);
            this.pnlSearchFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCountries)).EndInit();
            this.tblAuditSummary.ResumeLayout(false);
            this.grpCreationData.ResumeLayout(false);
            this.grpModificationData.ResumeLayout(false);
            this.grpCounters.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // =========================================================================
        // [تعريف متغيرات كافة أدوات الشاشة - Private Control Variables]
        // =========================================================================
        private System.Windows.Forms.TableLayoutPanel mainTableLayout; // الحاوية الرئيسية
        private System.Windows.Forms.FlowLayoutPanel panelButtons;     // لوحة الأزرار
        private System.Windows.Forms.Button btnNew, btnSave, btnEdit, btnDeactivate, btnCancel, btnReset, btnRefresh, btnSearch, btnPrint, btnClose;
        private System.Windows.Forms.GroupBox grpDataCard;             // إطار البيانات الأساسية
        private System.Windows.Forms.TableLayoutPanel tblDataCard;      // جدول حقول البيانات

        // أدوات العناوين وحقول الإدخال الخاصة بالدول
        private System.Windows.Forms.Label lblCountryCode, lblCountryNameAr, lblCountryNameEn, lblDisplayOrder, lblIso2, lblIso3, lblPhoneKey, lblCurrency, lblNationality, lblNotes;
        private System.Windows.Forms.TextBox txtCountryCode, txtCountryNameAr, txtCountryNameEn, txtIso2, txtIso3, txtPhoneKey, txtNationality, txtNotes;
        private System.Windows.Forms.NumericUpDown numDisplayOrder;
        private System.Windows.Forms.ComboBox cmbCurrency;

        // أدوات شريط البحث والتصفية
        private System.Windows.Forms.GroupBox grpSearchContainer;
        private System.Windows.Forms.FlowLayoutPanel pnlSearchFilter;
        private System.Windows.Forms.Label lblSearch, lblFilterStatus;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ComboBox cmbFilterStatus;
        private System.Windows.Forms.Button btnApplyFilter;

        // أدوات جدول العرض والتدقيق والتذييل
        private System.Windows.Forms.DataGridView dgvCountries;
        private System.Windows.Forms.TableLayoutPanel tblAuditSummary;
        private System.Windows.Forms.GroupBox grpCreationData, grpModificationData, grpCounters;
        private System.Windows.Forms.Label lblCreatedBy, lblCreatedAt, lblModifiedBy, lblModifiedAt, lblEditCount, lblPrintCount;
    }
}
