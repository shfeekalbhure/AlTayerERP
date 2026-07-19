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
            cmbCompanies.FormattingEnabled = true;
            cmbCompanies.Location = new Point(227, 99);
            cmbCompanies.Name = "cmbCompanies";
            cmbCompanies.Size = new Size(151, 33);
            cmbCompanies.TabIndex = 0;
            // 
            // txtBranchNameAr
            // 
            txtBranchNameAr.Location = new Point(227, 170);
            txtBranchNameAr.Name = "txtBranchNameAr";
            txtBranchNameAr.Size = new Size(125, 31);
            txtBranchNameAr.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(71, 99);
            label1.Name = "label1";
            label1.Size = new Size(150, 25);
            label1.TabIndex = 2;
            label1.Text = "اسم الشركة التابعة";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(134, 173);
            label2.Name = "label2";
            label2.Size = new Size(87, 25);
            label2.TabIndex = 2;
            label2.Text = "اسم الفرع";
            // 
            // btnSaveBranch
            // 
            btnSaveBranch.Location = new Point(273, 501);
            btnSaveBranch.Name = "btnSaveBranch";
            btnSaveBranch.Size = new Size(94, 29);
            btnSaveBranch.TabIndex = 3;
            btnSaveBranch.Text = "حفظ";
            btnSaveBranch.UseVisualStyleBackColor = true;
            btnSaveBranch.Click += btnSaveBranch_Click;
            // 
            // txtBranchNameEn
            // 
            txtBranchNameEn.Location = new Point(228, 204);
            txtBranchNameEn.Name = "txtBranchNameEn";
            txtBranchNameEn.Size = new Size(125, 31);
            txtBranchNameEn.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(82, 204);
            label3.Name = "label3";
            label3.Size = new Size(140, 25);
            label3.TabIndex = 2;
            label3.Text = "اسم الفرع انجلزي";
            // 
            // cmbBranchType
            // 
            cmbBranchType.FormattingEnabled = true;
            cmbBranchType.Location = new Point(223, 275);
            cmbBranchType.Name = "cmbBranchType";
            cmbBranchType.Size = new Size(151, 33);
            cmbBranchType.TabIndex = 0;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(135, 275);
            label4.Name = "label4";
            label4.Size = new Size(82, 25);
            label4.TabIndex = 2;
            label4.Text = "نوع الفرع";
            // 
            // txtLocation
            // 
            txtLocation.Location = new Point(228, 240);
            txtLocation.Name = "txtLocation";
            txtLocation.Size = new Size(125, 31);
            txtLocation.TabIndex = 1;
            // 
            // label5
            // 
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
            cmbParentBranch.FormattingEnabled = true;
            cmbParentBranch.Location = new Point(228, 312);
            cmbParentBranch.Name = "cmbParentBranch";
            cmbParentBranch.Size = new Size(151, 33);
            cmbParentBranch.TabIndex = 0;
            // 
            // label6
            // 
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
            label7.AutoSize = true;
            label7.Location = new Point(646, 362);
            label7.Name = "label7";
            label7.Size = new Size(132, 25);
            label7.TabIndex = 6;
            label7.Text = "البريد الالكتروني";
            // 
            // txtMobile
            // 
            txtMobile.Location = new Point(784, 286);
            txtMobile.Name = "txtMobile";
            txtMobile.Size = new Size(125, 31);
            txtMobile.TabIndex = 5;
            // 
            // cmbManager
            // 
            cmbManager.FormattingEnabled = true;
            cmbManager.Location = new Point(756, 215);
            cmbManager.Name = "cmbManager";
            cmbManager.Size = new Size(151, 33);
            cmbManager.TabIndex = 0;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(574, 218);
            label8.Name = "label8";
            label8.Size = new Size(176, 25);
            label8.TabIndex = 2;
            label8.Text = "الشخص المسؤول عنه";
            // 
            // btnUnApprove
            // 
            btnUnApprove.Location = new Point(773, 536);
            btnUnApprove.Name = "btnUnApprove";
            btnUnApprove.Size = new Size(109, 29);
            btnUnApprove.TabIndex = 7;
            btnUnApprove.Text = "الغاء الاعتماد";
            btnUnApprove.UseVisualStyleBackColor = true;
            btnUnApprove.Click += btnUnApprove_Click;
            // 
            // btnApprove
            // 
            btnApprove.Location = new Point(673, 536);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(94, 29);
            btnApprove.TabIndex = 8;
            btnApprove.Text = "اعتماد";
            btnApprove.UseVisualStyleBackColor = true;
            btnApprove.Click += btnApprove_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(673, 501);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(94, 29);
            btnRefresh.TabIndex = 9;
            btnRefresh.Text = "تحديث";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnPreview
            // 
            btnPreview.Location = new Point(573, 536);
            btnPreview.Name = "btnPreview";
            btnPreview.Size = new Size(94, 29);
            btnPreview.TabIndex = 10;
            btnPreview.Text = "معاينة";
            btnPreview.UseVisualStyleBackColor = true;
            btnPreview.Click += btnPreview_Click;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(573, 501);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(94, 29);
            btnSearch.TabIndex = 11;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnImport
            // 
            btnImport.Location = new Point(473, 536);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(94, 29);
            btnImport.TabIndex = 12;
            btnImport.Text = "استيراد";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(373, 536);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(94, 29);
            btnExport.TabIndex = 13;
            btnExport.Text = "تصدير";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(473, 501);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(94, 29);
            btnDelete.TabIndex = 14;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(273, 536);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(94, 29);
            btnClose.TabIndex = 15;
            btnClose.Text = "اغلاق";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(373, 501);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(94, 29);
            btnEdit.TabIndex = 16;
            btnEdit.Text = "تعديل";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(173, 536);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(94, 29);
            btnPrint.TabIndex = 17;
            btnPrint.Text = "طباعة";
            btnPrint.UseVisualStyleBackColor = true;
            btnPrint.Click += btnPrint_Click;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(173, 501);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(94, 29);
            btnNew.TabIndex = 18;
            btnNew.Text = "جديد";
            btnNew.UseVisualStyleBackColor = true;
            btnNew.Click += btnNew_Click;
            // 
            // dgvBranches
            // 
            dgvBranches.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBranches.Dock = DockStyle.Bottom;
            dgvBranches.Location = new Point(0, 591);
            dgvBranches.Name = "dgvBranches";
            dgvBranches.RowHeadersWidth = 51;
            dgvBranches.Size = new Size(1000, 184);
            dgvBranches.TabIndex = 19;
            dgvBranches.CellClick += dgvBranches_CellClick;
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(227, 352);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(151, 33);
            cmbStatus.TabIndex = 0;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(139, 352);
            label9.Name = "label9";
            label9.Size = new Size(54, 25);
            label9.TabIndex = 2;
            label9.Text = "الحالة";
            // 
            // txtPhone
            // 
            txtPhone.Location = new Point(784, 253);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(125, 31);
            txtPhone.TabIndex = 5;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(718, 259);
            label10.Name = "label10";
            label10.Size = new Size(61, 25);
            label10.TabIndex = 6;
            label10.Text = "الهاتف";
            // 
            // txtWebsite
            // 
            txtWebsite.Location = new Point(784, 320);
            txtWebsite.Name = "txtWebsite";
            txtWebsite.Size = new Size(125, 31);
            txtWebsite.TabIndex = 5;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(638, 326);
            label11.Name = "label11";
            label11.Size = new Size(141, 25);
            label11.TabIndex = 6;
            label11.Text = "الموقع الالكتروني";
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(784, 356);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(125, 31);
            txtEmail.TabIndex = 5;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(718, 292);
            label12.Name = "label12";
            label12.Size = new Size(60, 25);
            label12.TabIndex = 6;
            label12.Text = "الجوال";
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(223, 390);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.Size = new Size(686, 57);
            txtNotes.TabIndex = 5;
            // 
            // label13
            // 
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
            txtBranchCode.Location = new Point(230, 135);
            txtBranchCode.Name = "txtBranchCode";
            txtBranchCode.Size = new Size(125, 31);
            txtBranchCode.TabIndex = 1;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(137, 138);
            label14.Name = "label14";
            label14.Size = new Size(83, 25);
            label14.TabIndex = 2;
            label14.Text = "كود الفرع";
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
            pnlTopBar.Name = "pnlTopBar";
            pnlTopBar.Size = new Size(1000, 58);
            pnlTopBar.TabIndex = 20;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(11, 23);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(141, 30);
            btnLogout.TabIndex = 4;
            btnLogout.Text = "تسجيل الخروج";
            btnLogout.UseVisualStyleBackColor = true;
            // 
            // btnAboutSystem
            // 
            btnAboutSystem.Location = new Point(158, 0);
            btnAboutSystem.Name = "btnAboutSystem";
            btnAboutSystem.Size = new Size(94, 54);
            btnAboutSystem.TabIndex = 3;
            btnAboutSystem.Text = "حول\r\nالنظام";
            btnAboutSystem.UseVisualStyleBackColor = true;
            // 
            // btnSettings
            // 
            btnSettings.Location = new Point(257, 19);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(94, 30);
            btnSettings.TabIndex = 3;
            btnSettings.Text = "الاعدادات";
            btnSettings.UseVisualStyleBackColor = true;
            // 
            // btnNotifications
            // 
            btnNotifications.Location = new Point(355, 19);
            btnNotifications.Name = "btnNotifications";
            btnNotifications.Size = new Size(94, 30);
            btnNotifications.TabIndex = 2;
            btnNotifications.Text = "الاشعارات";
            btnNotifications.UseVisualStyleBackColor = true;
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
            lblCurrentUser.AutoSize = true;
            lblCurrentUser.Location = new Point(462, 8);
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new Size(104, 50);
            lblCurrentUser.TabIndex = 0;
            lblCurrentUser.Text = "المستخدم:\r\n مدير النظام";
            // 
            // lblCompanyName
            // 
            lblCompanyName.AutoSize = true;
            lblCompanyName.Location = new Point(844, 4);
            lblCompanyName.Name = "lblCompanyName";
            lblCompanyName.Size = new Size(148, 50);
            lblCompanyName.TabIndex = 0;
            lblCompanyName.Text = "شركة\r\n الطائر للنقل البري";
            // 
            // lblFiscalYear
            // 
            lblFiscalYear.AutoSize = true;
            lblFiscalYear.Location = new Point(566, 8);
            lblFiscalYear.Name = "lblFiscalYear";
            lblFiscalYear.Size = new Size(115, 50);
            lblFiscalYear.TabIndex = 0;
            lblFiscalYear.Text = "السنة المالية :\r\n 2026";
            // 
            // lblCurrentBranch
            // 
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
            ClientSize = new Size(1000, 775);
            Controls.Add(pnlTopBar);
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