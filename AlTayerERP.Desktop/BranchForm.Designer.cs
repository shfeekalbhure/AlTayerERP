using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    // فئة جزئية مخصصة لإعداد الأشكال البرمجية والمرئية لعناصر الشاشة تلقائياً
    partial class BranchForm
    {
        /// <summary>
        /// متغير المفسر الافتراضي المسؤول عن إدارة مكونات الشاشة وتنظيف الذاكرة
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// دالة تنظيف وتفريغ الذاكرة المؤقتة من أدوات الشاشة عند إغلاقها لتسريع النظام
        /// </summary>
        /// <param name="disposing">true إذا كان يجب تدمير المكونات المحقونة، وإلا false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose(); // تدمير المكونات النشطة
            }
            base.Dispose(disposing); // استدعاء التدمير الأساسي للفورم
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// الدالة الجوهرية لبناء وتجهيز عناصر الواجهة (المواقع، الأحجام، الخطوط، والأحداث)
        /// يتم توليدها وإدارتها تلقائياً عبر محرك الفيجوال ستوديو
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            cmbCompanies = new ComboBox();
            txtBranchNameAr = new TextBox();
            label1 = new Label();
            label2 = new Label();
            btnSaveBranch = new Button();
            txtBranchNameEn = new TextBox();
            label3 = new Label();
            cmbBranchType = new ComboBox();
            label4 = new Label();
            txtLocation = new TextBox();
            label5 = new Label();
            chkAllowCredit = new CheckBox();
            cmbParentBranch = new ComboBox();
            label6 = new Label();
            chkAllowPercentage = new CheckBox();
            label7 = new Label();
            txtMobile = new TextBox();
            cmbManager = new ComboBox();
            label8 = new Label();
            btnUnApprove = new Button();
            btnApprove = new Button();
            btnRefresh = new Button();
            btnPreview = new Button();
            btnSearch = new Button();
            btnImport = new Button();
            btnExport = new Button();
            btnDelete = new Button();
            btnClose = new Button();
            btnEdit = new Button();
            btnPrint = new Button();
            btnNew = new Button();
            dgvBranches = new DataGridView();
            cmbStatus = new ComboBox();
            label9 = new Label();
            txtPhone = new TextBox();
            label10 = new Label();
            txtWebsite = new TextBox();
            label11 = new Label();
            txtEmail = new TextBox();
            label12 = new Label();
            txtNotes = new TextBox();
            label13 = new Label();
            txtBranchCode = new TextBox();
            label14 = new Label();
            cmbCity = new ComboBox();
            label15 = new Label();
            pnlTopBar = new Panel();
            btnLogout = new Button();
            btnAboutSystem = new Button();
            btnSettings = new Button();
            btnNotifications = new Button();
            picCompanyLogo = new PictureBox();
            lblCurrentUser = new Label();
            lblCompanyName = new Label();
            lblFiscalYear = new Label();
            lblCurrentBranch = new Label();
            ((ISupportInitialize)dgvBranches).BeginInit();
            pnlTopBar.SuspendLayout();
            ((ISupportInitialize)picCompanyLogo).BeginInit();
            SuspendLayout();
            // 
            // cmbCompanies
            // 
            cmbCompanies.BackColor = Color.White;
            cmbCompanies.FlatStyle = FlatStyle.Flat;
            cmbCompanies.FormattingEnabled = true;
            cmbCompanies.Location = new Point(227, 99);
            cmbCompanies.Margin = new Padding(4, 6, 4, 6);
            cmbCompanies.Name = "cmbCompanies";
            cmbCompanies.Size = new Size(151, 33);
            cmbCompanies.TabIndex = 0;
            // 
            // txtBranchNameAr
            // 
            txtBranchNameAr.BackColor = Color.White;
            txtBranchNameAr.BorderStyle = BorderStyle.FixedSingle;
            txtBranchNameAr.Location = new Point(227, 170);
            txtBranchNameAr.Margin = new Padding(4, 6, 4, 6);
            txtBranchNameAr.Name = "txtBranchNameAr";
            txtBranchNameAr.Size = new Size(125, 31);
            txtBranchNameAr.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoEllipsis = true;
            label1.AutoSize = true;
            label1.Location = new Point(71, 99);
            label1.Name = "label1";
            label1.Size = new Size(150, 25);
            label1.TabIndex = 2;
            label1.Text = "اسم الشركة التابعة";
            // 
            // label2
            // 
            label2.AutoEllipsis = true;
            label2.AutoSize = true;
            label2.Location = new Point(134, 173);
            label2.Name = "label2";
            label2.Size = new Size(87, 25);
            label2.TabIndex = 2;
            label2.Text = "اسم الفرع";
            // 
            // btnSaveBranch
            // 
            btnSaveBranch.BackColor = Color.FromArgb(15, 103, 208);
            btnSaveBranch.FlatAppearance.BorderColor = Color.FromArgb(15, 103, 208);
            btnSaveBranch.FlatStyle = FlatStyle.Flat;
            btnSaveBranch.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnSaveBranch.ForeColor = Color.White;
            btnSaveBranch.Location = new Point(273, 501);
            btnSaveBranch.Name = "btnSaveBranch";
            btnSaveBranch.Padding = new Padding(6, 0, 6, 0);
            btnSaveBranch.Size = new Size(94, 34);
            btnSaveBranch.TabIndex = 3;
            btnSaveBranch.Text = "حفظ";
            btnSaveBranch.UseVisualStyleBackColor = false;
            btnSaveBranch.Click += btnSaveBranch_Click;
            // 
            // txtBranchNameEn
            // 
            txtBranchNameEn.BackColor = Color.White;
            txtBranchNameEn.BorderStyle = BorderStyle.FixedSingle;
            txtBranchNameEn.Location = new Point(228, 204);
            txtBranchNameEn.Margin = new Padding(4, 6, 4, 6);
            txtBranchNameEn.Name = "txtBranchNameEn";
            txtBranchNameEn.Size = new Size(125, 31);
            txtBranchNameEn.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoEllipsis = true;
            label3.AutoSize = true;
            label3.Location = new Point(82, 204);
            label3.Name = "label3";
            label3.Size = new Size(140, 25);
            label3.TabIndex = 2;
            label3.Text = "اسم الفرع انجلزي";
            // 
            // cmbBranchType
            // 
            cmbBranchType.BackColor = Color.White;
            cmbBranchType.FlatStyle = FlatStyle.Flat;
            cmbBranchType.FormattingEnabled = true;
            cmbBranchType.Location = new Point(223, 275);
            cmbBranchType.Margin = new Padding(4, 6, 4, 6);
            cmbBranchType.Name = "cmbBranchType";
            cmbBranchType.Size = new Size(151, 33);
            cmbBranchType.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoEllipsis = true;
            label4.AutoSize = true;
            label4.Location = new Point(135, 275);
            label4.Name = "label4";
            label4.Size = new Size(82, 25);
            label4.TabIndex = 2;
            label4.Text = "نوع الفرع";
            // 
            // txtLocation
            // 
            txtLocation.BackColor = Color.White;
            txtLocation.BorderStyle = BorderStyle.FixedSingle;
            txtLocation.Location = new Point(228, 240);
            txtLocation.Margin = new Padding(4, 6, 4, 6);
            txtLocation.Name = "txtLocation";
            txtLocation.Size = new Size(125, 31);
            txtLocation.TabIndex = 1;
            // 
            // label5
            // 
            label5.AutoEllipsis = true;
            label5.AutoSize = true;
            label5.Location = new Point(71, 243);
            label5.Name = "label5";
            label5.Size = new Size(156, 25);
            label5.TabIndex = 2;
            label5.Text = "موقع / عنوان الفرع";
            // 
            // chkAllowCredit
            // 
            chkAllowCredit.AutoSize = true;
            chkAllowCredit.Location = new Point(749, 157);
            chkAllowCredit.Name = "chkAllowCredit";
            chkAllowCredit.Size = new Size(137, 29);
            chkAllowCredit.TabIndex = 4;
            chkAllowCredit.Text = "السماح بالاجل";
            chkAllowCredit.UseVisualStyleBackColor = true;
            chkAllowCredit.CheckedChanged += chkAllowCredit_CheckedChanged;
            // 
            // cmbParentBranch
            // 
            cmbParentBranch.BackColor = Color.White;
            cmbParentBranch.FlatStyle = FlatStyle.Flat;
            cmbParentBranch.FormattingEnabled = true;
            cmbParentBranch.Location = new Point(228, 312);
            cmbParentBranch.Margin = new Padding(4, 6, 4, 6);
            cmbParentBranch.Name = "cmbParentBranch";
            cmbParentBranch.Size = new Size(151, 33);
            cmbParentBranch.TabIndex = 0;
            // 
            // label6
            // 
            label6.AutoEllipsis = true;
            label6.AutoSize = true;
            label6.Location = new Point(140, 309);
            label6.Name = "label6";
            label6.Size = new Size(73, 25);
            label6.TabIndex = 2;
            label6.Text = "تابع فرع";
            // 
            // chkAllowPercentage
            // 
            chkAllowPercentage.AutoSize = true;
            chkAllowPercentage.Location = new Point(749, 186);
            chkAllowPercentage.Name = "chkAllowPercentage";
            chkAllowPercentage.Size = new Size(145, 29);
            chkAllowPercentage.TabIndex = 4;
            chkAllowPercentage.Text = "السماح بالنسبة";
            chkAllowPercentage.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.AutoEllipsis = true;
            label7.AutoSize = true;
            label7.Location = new Point(646, 362);
            label7.Name = "label7";
            label7.Size = new Size(132, 25);
            label7.TabIndex = 6;
            label7.Text = "البريد الالكتروني";
            // 
            // txtMobile
            // 
            txtMobile.BackColor = Color.White;
            txtMobile.BorderStyle = BorderStyle.FixedSingle;
            txtMobile.Location = new Point(784, 286);
            txtMobile.Margin = new Padding(4, 6, 4, 6);
            txtMobile.Name = "txtMobile";
            txtMobile.Size = new Size(125, 31);
            txtMobile.TabIndex = 5;
            // 
            // cmbManager
            // 
            cmbManager.BackColor = Color.White;
            cmbManager.FlatStyle = FlatStyle.Flat;
            cmbManager.FormattingEnabled = true;
            cmbManager.Location = new Point(756, 215);
            cmbManager.Margin = new Padding(4, 6, 4, 6);
            cmbManager.Name = "cmbManager";
            cmbManager.Size = new Size(151, 33);
            cmbManager.TabIndex = 0;
            // 
            // label8
            // 
            label8.AutoEllipsis = true;
            label8.AutoSize = true;
            label8.Location = new Point(574, 218);
            label8.Name = "label8";
            label8.Size = new Size(176, 25);
            label8.TabIndex = 2;
            label8.Text = "الشخص المسؤول عنه";
            // 
            // btnUnApprove
            // 
            btnUnApprove.BackColor = Color.White;
            btnUnApprove.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnUnApprove.FlatStyle = FlatStyle.Flat;
            btnUnApprove.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnUnApprove.ForeColor = Color.FromArgb(16, 65, 112);
            btnUnApprove.Location = new Point(773, 536);
            btnUnApprove.Name = "btnUnApprove";
            btnUnApprove.Padding = new Padding(6, 0, 6, 0);
            btnUnApprove.Size = new Size(109, 34);
            btnUnApprove.TabIndex = 7;
            btnUnApprove.Text = "الغاء الاعتماد";
            btnUnApprove.UseVisualStyleBackColor = false;
            btnUnApprove.Click += btnUnApprove_Click;
            // 
            // btnApprove
            // 
            btnApprove.BackColor = Color.White;
            btnApprove.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnApprove.FlatStyle = FlatStyle.Flat;
            btnApprove.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnApprove.ForeColor = Color.FromArgb(16, 65, 112);
            btnApprove.Location = new Point(673, 536);
            btnApprove.Name = "btnApprove";
            btnApprove.Padding = new Padding(6, 0, 6, 0);
            btnApprove.Size = new Size(94, 34);
            btnApprove.TabIndex = 8;
            btnApprove.Text = "اعتماد";
            btnApprove.UseVisualStyleBackColor = false;
            btnApprove.Click += btnApprove_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.White;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(16, 65, 112);
            btnRefresh.Location = new Point(673, 501);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Padding = new Padding(6, 0, 6, 0);
            btnRefresh.Size = new Size(94, 34);
            btnRefresh.TabIndex = 9;
            btnRefresh.Text = "تحديث";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnPreview
            // 
            btnPreview.BackColor = Color.White;
            btnPreview.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnPreview.FlatStyle = FlatStyle.Flat;
            btnPreview.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnPreview.ForeColor = Color.FromArgb(16, 65, 112);
            btnPreview.Location = new Point(573, 536);
            btnPreview.Name = "btnPreview";
            btnPreview.Padding = new Padding(6, 0, 6, 0);
            btnPreview.Size = new Size(94, 34);
            btnPreview.TabIndex = 10;
            btnPreview.Text = "معاينة";
            btnPreview.UseVisualStyleBackColor = false;
            btnPreview.Click += btnPreview_Click;
            // 
            // btnSearch
            // 
            btnSearch.BackColor = Color.White;
            btnSearch.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnSearch.ForeColor = Color.FromArgb(16, 65, 112);
            btnSearch.Location = new Point(573, 501);
            btnSearch.Name = "btnSearch";
            btnSearch.Padding = new Padding(6, 0, 6, 0);
            btnSearch.Size = new Size(94, 34);
            btnSearch.TabIndex = 11;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnImport
            // 
            btnImport.BackColor = Color.White;
            btnImport.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnImport.ForeColor = Color.FromArgb(16, 65, 112);
            btnImport.Location = new Point(473, 536);
            btnImport.Name = "btnImport";
            btnImport.Padding = new Padding(6, 0, 6, 0);
            btnImport.Size = new Size(94, 34);
            btnImport.TabIndex = 12;
            btnImport.Text = "استيراد";
            btnImport.UseVisualStyleBackColor = false;
            btnImport.Click += btnImport_Click;
            // 
            // btnExport
            // 
            btnExport.BackColor = Color.White;
            btnExport.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnExport.ForeColor = Color.FromArgb(16, 65, 112);
            btnExport.Location = new Point(373, 536);
            btnExport.Name = "btnExport";
            btnExport.Padding = new Padding(6, 0, 6, 0);
            btnExport.Size = new Size(94, 34);
            btnExport.TabIndex = 13;
            btnExport.Text = "تصدير";
            btnExport.UseVisualStyleBackColor = false;
            btnExport.Click += btnExport_Click;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.White;
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnDelete.ForeColor = Color.FromArgb(16, 65, 112);
            btnDelete.Location = new Point(473, 501);
            btnDelete.Name = "btnDelete";
            btnDelete.Padding = new Padding(6, 0, 6, 0);
            btnDelete.Size = new Size(94, 34);
            btnDelete.TabIndex = 14;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.White;
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnClose.ForeColor = Color.FromArgb(16, 65, 112);
            btnClose.Location = new Point(273, 536);
            btnClose.Name = "btnClose";
            btnClose.Padding = new Padding(6, 0, 6, 0);
            btnClose.Size = new Size(94, 34);
            btnClose.TabIndex = 15;
            btnClose.Text = "اغلاق";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // btnEdit
            // 
            btnEdit.BackColor = Color.White;
            btnEdit.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnEdit.ForeColor = Color.FromArgb(16, 65, 112);
            btnEdit.Location = new Point(373, 501);
            btnEdit.Name = "btnEdit";
            btnEdit.Padding = new Padding(6, 0, 6, 0);
            btnEdit.Size = new Size(94, 34);
            btnEdit.TabIndex = 16;
            btnEdit.Text = "تعديل";
            btnEdit.UseVisualStyleBackColor = false;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnPrint
            // 
            btnPrint.BackColor = Color.White;
            btnPrint.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnPrint.ForeColor = Color.FromArgb(16, 65, 112);
            btnPrint.Location = new Point(173, 536);
            btnPrint.Name = "btnPrint";
            btnPrint.Padding = new Padding(6, 0, 6, 0);
            btnPrint.Size = new Size(94, 34);
            btnPrint.TabIndex = 17;
            btnPrint.Text = "طباعة";
            btnPrint.UseVisualStyleBackColor = false;
            btnPrint.Click += btnPrint_Click;
            // 
            // btnNew
            // 
            btnNew.BackColor = Color.White;
            btnNew.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnNew.FlatStyle = FlatStyle.Flat;
            btnNew.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnNew.ForeColor = Color.FromArgb(16, 65, 112);
            btnNew.Location = new Point(173, 501);
            btnNew.Name = "btnNew";
            btnNew.Padding = new Padding(6, 0, 6, 0);
            btnNew.Size = new Size(94, 34);
            btnNew.TabIndex = 18;
            btnNew.Text = "جديد";
            btnNew.UseVisualStyleBackColor = false;
            btnNew.Click += btnNew_Click;
            // 
            // dgvBranches
            // 
            dgvBranches.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(248, 250, 253);
            dgvBranches.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvBranches.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvBranches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBranches.BackgroundColor = Color.White;
            dgvBranches.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(231, 239, 249);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(19, 61, 103);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvBranches.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvBranches.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle3.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle3.Padding = new Padding(4);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(216, 232, 249);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(12, 52, 92);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvBranches.DefaultCellStyle = dataGridViewCellStyle3;
            dgvBranches.EnableHeadersVisualStyles = false;
            dgvBranches.GridColor = Color.FromArgb(226, 232, 240);
            dgvBranches.Location = new Point(0, 591);
            dgvBranches.Margin = new Padding(0);
            dgvBranches.Name = "dgvBranches";
            dgvBranches.RowHeadersVisible = false;
            dgvBranches.RowHeadersWidth = 51;
            dgvBranches.RowTemplate.Height = 34;
            dgvBranches.Size = new Size(2375, 1006);
            dgvBranches.TabIndex = 19;
            dgvBranches.CellClick += dgvBranches_CellClick;
            // 
            // cmbStatus
            // 
            cmbStatus.BackColor = Color.White;
            cmbStatus.FlatStyle = FlatStyle.Flat;
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(227, 352);
            cmbStatus.Margin = new Padding(4, 6, 4, 6);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(151, 33);
            cmbStatus.TabIndex = 0;
            // 
            // label9
            // 
            label9.AutoEllipsis = true;
            label9.AutoSize = true;
            label9.Location = new Point(139, 352);
            label9.Name = "label9";
            label9.Size = new Size(54, 25);
            label9.TabIndex = 2;
            label9.Text = "الحالة";
            // 
            // txtPhone
            // 
            txtPhone.BackColor = Color.White;
            txtPhone.BorderStyle = BorderStyle.FixedSingle;
            txtPhone.Location = new Point(784, 253);
            txtPhone.Margin = new Padding(4, 6, 4, 6);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(125, 31);
            txtPhone.TabIndex = 5;
            // 
            // label10
            // 
            label10.AutoEllipsis = true;
            label10.AutoSize = true;
            label10.Location = new Point(718, 259);
            label10.Name = "label10";
            label10.Size = new Size(61, 25);
            label10.TabIndex = 6;
            label10.Text = "الهاتف";
            // 
            // txtWebsite
            // 
            txtWebsite.BackColor = Color.White;
            txtWebsite.BorderStyle = BorderStyle.FixedSingle;
            txtWebsite.Location = new Point(784, 320);
            txtWebsite.Margin = new Padding(4, 6, 4, 6);
            txtWebsite.Name = "txtWebsite";
            txtWebsite.Size = new Size(125, 31);
            txtWebsite.TabIndex = 5;
            // 
            // label11
            // 
            label11.AutoEllipsis = true;
            label11.AutoSize = true;
            label11.Location = new Point(638, 326);
            label11.Name = "label11";
            label11.Size = new Size(141, 25);
            label11.TabIndex = 6;
            label11.Text = "الموقع الالكتروني";
            // 
            // txtEmail
            // 
            txtEmail.BackColor = Color.White;
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.Location = new Point(784, 356);
            txtEmail.Margin = new Padding(4, 6, 4, 6);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(125, 31);
            txtEmail.TabIndex = 5;
            // 
            // label12
            // 
            label12.AutoEllipsis = true;
            label12.AutoSize = true;
            label12.Location = new Point(718, 292);
            label12.Name = "label12";
            label12.Size = new Size(60, 25);
            label12.TabIndex = 6;
            label12.Text = "الجوال";
            // 
            // txtNotes
            // 
            txtNotes.BackColor = Color.White;
            txtNotes.BorderStyle = BorderStyle.FixedSingle;
            txtNotes.Location = new Point(223, 390);
            txtNotes.Margin = new Padding(4, 6, 4, 6);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.Size = new Size(686, 57);
            txtNotes.TabIndex = 5;
            // 
            // label13
            // 
            label13.AutoEllipsis = true;
            label13.AutoSize = true;
            label13.Location = new Point(124, 393);
            label13.Name = "label13";
            label13.Size = new Size(82, 25);
            label13.TabIndex = 6;
            label13.Text = "ملاحظات";
            label13.Click += label13_Click;
            // 
            // txtBranchCode
            // 
            txtBranchCode.BackColor = Color.White;
            txtBranchCode.BorderStyle = BorderStyle.FixedSingle;
            txtBranchCode.Location = new Point(230, 135);
            txtBranchCode.Margin = new Padding(4, 6, 4, 6);
            txtBranchCode.Name = "txtBranchCode";
            txtBranchCode.Size = new Size(125, 31);
            txtBranchCode.TabIndex = 1;
            // 
            // label14
            // 
            label14.AutoEllipsis = true;
            label14.AutoSize = true;
            label14.Location = new Point(137, 138);
            label14.Name = "label14";
            label14.Size = new Size(83, 25);
            label14.TabIndex = 2;
            label14.Text = "كود الفرع";
            // 
            // cmbCity
            // 
            cmbCity.BackColor = Color.White;
            cmbCity.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCity.FlatStyle = FlatStyle.Flat;
            cmbCity.FormattingEnabled = true;
            cmbCity.Location = new Point(484, 352);
            cmbCity.Margin = new Padding(4, 6, 4, 6);
            cmbCity.Name = "cmbCity";
            cmbCity.Size = new Size(151, 33);
            cmbCity.TabIndex = 21;
            // 
            // label15
            // 
            label15.AutoEllipsis = true;
            label15.AutoSize = true;
            label15.Location = new Point(405, 355);
            label15.Name = "label15";
            label15.Size = new Size(64, 25);
            label15.TabIndex = 22;
            label15.Text = "المدينة";
            // 
            // pnlTopBar
            // 
            pnlTopBar.BackColor = Color.White;
            pnlTopBar.Controls.Add(btnLogout);
            pnlTopBar.Controls.Add(btnAboutSystem);
            pnlTopBar.Controls.Add(btnSettings);
            pnlTopBar.Controls.Add(btnNotifications);
            pnlTopBar.Controls.Add(picCompanyLogo);
            pnlTopBar.Controls.Add(lblCurrentUser);
            pnlTopBar.Controls.Add(lblCompanyName);
            pnlTopBar.Controls.Add(lblFiscalYear);
            pnlTopBar.Controls.Add(lblCurrentBranch);
            pnlTopBar.Dock = DockStyle.Top;
            pnlTopBar.Location = new Point(0, 0);
            pnlTopBar.Margin = new Padding(0);
            pnlTopBar.Name = "pnlTopBar";
            pnlTopBar.Size = new Size(1523, 58);
            pnlTopBar.TabIndex = 20;
            // 
            // btnLogout
            // 
            btnLogout.BackColor = Color.White;
            btnLogout.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnLogout.ForeColor = Color.FromArgb(16, 65, 112);
            btnLogout.Location = new Point(11, 23);
            btnLogout.Name = "btnLogout";
            btnLogout.Padding = new Padding(6, 0, 6, 0);
            btnLogout.Size = new Size(141, 34);
            btnLogout.TabIndex = 4;
            btnLogout.Text = "تسجيل الخروج";
            btnLogout.UseVisualStyleBackColor = false;
            // 
            // btnAboutSystem
            // 
            btnAboutSystem.BackColor = Color.White;
            btnAboutSystem.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnAboutSystem.FlatStyle = FlatStyle.Flat;
            btnAboutSystem.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnAboutSystem.ForeColor = Color.FromArgb(16, 65, 112);
            btnAboutSystem.Location = new Point(158, 0);
            btnAboutSystem.Name = "btnAboutSystem";
            btnAboutSystem.Padding = new Padding(6, 0, 6, 0);
            btnAboutSystem.Size = new Size(94, 54);
            btnAboutSystem.TabIndex = 3;
            btnAboutSystem.Text = "حول\r\nالنظام";
            btnAboutSystem.UseVisualStyleBackColor = false;
            // 
            // btnSettings
            // 
            btnSettings.BackColor = Color.White;
            btnSettings.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnSettings.ForeColor = Color.FromArgb(16, 65, 112);
            btnSettings.Location = new Point(257, 19);
            btnSettings.Name = "btnSettings";
            btnSettings.Padding = new Padding(6, 0, 6, 0);
            btnSettings.Size = new Size(94, 34);
            btnSettings.TabIndex = 3;
            btnSettings.Text = "الاعدادات";
            btnSettings.UseVisualStyleBackColor = false;
            // 
            // btnNotifications
            // 
            btnNotifications.BackColor = Color.White;
            btnNotifications.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 230);
            btnNotifications.FlatStyle = FlatStyle.Flat;
            btnNotifications.Font = new Font("Segoe UI", 9.2F, FontStyle.Bold);
            btnNotifications.ForeColor = Color.FromArgb(16, 65, 112);
            btnNotifications.Location = new Point(355, 19);
            btnNotifications.Name = "btnNotifications";
            btnNotifications.Padding = new Padding(6, 0, 6, 0);
            btnNotifications.Size = new Size(94, 34);
            btnNotifications.TabIndex = 2;
            btnNotifications.Text = "الاشعارات";
            btnNotifications.UseVisualStyleBackColor = false;
            // 
            // picCompanyLogo
            // 
            picCompanyLogo.Location = new Point(999, 6);
            picCompanyLogo.Name = "picCompanyLogo";
            picCompanyLogo.Size = new Size(125, 62);
            picCompanyLogo.TabIndex = 1;
            picCompanyLogo.TabStop = false;
            // 
            // lblCurrentUser
            // 
            lblCurrentUser.AutoEllipsis = true;
            lblCurrentUser.AutoSize = true;
            lblCurrentUser.Location = new Point(462, 8);
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new Size(104, 50);
            lblCurrentUser.TabIndex = 0;
            lblCurrentUser.Text = "المستخدم:\r\n مدير النظام";
            // 
            // lblCompanyName
            // 
            lblCompanyName.AutoEllipsis = true;
            lblCompanyName.AutoSize = true;
            lblCompanyName.Location = new Point(844, 4);
            lblCompanyName.Name = "lblCompanyName";
            lblCompanyName.Size = new Size(148, 50);
            lblCompanyName.TabIndex = 0;
            lblCompanyName.Text = "شركة\r\n الطائر للنقل البري";
            // 
            // lblFiscalYear
            // 
            lblFiscalYear.AutoEllipsis = true;
            lblFiscalYear.AutoSize = true;
            lblFiscalYear.Location = new Point(566, 8);
            lblFiscalYear.Name = "lblFiscalYear";
            lblFiscalYear.Size = new Size(115, 50);
            lblFiscalYear.TabIndex = 0;
            lblFiscalYear.Text = "السنة المالية :\r\n 2026";
            // 
            // lblCurrentBranch
            // 
            lblCurrentBranch.AutoEllipsis = true;
            lblCurrentBranch.AutoSize = true;
            lblCurrentBranch.Location = new Point(696, 3);
            lblCurrentBranch.Name = "lblCurrentBranch";
            lblCurrentBranch.Size = new Size(121, 50);
            lblCurrentBranch.TabIndex = 0;
            lblCurrentBranch.Text = "الفرع \r\nعدن_المنصورة";
            // 
            // BranchForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1523, 822);
            Controls.Add(pnlTopBar);
            Controls.Add(label15);
            Controls.Add(cmbCity);
            Controls.Add(dgvBranches);
            Controls.Add(btnUnApprove);
            Controls.Add(btnApprove);
            Controls.Add(btnRefresh);
            Controls.Add(btnPreview);
            Controls.Add(btnSearch);
            Controls.Add(btnImport);
            Controls.Add(btnExport);
            Controls.Add(btnDelete);
            Controls.Add(btnClose);
            Controls.Add(btnEdit);
            Controls.Add(btnPrint);
            Controls.Add(btnNew);
            Controls.Add(label10);
            Controls.Add(txtPhone);
            Controls.Add(label13);
            Controls.Add(txtNotes);
            Controls.Add(label11);
            Controls.Add(label12);
            Controls.Add(txtWebsite);
            Controls.Add(txtEmail);
            Controls.Add(label7);
            Controls.Add(txtMobile);
            Controls.Add(chkAllowPercentage);
            Controls.Add(chkAllowCredit);
            Controls.Add(btnSaveBranch);
            Controls.Add(label5);
            Controls.Add(label3);
            Controls.Add(label14);
            Controls.Add(label2);
            Controls.Add(label9);
            Controls.Add(label6);
            Controls.Add(label8);
            Controls.Add(label4);
            Controls.Add(label1);
            Controls.Add(txtLocation);
            Controls.Add(txtBranchNameEn);
            Controls.Add(cmbManager);
            Controls.Add(cmbStatus);
            Controls.Add(cmbParentBranch);
            Controls.Add(txtBranchCode);
            Controls.Add(cmbBranchType);
            Controls.Add(txtBranchNameAr);
            Controls.Add(cmbCompanies);
            Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(0);
            Name = "BranchForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Text = "ادارة الفروع";
            Load += BranchForm_Load;
            ((ISupportInitialize)dgvBranches).EndInit();
            pnlTopBar.ResumeLayout(false);
            pnlTopBar.PerformLayout();
            ((ISupportInitialize)picCompanyLogo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        // ====================================================================
        // [4] قسم الإعلان عن المتغيرات وعناصر التحكم لـ شاشة إدارة الفروع
        // ====================================================================
        private ComboBox cmbCompanies;
        private TextBox txtBranchNameAr;
        private Label label1;
        private Label label2;
        private Button btnSaveBranch;
        private TextBox txtBranchNameEn;
        private Label label3;
        private ComboBox cmbBranchType;
        private Label label4;
        private TextBox txtLocation;
        private Label label5;
        private CheckBox chkAllowCredit;
        private ComboBox cmbParentBranch;
        private Label label6;
        private CheckBox chkAllowPercentage;
        private Label label7;
        private TextBox txtMobile;
        private ComboBox cmbManager;
        private Label label8;
        private Button btnUnApprove;
        private Button btnApprove;
        private Button btnRefresh;
        private Button btnPreview;
        private Button btnSearch;
        private Button btnImport;
        private Button btnExport;
        private Button btnDelete;
        private Button btnClose;
        private Button btnEdit;
        private Button btnPrint;
        private Button btnNew;
        private DataGridView dgvBranches;
        private ComboBox cmbStatus; // الإعلان البرمجي لكومبو بوكس الحالة (نشط/موقوف) ليتوافق مع كود الفورم المطور
        private Label label9;
        private TextBox txtPhone;
        private Label label10;
        private TextBox txtWebsite;
        private Label label11;
        private TextBox txtEmail;
        private Label label12;
        private TextBox txtNotes;
        private Label label13;
        private TextBox txtBranchCode;
        private Label label14;
        private ComboBox cmbCity;
        private Label label15;
        private Panel pnlTopBar;
        private Button btnLogout;
        private Button btnAboutSystem;
        private Button btnSettings;
        private Button btnNotifications;
        private PictureBox picCompanyLogo;
        private Label lblCurrentUser;
        private Label lblCompanyName;
        private Label lblFiscalYear;
        private Label lblCurrentBranch;
    }
}
