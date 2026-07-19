namespace AlTayerERP.Desktop
{
    partial class FrmPaymentVoucher
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
        /// Required method for Designer modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            statusSystem = new Panel();
            lblStatusTime = new Label();
            lblStatusLicense = new Label();
            lblStatusApi = new Label();
            lblVersion = new Label();
            lblStatusDatabase = new Label();
            pnlTopBar = new Panel();
            btnClose = new Button();
            picCompanyLogo = new PictureBox();
            lblCurrentUser = new Label();
            lblCompanyName = new Label();
            lblFiscalYear = new Label();
            lblCurrentBranch = new Label();
            pnlToolbar = new Panel();
            btnImport = new Button();
            btnExport = new Button();
            btnAttachments = new Button();
            btnPrint = new Button();
            btnCancelApprove = new Button();
            btnUnPost = new Button();
            btnApprove = new Button();
            btnPost = new Button();
            btnRefresh = new Button();
            btnSearch = new Button();
            btnDelete = new Button();
            btnEdit = new Button();
            btnNew = new Button();
            btnSave = new Button();
            grpVoucherInfo = new GroupBox();
            dtVoucherDate = new DateTimePicker();
            cmbStatus = new ComboBox();
            label14 = new Label();
            label13 = new Label();
            txtReferenceNo = new TextBox();
            cmbVoucherType = new ComboBox();
            label11 = new Label();
            label6 = new Label();
            cmbBranch = new ComboBox();
            label9 = new Label();
            txtVoucherNo = new TextBox();
            label4 = new Label();
            pnlUserInfo = new Panel();
            txtUpdatedBy = new TextBox();
            label3 = new Label();
            txtUpdatedDate = new TextBox();
            label2 = new Label();
            txtCreatedDate = new TextBox();
            label1 = new Label();
            txtCreatedBy = new TextBox();
            label7 = new Label();
            grpDistribution = new GroupBox();
            dgvVoucherDetails = new DataGridView();
            colNo = new DataGridViewTextBoxColumn();
            colAccountCode = new DataGridViewComboBoxColumn();
            colAccountName = new DataGridViewComboBoxColumn();
            colDescription = new DataGridViewTextBoxColumn();
            colCostCenter = new DataGridViewComboBoxColumn();
            colAmount = new DataGridViewTextBoxColumn();
            colCurrency = new DataGridViewComboBoxColumn();
            colExchangeRate = new DataGridViewTextBoxColumn();
            colLocalAmount = new DataGridViewTextBoxColumn();
            colForeignAmount = new DataGridViewTextBoxColumn();
            colNotes = new DataGridViewTextBoxColumn();
            pnlTotals = new Panel();
            chkPosted = new CheckBox();
            checkBox2 = new CheckBox();
            txtTotalForeignAmount = new TextBox();
            label16 = new Label();
            txtJournalNo = new TextBox();
            label20 = new Label();
            txtDifference = new TextBox();
            label21 = new Label();
            txtTotalAmount = new TextBox();
            label22 = new Label();
            groupBox1 = new GroupBox();
            numericUpDown1 = new NumericUpDown();
            numericUpDown2 = new NumericUpDown();
            textBox1 = new TextBox();
            label27 = new Label();
            numericUpDown3 = new NumericUpDown();
            dateTimePicker1 = new DateTimePicker();
            comboBox1 = new ComboBox();
            label28 = new Label();
            label29 = new Label();
            label30 = new Label();
            textBox2 = new TextBox();
            textBox3 = new TextBox();
            label31 = new Label();
            label32 = new Label();
            label33 = new Label();
            label34 = new Label();
            comboBox2 = new ComboBox();
            label35 = new Label();
            label36 = new Label();
            label37 = new Label();
            comboBox3 = new ComboBox();
            label38 = new Label();
            comboBox4 = new ComboBox();
            comboBox5 = new ComboBox();
            statusSystem.SuspendLayout();
            pnlTopBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).BeginInit();
            pnlToolbar.SuspendLayout();
            grpVoucherInfo.SuspendLayout();
            pnlUserInfo.SuspendLayout();
            grpDistribution.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVoucherDetails).BeginInit();
            pnlTotals.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).BeginInit();
            SuspendLayout();
            // 
            // statusSystem
            // 
            statusSystem.BackColor = Color.White;
            statusSystem.Controls.Add(lblStatusTime);
            statusSystem.Controls.Add(lblStatusLicense);
            statusSystem.Controls.Add(lblStatusApi);
            statusSystem.Controls.Add(lblVersion);
            statusSystem.Controls.Add(lblStatusDatabase);
            statusSystem.Dock = DockStyle.Bottom;
            statusSystem.Location = new Point(0, 757);
            statusSystem.Name = "statusSystem";
            statusSystem.Size = new Size(1412, 35);
            statusSystem.TabIndex = 28;
            // 
            // lblStatusTime
            // 
            lblStatusTime.AutoSize = true;
            lblStatusTime.Location = new Point(178, 7);
            lblStatusTime.Name = "lblStatusTime";
            lblStatusTime.Size = new Size(99, 20);
            lblStatusTime.TabIndex = 6;
            lblStatusTime.Text = "الوقت والتاريخ\r\n";
            // 
            // lblStatusLicense
            // 
            lblStatusLicense.AutoSize = true;
            lblStatusLicense.Location = new Point(408, 6);
            lblStatusLicense.Name = "lblStatusLicense";
            lblStatusLicense.Size = new Size(101, 20);
            lblStatusLicense.TabIndex = 5;
            lblStatusLicense.Text = "الترخيص: فعال";
            // 
            // lblStatusApi
            // 
            lblStatusApi.AutoSize = true;
            lblStatusApi.Location = new Point(694, 7);
            lblStatusApi.Name = "lblStatusApi";
            lblStatusApi.Size = new Size(116, 20);
            lblStatusApi.TabIndex = 4;
            lblStatusApi.Text = "API: غير مفحوص";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Location = new Point(1319, 7);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(55, 20);
            lblVersion.TabIndex = 2;
            lblVersion.Text = "الإصدار\t";
            // 
            // lblStatusDatabase
            // 
            lblStatusDatabase.AutoSize = true;
            lblStatusDatabase.Location = new Point(961, 7);
            lblStatusDatabase.Name = "lblStatusDatabase";
            lblStatusDatabase.Size = new Size(181, 20);
            lblStatusDatabase.TabIndex = 3;
            lblStatusDatabase.Text = "قاعدة البيانات: غير مفحوص";
            // 
            // pnlTopBar
            // 
            pnlTopBar.BackColor = Color.White;
            pnlTopBar.Controls.Add(btnClose);
            pnlTopBar.Controls.Add(picCompanyLogo);
            pnlTopBar.Controls.Add(lblCurrentUser);
            pnlTopBar.Controls.Add(lblCompanyName);
            pnlTopBar.Controls.Add(lblFiscalYear);
            pnlTopBar.Controls.Add(lblCurrentBranch);
            pnlTopBar.Dock = DockStyle.Top;
            pnlTopBar.Location = new Point(0, 0);
            pnlTopBar.Name = "pnlTopBar";
            pnlTopBar.Size = new Size(1412, 59);
            pnlTopBar.TabIndex = 27;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(52, 14);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(62, 30);
            btnClose.TabIndex = 4;
            btnClose.Text = "اغلاق";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // picCompanyLogo
            // 
            picCompanyLogo.Location = new Point(1209, 8);
            picCompanyLogo.Name = "picCompanyLogo";
            picCompanyLogo.Size = new Size(180, 45);
            picCompanyLogo.TabIndex = 1;
            picCompanyLogo.TabStop = false;
            // 
            // lblCurrentUser
            // 
            lblCurrentUser.AutoSize = true;
            lblCurrentUser.Location = new Point(338, 9);
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new Size(85, 40);
            lblCurrentUser.TabIndex = 0;
            lblCurrentUser.Text = "المستخدم:\r\n مدير النظام";
            // 
            // lblCompanyName
            // 
            lblCompanyName.AutoSize = true;
            lblCompanyName.Location = new Point(975, 9);
            lblCompanyName.Name = "lblCompanyName";
            lblCompanyName.Size = new Size(121, 40);
            lblCompanyName.TabIndex = 0;
            lblCompanyName.Text = "شركة\r\n الطائر للنقل البري";
            // 
            // lblFiscalYear
            // 
            lblFiscalYear.AutoSize = true;
            lblFiscalYear.Location = new Point(566, 8);
            lblFiscalYear.Name = "lblFiscalYear";
            lblFiscalYear.Size = new Size(95, 40);
            lblFiscalYear.TabIndex = 0;
            lblFiscalYear.Text = "السنة المالية :\r\n 2026";
            // 
            // lblCurrentBranch
            // 
            lblCurrentBranch.AutoSize = true;
            lblCurrentBranch.Location = new Point(748, 13);
            lblCurrentBranch.Name = "lblCurrentBranch";
            lblCurrentBranch.Size = new Size(101, 40);
            lblCurrentBranch.TabIndex = 0;
            lblCurrentBranch.Text = "الفرع \r\nعدن_المنصورة";
            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = SystemColors.ActiveCaption;
            pnlToolbar.Controls.Add(btnImport);
            pnlToolbar.Controls.Add(btnExport);
            pnlToolbar.Controls.Add(btnAttachments);
            pnlToolbar.Controls.Add(btnPrint);
            pnlToolbar.Controls.Add(btnCancelApprove);
            pnlToolbar.Controls.Add(btnUnPost);
            pnlToolbar.Controls.Add(btnApprove);
            pnlToolbar.Controls.Add(btnPost);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(btnSearch);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Controls.Add(btnEdit);
            pnlToolbar.Controls.Add(btnNew);
            pnlToolbar.Controls.Add(btnSave);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 59);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(1412, 50);
            pnlToolbar.TabIndex = 32;
            // 
            // btnImport
            // 
            btnImport.Location = new Point(352, 12);
            btnImport.Name = "btnImport";
            btnImport.RightToLeft = RightToLeft.No;
            btnImport.Size = new Size(62, 29);
            btnImport.TabIndex = 65;
            btnImport.Text = "استيراد";
            btnImport.UseVisualStyleBackColor = true;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(415, 12);
            btnExport.Name = "btnExport";
            btnExport.RightToLeft = RightToLeft.No;
            btnExport.Size = new Size(62, 29);
            btnExport.TabIndex = 60;
            btnExport.Text = "تصدير";
            btnExport.UseVisualStyleBackColor = true;
            // 
            // btnAttachments
            // 
            btnAttachments.Location = new Point(483, 12);
            btnAttachments.Name = "btnAttachments";
            btnAttachments.RightToLeft = RightToLeft.No;
            btnAttachments.Size = new Size(77, 29);
            btnAttachments.TabIndex = 61;
            btnAttachments.Text = "مرفقات";
            btnAttachments.UseVisualStyleBackColor = true;
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(565, 15);
            btnPrint.Name = "btnPrint";
            btnPrint.RightToLeft = RightToLeft.No;
            btnPrint.Size = new Size(62, 29);
            btnPrint.TabIndex = 62;
            btnPrint.Text = "طباعة";
            btnPrint.UseVisualStyleBackColor = true;
            // 
            // btnCancelApprove
            // 
            btnCancelApprove.Location = new Point(633, 15);
            btnCancelApprove.Name = "btnCancelApprove";
            btnCancelApprove.RightToLeft = RightToLeft.No;
            btnCancelApprove.Size = new Size(101, 29);
            btnCancelApprove.TabIndex = 63;
            btnCancelApprove.Text = "الغاء الاعتماد";
            btnCancelApprove.UseVisualStyleBackColor = true;
            // 
            // btnUnPost
            // 
            btnUnPost.Location = new Point(802, 15);
            btnUnPost.Name = "btnUnPost";
            btnUnPost.RightToLeft = RightToLeft.No;
            btnUnPost.Size = new Size(99, 29);
            btnUnPost.TabIndex = 64;
            btnUnPost.Text = "الغاء الترحيل";
            btnUnPost.UseVisualStyleBackColor = true;
            // 
            // btnApprove
            // 
            btnApprove.Location = new Point(737, 15);
            btnApprove.Name = "btnApprove";
            btnApprove.RightToLeft = RightToLeft.No;
            btnApprove.Size = new Size(62, 29);
            btnApprove.TabIndex = 59;
            btnApprove.Text = "اعتماد";
            btnApprove.UseVisualStyleBackColor = true;
            // 
            // btnPost
            // 
            btnPost.Location = new Point(907, 9);
            btnPost.Name = "btnPost";
            btnPost.RightToLeft = RightToLeft.No;
            btnPost.Size = new Size(62, 29);
            btnPost.TabIndex = 58;
            btnPost.Text = "ترحيل";
            btnPost.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(970, 9);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.RightToLeft = RightToLeft.No;
            btnRefresh.Size = new Size(62, 29);
            btnRefresh.TabIndex = 53;
            btnRefresh.Text = "تحديث";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(1041, 9);
            btnSearch.Name = "btnSearch";
            btnSearch.RightToLeft = RightToLeft.No;
            btnSearch.Size = new Size(62, 29);
            btnSearch.TabIndex = 54;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(1117, 12);
            btnDelete.Name = "btnDelete";
            btnDelete.RightToLeft = RightToLeft.No;
            btnDelete.Size = new Size(62, 29);
            btnDelete.TabIndex = 55;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(1196, 12);
            btnEdit.Name = "btnEdit";
            btnEdit.RightToLeft = RightToLeft.No;
            btnEdit.Size = new Size(62, 29);
            btnEdit.TabIndex = 56;
            btnEdit.Text = "تعديل";
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(1345, 12);
            btnNew.Name = "btnNew";
            btnNew.RightToLeft = RightToLeft.No;
            btnNew.Size = new Size(62, 29);
            btnNew.TabIndex = 57;
            btnNew.Text = "جديد";
            btnNew.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(1276, 12);
            btnSave.Name = "btnSave";
            btnSave.RightToLeft = RightToLeft.No;
            btnSave.Size = new Size(62, 29);
            btnSave.TabIndex = 52;
            btnSave.Text = "حفظ";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // grpVoucherInfo
            // 
            grpVoucherInfo.Controls.Add(dtVoucherDate);
            grpVoucherInfo.Controls.Add(cmbStatus);
            grpVoucherInfo.Controls.Add(label14);
            grpVoucherInfo.Controls.Add(label13);
            grpVoucherInfo.Controls.Add(txtReferenceNo);
            grpVoucherInfo.Controls.Add(cmbVoucherType);
            grpVoucherInfo.Controls.Add(label11);
            grpVoucherInfo.Controls.Add(label6);
            grpVoucherInfo.Controls.Add(cmbBranch);
            grpVoucherInfo.Controls.Add(label9);
            grpVoucherInfo.Controls.Add(txtVoucherNo);
            grpVoucherInfo.Controls.Add(label4);
            grpVoucherInfo.Dock = DockStyle.Top;
            grpVoucherInfo.Location = new Point(0, 109);
            grpVoucherInfo.Name = "grpVoucherInfo";
            grpVoucherInfo.RightToLeft = RightToLeft.Yes;
            grpVoucherInfo.Size = new Size(1412, 88);
            grpVoucherInfo.TabIndex = 31;
            grpVoucherInfo.TabStop = false;
            grpVoucherInfo.Text = "معلومات السند الاساسيه";
            // 
            // dtVoucherDate
            // 
            dtVoucherDate.Location = new Point(822, 18);
            dtVoucherDate.Name = "dtVoucherDate";
            dtVoucherDate.Size = new Size(176, 27);
            dtVoucherDate.TabIndex = 37;
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(338, 24);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.RightToLeft = RightToLeft.No;
            cmbStatus.Size = new Size(134, 28);
            cmbStatus.TabIndex = 72;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(481, 27);
            label14.Name = "label14";
            label14.Size = new Size(44, 20);
            label14.TabIndex = 71;
            label14.Text = "الحالة";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(857, 59);
            label13.Name = "label13";
            label13.Size = new Size(51, 20);
            label13.TabIndex = 67;
            label13.Text = "المرجع";
            // 
            // txtReferenceNo
            // 
            txtReferenceNo.Location = new Point(690, 56);
            txtReferenceNo.Name = "txtReferenceNo";
            txtReferenceNo.RightToLeft = RightToLeft.No;
            txtReferenceNo.Size = new Size(164, 27);
            txtReferenceNo.TabIndex = 68;
            // 
            // cmbVoucherType
            // 
            cmbVoucherType.FormattingEnabled = true;
            cmbVoucherType.Location = new Point(546, 20);
            cmbVoucherType.Name = "cmbVoucherType";
            cmbVoucherType.RightToLeft = RightToLeft.No;
            cmbVoucherType.Size = new Size(195, 28);
            cmbVoucherType.TabIndex = 66;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(746, 20);
            label11.Name = "label11";
            label11.Size = new Size(70, 20);
            label11.TabIndex = 65;
            label11.Text = "نوع السند";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(1304, 23);
            label6.Name = "label6";
            label6.Size = new Size(70, 20);
            label6.TabIndex = 59;
            label6.Text = "رقم السند";
            // 
            // cmbBranch
            // 
            cmbBranch.FormattingEnabled = true;
            cmbBranch.Location = new Point(1036, 54);
            cmbBranch.Name = "cmbBranch";
            cmbBranch.RightToLeft = RightToLeft.No;
            cmbBranch.Size = new Size(261, 28);
            cmbBranch.TabIndex = 55;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(1336, 56);
            label9.Name = "label9";
            label9.Size = new Size(41, 20);
            label9.TabIndex = 53;
            label9.Text = "الفرع";
            // 
            // txtVoucherNo
            // 
            txtVoucherNo.Location = new Point(1160, 20);
            txtVoucherNo.Name = "txtVoucherNo";
            txtVoucherNo.ReadOnly = true;
            txtVoucherNo.RightToLeft = RightToLeft.No;
            txtVoucherNo.Size = new Size(137, 27);
            txtVoucherNo.TabIndex = 62;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(1004, 23);
            label4.Name = "label4";
            label4.Size = new Size(92, 20);
            label4.TabIndex = 60;
            label4.Text = "تاريخ المستند";
            // 
            // pnlUserInfo
            // 
            pnlUserInfo.BackColor = SystemColors.ActiveCaption;
            pnlUserInfo.Controls.Add(txtUpdatedBy);
            pnlUserInfo.Controls.Add(label3);
            pnlUserInfo.Controls.Add(txtUpdatedDate);
            pnlUserInfo.Controls.Add(label2);
            pnlUserInfo.Controls.Add(txtCreatedDate);
            pnlUserInfo.Controls.Add(label1);
            pnlUserInfo.Controls.Add(txtCreatedBy);
            pnlUserInfo.Controls.Add(label7);
            pnlUserInfo.Dock = DockStyle.Bottom;
            pnlUserInfo.Location = new Point(0, 724);
            pnlUserInfo.Name = "pnlUserInfo";
            pnlUserInfo.Size = new Size(1412, 33);
            pnlUserInfo.TabIndex = 34;
            // 
            // txtUpdatedBy
            // 
            txtUpdatedBy.Location = new Point(375, 3);
            txtUpdatedBy.Name = "txtUpdatedBy";
            txtUpdatedBy.RightToLeft = RightToLeft.No;
            txtUpdatedBy.Size = new Size(134, 27);
            txtUpdatedBy.TabIndex = 59;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(515, 6);
            label3.Name = "label3";
            label3.Size = new Size(102, 20);
            label3.TabIndex = 58;
            label3.Text = "مستخدم معدل";
            // 
            // txtUpdatedDate
            // 
            txtUpdatedDate.Location = new Point(123, 3);
            txtUpdatedDate.Name = "txtUpdatedDate";
            txtUpdatedDate.RightToLeft = RightToLeft.No;
            txtUpdatedDate.Size = new Size(134, 27);
            txtUpdatedDate.TabIndex = 57;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(263, 6);
            label2.Name = "label2";
            label2.Size = new Size(89, 20);
            label2.TabIndex = 56;
            label2.Text = "تاريخ التعديل";
            // 
            // txtCreatedDate
            // 
            txtCreatedDate.Location = new Point(618, 3);
            txtCreatedDate.Name = "txtCreatedDate";
            txtCreatedDate.RightToLeft = RightToLeft.No;
            txtCreatedDate.Size = new Size(134, 27);
            txtCreatedDate.TabIndex = 55;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(758, 6);
            label1.Name = "label1";
            label1.Size = new Size(91, 20);
            label1.TabIndex = 54;
            label1.Text = "تاريخ الاضافه";
            // 
            // txtCreatedBy
            // 
            txtCreatedBy.Location = new Point(867, 4);
            txtCreatedBy.Name = "txtCreatedBy";
            txtCreatedBy.RightToLeft = RightToLeft.No;
            txtCreatedBy.Size = new Size(134, 27);
            txtCreatedBy.TabIndex = 53;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(1007, 7);
            label7.Name = "label7";
            label7.Size = new Size(71, 20);
            label7.TabIndex = 52;
            label7.Text = "المستخدم";
            // 
            // grpDistribution
            // 
            grpDistribution.Controls.Add(dgvVoucherDetails);
            grpDistribution.Controls.Add(pnlTotals);
            grpDistribution.Dock = DockStyle.Fill;
            grpDistribution.Location = new Point(0, 197);
            grpDistribution.Name = "grpDistribution";
            grpDistribution.RightToLeft = RightToLeft.Yes;
            grpDistribution.Size = new Size(1412, 527);
            grpDistribution.TabIndex = 36;
            grpDistribution.TabStop = false;
            grpDistribution.Text = "التوزيع المحاسبي";
            // 
            // dgvVoucherDetails
            // 
            dgvVoucherDetails.ColumnHeadersHeight = 35;
            dgvVoucherDetails.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvVoucherDetails.Columns.AddRange(new DataGridViewColumn[] { colNo, colAccountCode, colAccountName, colDescription, colCostCenter, colAmount, colCurrency, colExchangeRate, colLocalAmount, colForeignAmount, colNotes });
            dgvVoucherDetails.Dock = DockStyle.Fill;
            dgvVoucherDetails.Location = new Point(3, 23);
            dgvVoucherDetails.Name = "dgvVoucherDetails";
            dgvVoucherDetails.RowHeadersWidth = 51;
            dgvVoucherDetails.Size = new Size(1406, 456);
            dgvVoucherDetails.TabIndex = 37;
            dgvVoucherDetails.CellContentClick += dgvVoucherDetails_CellContentClick;
            // 
            // colNo
            // 
            colNo.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colNo.HeaderText = "م";
            colNo.MinimumWidth = 6;
            colNo.Name = "colNo";
            colNo.Width = 60;
            // 
            // colAccountCode
            // 
            colAccountCode.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colAccountCode.HeaderText = "رقم الحساب";
            colAccountCode.MinimumWidth = 6;
            colAccountCode.Name = "colAccountCode";
            colAccountCode.Width = 120;
            // 
            // colAccountName
            // 
            colAccountName.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colAccountName.HeaderText = "اسم الحساب";
            colAccountName.MinimumWidth = 6;
            colAccountName.Name = "colAccountName";
            colAccountName.Width = 200;
            // 
            // colDescription
            // 
            colDescription.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colDescription.HeaderText = "البيان";
            colDescription.MinimumWidth = 6;
            colDescription.Name = "colDescription";
            colDescription.Width = 250;
            // 
            // colCostCenter
            // 
            colCostCenter.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colCostCenter.HeaderText = "مركز التكلفة";
            colCostCenter.MinimumWidth = 6;
            colCostCenter.Name = "colCostCenter";
            colCostCenter.Resizable = DataGridViewTriState.True;
            colCostCenter.SortMode = DataGridViewColumnSortMode.Automatic;
            colCostCenter.Width = 120;
            // 
            // colAmount
            // 
            colAmount.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colAmount.HeaderText = "المبلغ";
            colAmount.MinimumWidth = 6;
            colAmount.Name = "colAmount";
            colAmount.Width = 125;
            // 
            // colCurrency
            // 
            colCurrency.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colCurrency.HeaderText = "العملة";
            colCurrency.MinimumWidth = 6;
            colCurrency.Name = "colCurrency";
            colCurrency.Width = 80;
            // 
            // colExchangeRate
            // 
            colExchangeRate.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colExchangeRate.HeaderText = "سعر الصرف";
            colExchangeRate.MinimumWidth = 6;
            colExchangeRate.Name = "colExchangeRate";
            colExchangeRate.Width = 90;
            // 
            // colLocalAmount
            // 
            colLocalAmount.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colLocalAmount.HeaderText = "المبلغ بالعملة المحلية";
            colLocalAmount.MinimumWidth = 6;
            colLocalAmount.Name = "colLocalAmount";
            colLocalAmount.Width = 130;
            // 
            // colForeignAmount
            // 
            colForeignAmount.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colForeignAmount.HeaderText = "المبلغ بالعملة الاجنبية";
            colForeignAmount.MinimumWidth = 6;
            colForeignAmount.Name = "colForeignAmount";
            colForeignAmount.Width = 130;
            // 
            // colNotes
            // 
            colNotes.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colNotes.HeaderText = "ملاحظات";
            colNotes.MinimumWidth = 6;
            colNotes.Name = "colNotes";
            colNotes.Width = 200;
            // 
            // pnlTotals
            // 
            pnlTotals.Controls.Add(chkPosted);
            pnlTotals.Controls.Add(checkBox2);
            pnlTotals.Controls.Add(txtTotalForeignAmount);
            pnlTotals.Controls.Add(label16);
            pnlTotals.Controls.Add(txtJournalNo);
            pnlTotals.Controls.Add(label20);
            pnlTotals.Controls.Add(txtDifference);
            pnlTotals.Controls.Add(label21);
            pnlTotals.Controls.Add(txtTotalAmount);
            pnlTotals.Controls.Add(label22);
            pnlTotals.Dock = DockStyle.Bottom;
            pnlTotals.Location = new Point(3, 479);
            pnlTotals.Name = "pnlTotals";
            pnlTotals.Size = new Size(1406, 45);
            pnlTotals.TabIndex = 37;
            pnlTotals.Paint += pnlTotals_Paint;
            // 
            // chkPosted
            // 
            chkPosted.AutoSize = true;
            chkPosted.Location = new Point(46, 12);
            chkPosted.Name = "chkPosted";
            chkPosted.Size = new Size(65, 24);
            chkPosted.TabIndex = 69;
            chkPosted.Text = "مرحل";
            chkPosted.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(1206, 12);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(65, 24);
            checkBox2.TabIndex = 68;
            checkBox2.Text = "معلق";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // txtTotalForeignAmount
            // 
            txtTotalForeignAmount.Location = new Point(376, 8);
            txtTotalForeignAmount.Name = "txtTotalForeignAmount";
            txtTotalForeignAmount.RightToLeft = RightToLeft.No;
            txtTotalForeignAmount.Size = new Size(134, 27);
            txtTotalForeignAmount.TabIndex = 67;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(516, 11);
            label16.Name = "label16";
            label16.Size = new Size(142, 20);
            label16.TabIndex = 66;
            label16.Text = "اجمالى العملة الجنبية";
            // 
            // txtJournalNo
            // 
            txtJournalNo.Location = new Point(124, 8);
            txtJournalNo.Name = "txtJournalNo";
            txtJournalNo.RightToLeft = RightToLeft.No;
            txtJournalNo.Size = new Size(134, 27);
            txtJournalNo.TabIndex = 65;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(264, 11);
            label20.Name = "label20";
            label20.Size = new Size(65, 20);
            label20.TabIndex = 64;
            label20.Text = "رقم القيد";
            // 
            // txtDifference
            // 
            txtDifference.Location = new Point(673, 8);
            txtDifference.Name = "txtDifference";
            txtDifference.RightToLeft = RightToLeft.No;
            txtDifference.Size = new Size(134, 27);
            txtDifference.TabIndex = 63;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(813, 11);
            label21.Name = "label21";
            label21.Size = new Size(95, 20);
            label21.TabIndex = 62;
            label21.Text = "اجمالى الفارق";
            // 
            // txtTotalAmount
            // 
            txtTotalAmount.Location = new Point(922, 9);
            txtTotalAmount.Name = "txtTotalAmount";
            txtTotalAmount.RightToLeft = RightToLeft.No;
            txtTotalAmount.Size = new Size(134, 27);
            txtTotalAmount.TabIndex = 61;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(1062, 12);
            label22.Name = "label22";
            label22.Size = new Size(94, 20);
            label22.TabIndex = 60;
            label22.Text = "اجمالى المبلغ";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(numericUpDown1);
            groupBox1.Controls.Add(numericUpDown2);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label27);
            groupBox1.Controls.Add(numericUpDown3);
            groupBox1.Controls.Add(dateTimePicker1);
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Controls.Add(label28);
            groupBox1.Controls.Add(label29);
            groupBox1.Controls.Add(label30);
            groupBox1.Controls.Add(textBox2);
            groupBox1.Controls.Add(textBox3);
            groupBox1.Controls.Add(label31);
            groupBox1.Controls.Add(label32);
            groupBox1.Controls.Add(label33);
            groupBox1.Controls.Add(label34);
            groupBox1.Controls.Add(comboBox2);
            groupBox1.Controls.Add(label35);
            groupBox1.Controls.Add(label36);
            groupBox1.Controls.Add(label37);
            groupBox1.Controls.Add(comboBox3);
            groupBox1.Controls.Add(label38);
            groupBox1.Controls.Add(comboBox4);
            groupBox1.Controls.Add(comboBox5);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(0, 197);
            groupBox1.Name = "groupBox1";
            groupBox1.RightToLeft = RightToLeft.Yes;
            groupBox1.Size = new Size(1412, 191);
            groupBox1.TabIndex = 37;
            groupBox1.TabStop = false;
            groupBox1.Text = "بيانات قيد الصندوق";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(462, 14);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.RightToLeft = RightToLeft.No;
            numericUpDown1.Size = new Size(150, 27);
            numericUpDown1.TabIndex = 79;
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(462, 90);
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.RightToLeft = RightToLeft.No;
            numericUpDown2.Size = new Size(150, 27);
            numericUpDown2.TabIndex = 78;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(796, 126);
            textBox1.MaxLength = 500;
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.RightToLeft = RightToLeft.No;
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.Size = new Size(494, 55);
            textBox1.TabIndex = 77;
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new Point(1293, 129);
            label27.Name = "label27";
            label27.Size = new Size(57, 20);
            label27.TabIndex = 76;
            label27.Text = "ملاحظة";
            // 
            // numericUpDown3
            // 
            numericUpDown3.Location = new Point(462, 54);
            numericUpDown3.Name = "numericUpDown3";
            numericUpDown3.RightToLeft = RightToLeft.No;
            numericUpDown3.Size = new Size(150, 27);
            numericUpDown3.TabIndex = 37;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(83, 119);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(176, 27);
            dateTimePicker1.TabIndex = 37;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(77, 54);
            comboBox1.Name = "comboBox1";
            comboBox1.RightToLeft = RightToLeft.No;
            comboBox1.Size = new Size(183, 28);
            comboBox1.TabIndex = 75;
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Location = new Point(265, 56);
            label28.Name = "label28";
            label28.Size = new Size(96, 20);
            label28.TabIndex = 74;
            label28.Text = "طريقة السداد";
            // 
            // label29
            // 
            label29.AutoSize = true;
            label29.Location = new Point(614, 90);
            label29.Name = "label29";
            label29.Size = new Size(86, 20);
            label29.TabIndex = 72;
            label29.Text = "المبلغ اجنبي";
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.Location = new Point(616, 57);
            label30.Name = "label30";
            label30.Size = new Size(84, 20);
            label30.TabIndex = 70;
            label30.Text = "سعر الصرف";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(727, 93);
            textBox2.Name = "textBox2";
            textBox2.RightToLeft = RightToLeft.No;
            textBox2.Size = new Size(563, 27);
            textBox2.TabIndex = 61;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(83, 86);
            textBox3.Name = "textBox3";
            textBox3.RightToLeft = RightToLeft.No;
            textBox3.Size = new Size(172, 27);
            textBox3.TabIndex = 61;
            // 
            // label31
            // 
            label31.AutoSize = true;
            label31.Location = new Point(259, 122);
            label31.Name = "label31";
            label31.Size = new Size(85, 20);
            label31.TabIndex = 58;
            label31.Text = "تاريخ المرجع";
            // 
            // label32
            // 
            label32.AutoSize = true;
            label32.Location = new Point(259, 88);
            label32.Name = "label32";
            label32.Size = new Size(77, 20);
            label32.TabIndex = 58;
            label32.Text = "رقم المرجع";
            // 
            // label33
            // 
            label33.AutoSize = true;
            label33.Location = new Point(1293, 96);
            label33.Name = "label33";
            label33.Size = new Size(83, 20);
            label33.TabIndex = 58;
            label33.Text = "وذلك مقابل";
            // 
            // label34
            // 
            label34.AutoSize = true;
            label34.Location = new Point(616, 21);
            label34.Name = "label34";
            label34.Size = new Size(86, 20);
            label34.TabIndex = 58;
            label34.Text = "المبلغ محلية";
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(77, 21);
            comboBox2.Name = "comboBox2";
            comboBox2.RightToLeft = RightToLeft.No;
            comboBox2.Size = new Size(186, 28);
            comboBox2.TabIndex = 66;
            // 
            // label35
            // 
            label35.AutoSize = true;
            label35.Location = new Point(268, 23);
            label35.Name = "label35";
            label35.Size = new Size(84, 20);
            label35.TabIndex = 65;
            label35.Text = "مركز التكلفة";
            // 
            // label36
            // 
            label36.AutoSize = true;
            label36.Location = new Point(1253, 55);
            label36.Name = "label36";
            label36.Size = new Size(124, 20);
            label36.TabIndex = 64;
            label36.Text = "صرف لصالح السيد";
            // 
            // label37
            // 
            label37.AutoSize = true;
            label37.Location = new Point(1269, 21);
            label37.Name = "label37";
            label37.Size = new Size(91, 20);
            label37.TabIndex = 64;
            label37.Text = "رقم الصندوق";
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(752, 19);
            comboBox3.Name = "comboBox3";
            comboBox3.RightToLeft = RightToLeft.No;
            comboBox3.Size = new Size(149, 28);
            comboBox3.TabIndex = 57;
            // 
            // label38
            // 
            label38.AutoSize = true;
            label38.Location = new Point(906, 21);
            label38.Name = "label38";
            label38.Size = new Size(49, 20);
            label38.TabIndex = 54;
            label38.Text = "العمله";
            // 
            // comboBox4
            // 
            comboBox4.FormattingEnabled = true;
            comboBox4.Location = new Point(893, 55);
            comboBox4.Name = "comboBox4";
            comboBox4.RightToLeft = RightToLeft.No;
            comboBox4.Size = new Size(354, 28);
            comboBox4.TabIndex = 56;
            // 
            // comboBox5
            // 
            comboBox5.FormattingEnabled = true;
            comboBox5.Location = new Point(997, 21);
            comboBox5.Name = "comboBox5";
            comboBox5.RightToLeft = RightToLeft.No;
            comboBox5.Size = new Size(266, 28);
            comboBox5.TabIndex = 56;
            // 
            // FrmPaymentVoucher
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1412, 792);
            Controls.Add(groupBox1);
            Controls.Add(grpDistribution);
            Controls.Add(pnlUserInfo);
            Controls.Add(grpVoucherInfo);
            Controls.Add(pnlToolbar);
            Controls.Add(statusSystem);
            Controls.Add(pnlTopBar);
            Name = "FrmPaymentVoucher";
            Text = "سند الصرف";
            statusSystem.ResumeLayout(false);
            statusSystem.PerformLayout();
            pnlTopBar.ResumeLayout(false);
            pnlTopBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).EndInit();
            pnlToolbar.ResumeLayout(false);
            grpVoucherInfo.ResumeLayout(false);
            grpVoucherInfo.PerformLayout();
            pnlUserInfo.ResumeLayout(false);
            pnlUserInfo.PerformLayout();
            grpDistribution.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvVoucherDetails).EndInit();
            pnlTotals.ResumeLayout(false);
            pnlTotals.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel statusSystem;
        private Label lblStatusTime;
        private Label lblStatusLicense;
        private Label lblStatusApi;
        private Label lblVersion;
        private Label lblStatusDatabase;
        private Panel pnlTopBar;
        private Button btnClose;
        private PictureBox picCompanyLogo;
        private Label lblCurrentUser;
        private Label lblCompanyName;
        private Label lblFiscalYear;
        private Label lblCurrentBranch;
        private GroupBox grpDistribution;
        private GroupBox grpVoucherInfo;
        private Panel pnlTotals;
        private Panel pnlToolbar;
        private Panel pnlUserInfo;
        private TextBox txtCreatedBy;
        private Label label7;
        private TextBox txtUpdatedBy;
        private Label label3;
        private TextBox txtUpdatedDate;
        private Label label2;
        private TextBox txtCreatedDate;
        private Label label1;
        private Button btnImport;
        private Button btnExport;
        private Button btnAttachments;
        private Button btnPrint;
        private Button btnCancelApprove;
        private Button btnUnPost;
        private Button btnApprove;
        private Button btnPost;
        private Button btnRefresh;
        private Button btnSearch;
        private Button btnDelete;
        private Button btnEdit;
        private Button btnNew;
        private Button btnSave;
        private ComboBox cmbVoucherType;
        private Label label11;
        private Label label6;
        private TextBox txtVoucherNo;
        private Label label4;
        private ComboBox cmbBranch;
        private Label label9;
        private ComboBox cmbStatus;
        private Label label14;
        private Label label13;
        private TextBox txtReferenceNo;
        private CheckBox checkBox2;
        private CheckBox chkIsActive;
        private ComboBox comboBox6;
        private Label label16;
        private TextBox txtTotalForeignAmount;
        private TextBox txtJournalNo;
        private Label label20;
        private TextBox txtDifference;
        private Label label21;
        private TextBox txtTotalAmount;
        private Label label22;
        private DateTimePicker dtVoucherDate;
        private DataGridView dgvVoucherDetails;
        private CheckBox chkPosted;
        private DataGridViewTextBoxColumn colNo;
        private DataGridViewComboBoxColumn colAccountCode;
        private DataGridViewComboBoxColumn colAccountName;
        private DataGridViewTextBoxColumn colDescription;
        private DataGridViewComboBoxColumn colCostCenter;
        private DataGridViewTextBoxColumn colAmount;
        private DataGridViewComboBoxColumn colCurrency;
        private DataGridViewTextBoxColumn colExchangeRate;
        private DataGridViewTextBoxColumn colLocalAmount;
        private DataGridViewTextBoxColumn colForeignAmount;
        private DataGridViewTextBoxColumn colNotes;
        private GroupBox groupBox1;
        private NumericUpDown numericUpDown1;
        private NumericUpDown numericUpDown2;
        private TextBox textBox1;
        private Label label27;
        private NumericUpDown numericUpDown3;
        private DateTimePicker dateTimePicker1;
        private ComboBox comboBox1;
        private Label label28;
        private Label label29;
        private Label label30;
        private TextBox textBox2;
        private TextBox textBox3;
        private Label label31;
        private Label label32;
        private Label label33;
        private Label label34;
        private ComboBox comboBox2;
        private Label label35;
        private Label label36;
        private Label label37;
        private ComboBox comboBox3;
        private Label label38;
        private ComboBox comboBox4;
        private ComboBox comboBox5;
    }
}