namespace AlTayerERP.Desktop
{
    partial class CompanyForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cmbGroups = new ComboBox();
            txtCompanyNameEn = new TextBox();
            txtCompanyNameAr = new TextBox();
            btnSaveCompany = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            txtCompanyPrefix = new TextBox();
            label4 = new Label();
            btnNew = new Button();
            btnEdit = new Button();
            btnDelete = new Button();
            btnSearch = new Button();
            txtEmail = new TextBox();
            txtAddress = new TextBox();
            txtPhone = new TextBox();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            txtTaxNumber = new TextBox();
            label8 = new Label();
            chkIsActive = new CheckBox();
            btnRefresh = new Button();
            btnPrint = new Button();
            btnClose = new Button();
            btnExport = new Button();
            btnImport = new Button();
            btnPreview = new Button();
            btnApprove = new Button();
            btnUnApprove = new Button();
            dgvCompanies = new DataGridView();
            picCompanyLogo = new PictureBox();
            label9 = new Label();
            btnBrowseLogo = new Button();
            btnRemoveLogo = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvCompanies).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).BeginInit();
            SuspendLayout();
            // 
            // cmbGroups
            // 
            cmbGroups.FormattingEnabled = true;
            cmbGroups.Location = new Point(182, 94);
            cmbGroups.Name = "cmbGroups";
            cmbGroups.Size = new Size(277, 28);
            cmbGroups.TabIndex = 0;
            // 
            // txtCompanyNameEn
            // 
            txtCompanyNameEn.Location = new Point(182, 161);
            txtCompanyNameEn.Name = "txtCompanyNameEn";
            txtCompanyNameEn.Size = new Size(245, 27);
            txtCompanyNameEn.TabIndex = 1;
            // 
            // txtCompanyNameAr
            // 
            txtCompanyNameAr.Location = new Point(182, 128);
            txtCompanyNameAr.Name = "txtCompanyNameAr";
            txtCompanyNameAr.Size = new Size(245, 27);
            txtCompanyNameAr.TabIndex = 1;
            // 
            // btnSaveCompany
            // 
            btnSaveCompany.Location = new Point(79, 12);
            btnSaveCompany.Name = "btnSaveCompany";
            btnSaveCompany.Size = new Size(94, 29);
            btnSaveCompany.TabIndex = 2;
            btnSaveCompany.Text = "حفظ الشركة";
            btnSaveCompany.UseVisualStyleBackColor = true;
            btnSaveCompany.Click += btnSaveCompany_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            label1.Location = new Point(25, 97);
            label1.Name = "label1";
            label1.Size = new Size(148, 25);
            label1.TabIndex = 3;
            label1.Text = "المجموعة التجارية";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            label2.Location = new Point(15, 131);
            label2.Name = "label2";
            label2.Size = new Size(159, 25);
            label2.TabIndex = 3;
            label2.Text = "اسم الشركة بالعربي";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            label3.Location = new Point(23, 164);
            label3.Name = "label3";
            label3.Size = new Size(150, 25);
            label3.TabIndex = 3;
            label3.Text = "اسم الشركة انجلزي";
            // 
            // txtCompanyPrefix
            // 
            txtCompanyPrefix.Location = new Point(182, 194);
            txtCompanyPrefix.Name = "txtCompanyPrefix";
            txtCompanyPrefix.Size = new Size(245, 27);
            txtCompanyPrefix.TabIndex = 1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            label4.Location = new Point(82, 196);
            label4.Name = "label4";
            label4.Size = new Size(91, 25);
            label4.TabIndex = 3;
            label4.Text = "رمز الشركة";
            // 
            // btnNew
            // 
            btnNew.Location = new Point(179, 12);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(94, 29);
            btnNew.TabIndex = 2;
            btnNew.Text = "جديد";
            btnNew.UseVisualStyleBackColor = true;
            btnNew.Click += btnNew_Click;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(279, 12);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(94, 29);
            btnEdit.TabIndex = 2;
            btnEdit.Text = "تعديل";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(379, 12);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(94, 29);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(479, 12);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(94, 29);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(182, 328);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(245, 27);
            txtEmail.TabIndex = 1;
            // 
            // txtAddress
            // 
            txtAddress.Location = new Point(182, 227);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(245, 27);
            txtAddress.TabIndex = 1;
            // 
            // txtPhone
            // 
            txtPhone.Location = new Point(182, 295);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(245, 27);
            txtPhone.TabIndex = 1;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            label5.Location = new Point(42, 298);
            label5.Name = "label5";
            label5.Size = new Size(61, 25);
            label5.TabIndex = 3;
            label5.Text = "الهاتف";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            label6.Location = new Point(43, 331);
            label6.Name = "label6";
            label6.Size = new Size(132, 25);
            label6.TabIndex = 3;
            label6.Text = "البريد الإلكتروني";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            label7.Location = new Point(41, 229);
            label7.Name = "label7";
            label7.Size = new Size(66, 25);
            label7.TabIndex = 3;
            label7.Text = "العنوان";
            // 
            // txtTaxNumber
            // 
            txtTaxNumber.Location = new Point(182, 262);
            txtTaxNumber.Name = "txtTaxNumber";
            txtTaxNumber.Size = new Size(245, 27);
            txtTaxNumber.TabIndex = 1;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            label8.Location = new Point(53, 264);
            label8.Name = "label8";
            label8.Size = new Size(119, 25);
            label8.TabIndex = 3;
            label8.Text = "الرقم الضريبي";
            // 
            // chkIsActive
            // 
            chkIsActive.AutoSize = true;
            chkIsActive.Location = new Point(493, 100);
            chkIsActive.Name = "chkIsActive";
            chkIsActive.Size = new Size(62, 24);
            chkIsActive.TabIndex = 4;
            chkIsActive.Text = "نشط";
            chkIsActive.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(579, 12);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(94, 29);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "تحديث";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(79, 47);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(94, 29);
            btnPrint.TabIndex = 2;
            btnPrint.Text = "طباعة";
            btnPrint.UseVisualStyleBackColor = true;
            btnPrint.Click += btnPrint_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(179, 47);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(94, 29);
            btnClose.TabIndex = 2;
            btnClose.Text = "اغلاق";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(279, 47);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(94, 29);
            btnExport.TabIndex = 2;
            btnExport.Text = "تصدير";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // btnImport
            // 
            btnImport.Location = new Point(379, 47);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(94, 29);
            btnImport.TabIndex = 2;
            btnImport.Text = "استيراد";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // btnPreview
            // 
            btnPreview.Location = new Point(479, 47);
            btnPreview.Name = "btnPreview";
            btnPreview.Size = new Size(94, 29);
            btnPreview.TabIndex = 2;
            btnPreview.Text = "معاينة";
            btnPreview.UseVisualStyleBackColor = true;
            btnPreview.Click += btnPreview_Click;
            // 
            // btnApprove
            // 
            btnApprove.Location = new Point(579, 47);
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(94, 29);
            btnApprove.TabIndex = 2;
            btnApprove.Text = "اعتماد";
            btnApprove.UseVisualStyleBackColor = true;
            btnApprove.Click += btnApprove_Click;
            // 
            // btnUnApprove
            // 
            btnUnApprove.Location = new Point(679, 47);
            btnUnApprove.Name = "btnUnApprove";
            btnUnApprove.Size = new Size(109, 29);
            btnUnApprove.TabIndex = 2;
            btnUnApprove.Text = "الغاء الاعتماد";
            btnUnApprove.UseVisualStyleBackColor = true;
            btnUnApprove.Click += btnUnApprove_Click;
            // 
            // dgvCompanies
            // 
            dgvCompanies.AllowUserToAddRows = false;
            dgvCompanies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCompanies.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCompanies.Dock = DockStyle.Bottom;
            dgvCompanies.Location = new Point(0, 370);
            dgvCompanies.Name = "dgvCompanies";
            dgvCompanies.ReadOnly = true;
            dgvCompanies.RowHeadersWidth = 51;
            dgvCompanies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCompanies.Size = new Size(1022, 220);
            dgvCompanies.TabIndex = 5;
            dgvCompanies.CellContentClick += dgvCompanies_CellClick;
            // 
            // picCompanyLogo
            // 
            picCompanyLogo.BackColor = Color.FromArgb(255, 255, 128);
            picCompanyLogo.Location = new Point(769, 163);
            picCompanyLogo.Name = "picCompanyLogo";
            picCompanyLogo.Size = new Size(180, 180);
            picCompanyLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picCompanyLogo.TabIndex = 6;
            picCompanyLogo.TabStop = false;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            label9.Location = new Point(771, 131);
            label9.Name = "label9";
            label9.Size = new Size(105, 25);
            label9.TabIndex = 3;
            label9.Text = "شعار الشركة";
            // 
            // btnBrowseLogo
            // 
            btnBrowseLogo.Location = new Point(631, 196);
            btnBrowseLogo.Name = "btnBrowseLogo";
            btnBrowseLogo.Size = new Size(120, 29);
            btnBrowseLogo.TabIndex = 2;
            btnBrowseLogo.Text = "اختيار الشعار";
            btnBrowseLogo.UseVisualStyleBackColor = true;
            btnBrowseLogo.Click += btnBrowseLogo_Click;
            // 
            // btnRemoveLogo
            // 
            btnRemoveLogo.Location = new Point(631, 231);
            btnRemoveLogo.Name = "btnRemoveLogo";
            btnRemoveLogo.Size = new Size(120, 29);
            btnRemoveLogo.TabIndex = 2;
            btnRemoveLogo.Text = "حذف الشعار";
            btnRemoveLogo.UseVisualStyleBackColor = true;
            btnRemoveLogo.Click += btnRemoveLogo_Click;
            // 
            // CompanyForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1022, 590);
            Controls.Add(picCompanyLogo);
            Controls.Add(dgvCompanies);
            Controls.Add(chkIsActive);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label4);
            Controls.Add(label9);
            Controls.Add(label6);
            Controls.Add(label3);
            Controls.Add(label5);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnUnApprove);
            Controls.Add(btnRemoveLogo);
            Controls.Add(btnApprove);
            Controls.Add(btnBrowseLogo);
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
            Controls.Add(btnSaveCompany);
            Controls.Add(txtPhone);
            Controls.Add(txtCompanyNameAr);
            Controls.Add(txtTaxNumber);
            Controls.Add(txtAddress);
            Controls.Add(txtCompanyPrefix);
            Controls.Add(txtEmail);
            Controls.Add(txtCompanyNameEn);
            Controls.Add(cmbGroups);
            Name = "CompanyForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Text = "ادارة تاسيس الشركات التابعة";
            Load += CompanyForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvCompanies).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cmbGroups;
        private TextBox txtCompanyNameAr;
        private TextBox txtCompanyNameEn;
        private Button btnSaveCompany;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox txtCompanyPrefix;
        private Label label4;
        private Button btnRefresh;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnSearch;
        private TextBox txtEmail;
        private TextBox txtAddress;
        private TextBox txtPhone;
        private Label label5;
        private Label label6;
        private Label label7;
        private TextBox txtTaxNumber;
        private Label label8;
        private CheckBox chkIsActive;
        private Button btnNew;
        private Button btnPrint;
        private Button btnClose;
        private Button btnExport;
        private Button btnImport;
        private Button btnPreview;
        private Button btnApprove;
        private Button btnUnApprove;
        private DataGridView dgvCompanies;
        private PictureBox picCompanyLogo;
        private Label label9;
        private Button btnBrowseLogo;
        private Button btnRemoveLogo;
    }
}