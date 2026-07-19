namespace AlTayerERP.Desktop
{
    partial class FrmReceiptVoucher
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
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
            btnViewJournalEntry = new Button();
            btnUndo = new Button();
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
            txtLastPrintDate = new TextBox();
            label30 = new Label();
            txtLastPrintedBy = new TextBox();
            label31 = new Label();
            txtEditCount = new TextBox();
            txtUpdatedBy = new TextBox();
            label29 = new Label();
            label3 = new Label();
            txtPrintCount = new TextBox();
            label28 = new Label();
            txtUpdatedDate = new TextBox();
            label2 = new Label();
            txtCreatedDate = new TextBox();
            label1 = new Label();
            txtCreatedBy = new TextBox();
            label7 = new Label();
            groupBox1 = new GroupBox();
            numAmount = new NumericUpDown();
            label15 = new Label();
            numLocalAmount = new NumericUpDown();
            numForeignAmount = new NumericUpDown();
            txtHeaderNotes = new TextBox();
            label27 = new Label();
            numExchangeRate = new NumericUpDown();
            dtReferenceDate = new DateTimePicker();
            cmbPaymentMethod = new ComboBox();
            label5 = new Label();
            label8 = new Label();
            label10 = new Label();
            txtAgainst = new TextBox();
            txtReference = new TextBox();
            label12 = new Label();
            label32 = new Label();
            label33 = new Label();
            label34 = new Label();
            cmbCostCenter = new ComboBox();
            label35 = new Label();
            label36 = new Label();
            label37 = new Label();
            cmbCurrency = new ComboBox();
            label38 = new Label();
            cmbParty = new ComboBox();
            cmbCashAccount = new ComboBox();
            grpDistribution = new GroupBox();
            button1 = new Button();
            dgvVoucherDetails = new DataGridView();
            colNo = new DataGridViewTextBoxColumn();
            colAccountCode = new DataGridViewComboBoxColumn();
            colAccountName = new DataGridViewComboBoxColumn();
            colDescription = new DataGridViewTextBoxColumn();
            colCostCenter = new DataGridViewComboBoxColumn();
            colReferenceNo = new DataGridViewTextBoxColumn();
            colReferenceName = new DataGridViewTextBoxColumn();
            colReferenceType = new DataGridViewTextBoxColumn();
            colReferenceDate = new DataGridViewTextBoxColumn();
            colAmount = new DataGridViewTextBoxColumn();
            colCurrency = new DataGridViewComboBoxColumn();
            colExchangeRate = new DataGridViewTextBoxColumn();
            colForeignAmount = new DataGridViewTextBoxColumn();
            colLocalAmount = new DataGridViewTextBoxColumn();
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
            statusSystem.SuspendLayout();
            pnlTopBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).BeginInit();
            pnlToolbar.SuspendLayout();
            grpVoucherInfo.SuspendLayout();
            pnlUserInfo.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numAmount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numLocalAmount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numForeignAmount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numExchangeRate).BeginInit();
            grpDistribution.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVoucherDetails).BeginInit();
            pnlTotals.SuspendLayout();
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
            statusSystem.Location = new Point(0, 745);
            statusSystem.Name = "statusSystem";
            statusSystem.Size = new Size(1444, 35);
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
            pnlTopBar.Size = new Size(1444, 59);
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
            picCompanyLogo.Location = new Point(1194, 8);
            picCompanyLogo.Name = "picCompanyLogo";
            picCompanyLogo.Size = new Size(180, 45);
            picCompanyLogo.TabIndex = 1;
            picCompanyLogo.TabStop = false;
            // 
            // lblCurrentUser
            // 
            lblCurrentUser.AutoSize = true;
            lblCurrentUser.Location = new Point(231, 9);
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new Size(85, 40);
            lblCurrentUser.TabIndex = 0;
            lblCurrentUser.Text = "المستخدم:\r\n مدير النظام";
            lblCurrentUser.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCompanyName
            // 
            lblCompanyName.AutoSize = true;
            lblCompanyName.Location = new Point(1029, 8);
            lblCompanyName.Name = "lblCompanyName";
            lblCompanyName.Size = new Size(121, 40);
            lblCompanyName.TabIndex = 0;
            lblCompanyName.Text = "شركة\r\n الطائر للنقل البري";
            lblCompanyName.TextAlign = ContentAlignment.MiddleCenter;
            lblCompanyName.Click += lblCompanyName_Click;
            // 
            // lblFiscalYear
            // 
            lblFiscalYear.AutoSize = true;
            lblFiscalYear.Location = new Point(500, 10);
            lblFiscalYear.Name = "lblFiscalYear";
            lblFiscalYear.Size = new Size(95, 40);
            lblFiscalYear.TabIndex = 0;
            lblFiscalYear.Text = "السنة المالية :\r\n 2026";
            lblFiscalYear.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCurrentBranch
            // 
            lblCurrentBranch.AutoSize = true;
            lblCurrentBranch.Location = new Point(789, 8);
            lblCurrentBranch.Name = "lblCurrentBranch";
            lblCurrentBranch.Size = new Size(101, 40);
            lblCurrentBranch.TabIndex = 0;
            lblCurrentBranch.Text = "الفرع \r\nعدن_المنصورة";
            lblCurrentBranch.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = SystemColors.ActiveCaption;
            pnlToolbar.Controls.Add(btnViewJournalEntry);
            pnlToolbar.Controls.Add(btnUndo);
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
            pnlToolbar.Size = new Size(1444, 50);
            pnlToolbar.TabIndex = 32;
            pnlToolbar.Paint += pnlToolbar_Paint;
            // 
            // btnViewJournalEntry
            // 
            btnViewJournalEntry.Location = new Point(138, 12);
            btnViewJournalEntry.Name = "btnViewJournalEntry";
            btnViewJournalEntry.RightToLeft = RightToLeft.No;
            btnViewJournalEntry.Size = new Size(123, 29);
            btnViewJournalEntry.TabIndex = 67;
            btnViewJournalEntry.Text = "استعراض القيد";
            btnViewJournalEntry.UseVisualStyleBackColor = true;
            btnViewJournalEntry.Click += btnViewJournalEntry_Click;
            // 
            // btnUndo
            // 
            btnUndo.Location = new Point(282, 15);
            btnUndo.Name = "btnUndo";
            btnUndo.RightToLeft = RightToLeft.No;
            btnUndo.Size = new Size(62, 29);
            btnUndo.TabIndex = 66;
            btnUndo.Text = "تراجع";
            btnUndo.UseVisualStyleBackColor = true;
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
            btnSearch.Location = new Point(1212, 12);
            btnSearch.Name = "btnSearch";
            btnSearch.RightToLeft = RightToLeft.No;
            btnSearch.Size = new Size(62, 29);
            btnSearch.TabIndex = 54;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(1043, 10);
            btnDelete.Name = "btnDelete";
            btnDelete.RightToLeft = RightToLeft.No;
            btnDelete.Size = new Size(62, 29);
            btnDelete.TabIndex = 55;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(1122, 10);
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
            grpVoucherInfo.Size = new Size(1444, 88);
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
            txtReferenceNo.TextAlign = HorizontalAlignment.Right;
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
            txtVoucherNo.TextAlign = HorizontalAlignment.Right;
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
            pnlUserInfo.Controls.Add(txtLastPrintDate);
            pnlUserInfo.Controls.Add(label30);
            pnlUserInfo.Controls.Add(txtLastPrintedBy);
            pnlUserInfo.Controls.Add(label31);
            pnlUserInfo.Controls.Add(txtEditCount);
            pnlUserInfo.Controls.Add(txtUpdatedBy);
            pnlUserInfo.Controls.Add(label29);
            pnlUserInfo.Controls.Add(label3);
            pnlUserInfo.Controls.Add(txtPrintCount);
            pnlUserInfo.Controls.Add(label28);
            pnlUserInfo.Controls.Add(txtUpdatedDate);
            pnlUserInfo.Controls.Add(label2);
            pnlUserInfo.Controls.Add(txtCreatedDate);
            pnlUserInfo.Controls.Add(label1);
            pnlUserInfo.Controls.Add(txtCreatedBy);
            pnlUserInfo.Controls.Add(label7);
            pnlUserInfo.Dock = DockStyle.Bottom;
            pnlUserInfo.Location = new Point(0, 667);
            pnlUserInfo.Name = "pnlUserInfo";
            pnlUserInfo.Size = new Size(1444, 78);
            pnlUserInfo.TabIndex = 34;
            pnlUserInfo.Paint += pnlUserInfo_Paint;
            // 
            // txtLastPrintDate
            // 
            txtLastPrintDate.Location = new Point(93, 7);
            txtLastPrintDate.Name = "txtLastPrintDate";
            txtLastPrintDate.RightToLeft = RightToLeft.No;
            txtLastPrintDate.Size = new Size(134, 27);
            txtLastPrintDate.TabIndex = 63;
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.Location = new Point(230, 10);
            label30.Name = "label30";
            label30.Size = new Size(71, 20);
            label30.TabIndex = 62;
            label30.Text = "اخر طباعة";
            // 
            // txtLastPrintedBy
            // 
            txtLastPrintedBy.Location = new Point(93, 40);
            txtLastPrintedBy.Name = "txtLastPrintedBy";
            txtLastPrintedBy.RightToLeft = RightToLeft.No;
            txtLastPrintedBy.Size = new Size(134, 27);
            txtLastPrintedBy.TabIndex = 61;
            // 
            // label31
            // 
            label31.AutoSize = true;
            label31.Location = new Point(230, 43);
            label31.Name = "label31";
            label31.Size = new Size(141, 20);
            label31.TabIndex = 60;
            label31.Text = "اخر مستخدم  الطباعة";
            // 
            // txtEditCount
            // 
            txtEditCount.Location = new Point(385, 4);
            txtEditCount.Name = "txtEditCount";
            txtEditCount.RightToLeft = RightToLeft.No;
            txtEditCount.Size = new Size(134, 27);
            txtEditCount.TabIndex = 59;
            // 
            // txtUpdatedBy
            // 
            txtUpdatedBy.Location = new Point(694, 3);
            txtUpdatedBy.Name = "txtUpdatedBy";
            txtUpdatedBy.RightToLeft = RightToLeft.No;
            txtUpdatedBy.Size = new Size(184, 27);
            txtUpdatedBy.TabIndex = 59;
            // 
            // label29
            // 
            label29.AutoSize = true;
            label29.Location = new Point(522, 7);
            label29.Name = "label29";
            label29.Size = new Size(97, 20);
            label29.TabIndex = 58;
            label29.Text = "عدد التعديلات";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(884, 6);
            label3.Name = "label3";
            label3.Size = new Size(102, 20);
            label3.TabIndex = 58;
            label3.Text = "مستخدم معدل";
            // 
            // txtPrintCount
            // 
            txtPrintCount.Location = new Point(385, 37);
            txtPrintCount.Name = "txtPrintCount";
            txtPrintCount.RightToLeft = RightToLeft.No;
            txtPrintCount.Size = new Size(134, 27);
            txtPrintCount.TabIndex = 57;
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Location = new Point(522, 40);
            label28.Name = "label28";
            label28.Size = new Size(86, 20);
            label28.TabIndex = 56;
            label28.Text = "عدد الطباعة";
            // 
            // txtUpdatedDate
            // 
            txtUpdatedDate.Location = new Point(696, 36);
            txtUpdatedDate.Name = "txtUpdatedDate";
            txtUpdatedDate.RightToLeft = RightToLeft.No;
            txtUpdatedDate.Size = new Size(184, 27);
            txtUpdatedDate.TabIndex = 57;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(886, 39);
            label2.Name = "label2";
            label2.Size = new Size(89, 20);
            label2.TabIndex = 56;
            label2.Text = "تاريخ التعديل";
            // 
            // txtCreatedDate
            // 
            txtCreatedDate.Location = new Point(1204, 40);
            txtCreatedDate.Name = "txtCreatedDate";
            txtCreatedDate.RightToLeft = RightToLeft.No;
            txtCreatedDate.Size = new Size(134, 27);
            txtCreatedDate.TabIndex = 55;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(1344, 43);
            label1.Name = "label1";
            label1.Size = new Size(91, 20);
            label1.TabIndex = 54;
            label1.Text = "تاريخ الاضافه";
            // 
            // txtCreatedBy
            // 
            txtCreatedBy.Location = new Point(1204, 4);
            txtCreatedBy.Name = "txtCreatedBy";
            txtCreatedBy.RightToLeft = RightToLeft.No;
            txtCreatedBy.Size = new Size(134, 27);
            txtCreatedBy.TabIndex = 53;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(1344, 7);
            label7.Name = "label7";
            label7.Size = new Size(71, 20);
            label7.TabIndex = 52;
            label7.Text = "المستخدم";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(numAmount);
            groupBox1.Controls.Add(label15);
            groupBox1.Controls.Add(numLocalAmount);
            groupBox1.Controls.Add(numForeignAmount);
            groupBox1.Controls.Add(txtHeaderNotes);
            groupBox1.Controls.Add(label27);
            groupBox1.Controls.Add(numExchangeRate);
            groupBox1.Controls.Add(dtReferenceDate);
            groupBox1.Controls.Add(cmbPaymentMethod);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(txtAgainst);
            groupBox1.Controls.Add(txtReference);
            groupBox1.Controls.Add(label12);
            groupBox1.Controls.Add(label32);
            groupBox1.Controls.Add(label33);
            groupBox1.Controls.Add(label34);
            groupBox1.Controls.Add(cmbCostCenter);
            groupBox1.Controls.Add(label35);
            groupBox1.Controls.Add(label36);
            groupBox1.Controls.Add(label37);
            groupBox1.Controls.Add(cmbCurrency);
            groupBox1.Controls.Add(label38);
            groupBox1.Controls.Add(cmbParty);
            groupBox1.Controls.Add(cmbCashAccount);
            groupBox1.Dock = DockStyle.Top;
            groupBox1.Location = new Point(0, 197);
            groupBox1.Name = "groupBox1";
            groupBox1.RightToLeft = RightToLeft.Yes;
            groupBox1.Size = new Size(1444, 191);
            groupBox1.TabIndex = 38;
            groupBox1.TabStop = false;
            groupBox1.Text = "بيانات قيد الصندوق";
            groupBox1.Enter += groupBox1_Enter;
            // 
            // numAmount
            // 
            numAmount.DecimalPlaces = 2;
            numAmount.Location = new Point(462, 142);
            numAmount.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numAmount.Name = "numAmount";
            numAmount.RightToLeft = RightToLeft.No;
            numAmount.Size = new Size(150, 27);
            numAmount.TabIndex = 81;
            numAmount.TextAlign = HorizontalAlignment.Center;
            numAmount.ValueChanged += numAmount_ValueChanged_1;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(614, 142);
            label15.Name = "label15";
            label15.Size = new Size(46, 20);
            label15.TabIndex = 80;
            label15.Text = "المبلغ";
            // 
            // numLocalAmount
            // 
            numLocalAmount.DecimalPlaces = 2;
            numLocalAmount.Location = new Point(462, 14);
            numLocalAmount.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numLocalAmount.Name = "numLocalAmount";
            numLocalAmount.RightToLeft = RightToLeft.No;
            numLocalAmount.Size = new Size(150, 27);
            numLocalAmount.TabIndex = 79;
            numLocalAmount.TextAlign = HorizontalAlignment.Center;
            // 
            // numForeignAmount
            // 
            numForeignAmount.DecimalPlaces = 2;
            numForeignAmount.Location = new Point(462, 90);
            numForeignAmount.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numForeignAmount.Name = "numForeignAmount";
            numForeignAmount.RightToLeft = RightToLeft.No;
            numForeignAmount.Size = new Size(150, 27);
            numForeignAmount.TabIndex = 78;
            numForeignAmount.TextAlign = HorizontalAlignment.Center;
            // 
            // txtHeaderNotes
            // 
            txtHeaderNotes.Location = new Point(796, 126);
            txtHeaderNotes.MaxLength = 500;
            txtHeaderNotes.Multiline = true;
            txtHeaderNotes.Name = "txtHeaderNotes";
            txtHeaderNotes.RightToLeft = RightToLeft.No;
            txtHeaderNotes.ScrollBars = ScrollBars.Vertical;
            txtHeaderNotes.Size = new Size(494, 55);
            txtHeaderNotes.TabIndex = 77;
            txtHeaderNotes.TextAlign = HorizontalAlignment.Right;
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
            // numExchangeRate
            // 
            numExchangeRate.DecimalPlaces = 2;
            numExchangeRate.Location = new Point(462, 54);
            numExchangeRate.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numExchangeRate.Name = "numExchangeRate";
            numExchangeRate.RightToLeft = RightToLeft.No;
            numExchangeRate.Size = new Size(150, 27);
            numExchangeRate.TabIndex = 37;
            numExchangeRate.TextAlign = HorizontalAlignment.Center;
            // 
            // dtReferenceDate
            // 
            dtReferenceDate.Location = new Point(83, 119);
            dtReferenceDate.Name = "dtReferenceDate";
            dtReferenceDate.Size = new Size(176, 27);
            dtReferenceDate.TabIndex = 37;
            // 
            // cmbPaymentMethod
            // 
            cmbPaymentMethod.FormattingEnabled = true;
            cmbPaymentMethod.Location = new Point(77, 54);
            cmbPaymentMethod.Name = "cmbPaymentMethod";
            cmbPaymentMethod.RightToLeft = RightToLeft.No;
            cmbPaymentMethod.Size = new Size(183, 28);
            cmbPaymentMethod.TabIndex = 75;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(265, 56);
            label5.Name = "label5";
            label5.Size = new Size(96, 20);
            label5.TabIndex = 74;
            label5.Text = "طريقة السداد";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(614, 90);
            label8.Name = "label8";
            label8.Size = new Size(86, 20);
            label8.TabIndex = 72;
            label8.Text = "المبلغ اجنبي";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(616, 57);
            label10.Name = "label10";
            label10.Size = new Size(84, 20);
            label10.TabIndex = 70;
            label10.Text = "سعر الصرف";
            // 
            // txtAgainst
            // 
            txtAgainst.Location = new Point(727, 93);
            txtAgainst.Name = "txtAgainst";
            txtAgainst.RightToLeft = RightToLeft.No;
            txtAgainst.Size = new Size(563, 27);
            txtAgainst.TabIndex = 61;
            txtAgainst.TextAlign = HorizontalAlignment.Right;
            // 
            // txtReference
            // 
            txtReference.Location = new Point(83, 86);
            txtReference.Name = "txtReference";
            txtReference.RightToLeft = RightToLeft.No;
            txtReference.Size = new Size(172, 27);
            txtReference.TabIndex = 61;
            txtReference.TextAlign = HorizontalAlignment.Right;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(259, 122);
            label12.Name = "label12";
            label12.Size = new Size(85, 20);
            label12.TabIndex = 58;
            label12.Text = "تاريخ المرجع";
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
            // cmbCostCenter
            // 
            cmbCostCenter.FormattingEnabled = true;
            cmbCostCenter.Location = new Point(77, 21);
            cmbCostCenter.Name = "cmbCostCenter";
            cmbCostCenter.RightToLeft = RightToLeft.No;
            cmbCostCenter.Size = new Size(186, 28);
            cmbCostCenter.TabIndex = 66;
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
            label36.Size = new Size(121, 20);
            label36.TabIndex = 64;
            label36.Text = "استلمت من السيد";
            label36.Click += label36_Click;
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
            // cmbCurrency
            // 
            cmbCurrency.FormattingEnabled = true;
            cmbCurrency.Location = new Point(752, 19);
            cmbCurrency.Name = "cmbCurrency";
            cmbCurrency.RightToLeft = RightToLeft.No;
            cmbCurrency.Size = new Size(149, 28);
            cmbCurrency.TabIndex = 57;
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
            // cmbParty
            // 
            cmbParty.FormattingEnabled = true;
            cmbParty.Location = new Point(893, 55);
            cmbParty.Name = "cmbParty";
            cmbParty.RightToLeft = RightToLeft.No;
            cmbParty.Size = new Size(354, 28);
            cmbParty.TabIndex = 56;
            // 
            // cmbCashAccount
            // 
            cmbCashAccount.FormattingEnabled = true;
            cmbCashAccount.Location = new Point(997, 21);
            cmbCashAccount.Name = "cmbCashAccount";
            cmbCashAccount.RightToLeft = RightToLeft.No;
            cmbCashAccount.Size = new Size(266, 28);
            cmbCashAccount.TabIndex = 56;
            // 
            // grpDistribution
            // 
            grpDistribution.Controls.Add(button1);
            grpDistribution.Controls.Add(dgvVoucherDetails);
            grpDistribution.Controls.Add(pnlTotals);
            grpDistribution.Dock = DockStyle.Fill;
            grpDistribution.Location = new Point(0, 388);
            grpDistribution.Name = "grpDistribution";
            grpDistribution.RightToLeft = RightToLeft.Yes;
            grpDistribution.Size = new Size(1444, 279);
            grpDistribution.TabIndex = 39;
            grpDistribution.TabStop = false;
            grpDistribution.Text = "التوزيع المحاسبي";
            // 
            // button1
            // 
            button1.Location = new Point(696, 0);
            button1.Name = "button1";
            button1.RightToLeft = RightToLeft.No;
            button1.Size = new Size(77, 29);
            button1.TabIndex = 62;
            button1.Text = "بحث حساب";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // dgvVoucherDetails
            // 
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvVoucherDetails.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvVoucherDetails.ColumnHeadersHeight = 35;
            dgvVoucherDetails.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvVoucherDetails.Columns.AddRange(new DataGridViewColumn[] { colNo, colAccountCode, colAccountName, colDescription, colCostCenter, colReferenceNo, colReferenceName, colReferenceType, colReferenceDate, colAmount, colCurrency, colExchangeRate, colForeignAmount, colLocalAmount, colNotes });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ActiveCaptionText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvVoucherDetails.DefaultCellStyle = dataGridViewCellStyle2;
            dgvVoucherDetails.Dock = DockStyle.Fill;
            dgvVoucherDetails.Location = new Point(3, 23);
            dgvVoucherDetails.Name = "dgvVoucherDetails";
            dgvVoucherDetails.RowHeadersWidth = 51;
            dgvVoucherDetails.Size = new Size(1438, 208);
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
            colAccountCode.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
            colAccountCode.DropDownWidth = 150;
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
            // colReferenceNo
            // 
            colReferenceNo.HeaderText = "رقم المرجع";
            colReferenceNo.MinimumWidth = 6;
            colReferenceNo.Name = "colReferenceNo";
            colReferenceNo.Width = 125;
            // 
            // colReferenceName
            // 
            colReferenceName.HeaderText = "اسم المرجع";
            colReferenceName.MinimumWidth = 6;
            colReferenceName.Name = "colReferenceName";
            colReferenceName.Resizable = DataGridViewTriState.True;
            colReferenceName.SortMode = DataGridViewColumnSortMode.Automatic;
            colReferenceName.Width = 125;
            // 
            // colReferenceType
            // 
            colReferenceType.HeaderText = "نوع المرجع";
            colReferenceType.MinimumWidth = 6;
            colReferenceType.Name = "colReferenceType";
            colReferenceType.Resizable = DataGridViewTriState.True;
            colReferenceType.SortMode = DataGridViewColumnSortMode.Automatic;
            colReferenceType.Width = 125;
            // 
            // colReferenceDate
            // 
            colReferenceDate.HeaderText = "تاريخ المرجع";
            colReferenceDate.MinimumWidth = 6;
            colReferenceDate.Name = "colReferenceDate";
            colReferenceDate.Width = 125;
            // 
            // colAmount
            // 
            colAmount.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colAmount.DividerWidth = 2;
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
            colCurrency.Width = 125;
            // 
            // colExchangeRate
            // 
            colExchangeRate.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colExchangeRate.HeaderText = "سعر الصرف";
            colExchangeRate.MinimumWidth = 6;
            colExchangeRate.Name = "colExchangeRate";
            colExchangeRate.Width = 90;
            // 
            // colForeignAmount
            // 
            colForeignAmount.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colForeignAmount.HeaderText = "المبلغ بالعملة الاجنبية";
            colForeignAmount.MinimumWidth = 6;
            colForeignAmount.Name = "colForeignAmount";
            colForeignAmount.Width = 170;
            // 
            // colLocalAmount
            // 
            colLocalAmount.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colLocalAmount.DividerWidth = 2;
            colLocalAmount.HeaderText = "المبلغ بالعملة المحلية";
            colLocalAmount.MinimumWidth = 6;
            colLocalAmount.Name = "colLocalAmount";
            colLocalAmount.Width = 170;
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
            pnlTotals.Location = new Point(3, 231);
            pnlTotals.Name = "pnlTotals";
            pnlTotals.Size = new Size(1438, 45);
            pnlTotals.TabIndex = 37;
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
            // FrmReceiptVoucher
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(224, 224, 224);
            ClientSize = new Size(1444, 780);
            Controls.Add(grpDistribution);
            Controls.Add(groupBox1);
            Controls.Add(pnlUserInfo);
            Controls.Add(grpVoucherInfo);
            Controls.Add(pnlToolbar);
            Controls.Add(statusSystem);
            Controls.Add(pnlTopBar);
            ForeColor = SystemColors.ActiveCaptionText;
            Name = "FrmReceiptVoucher";
            Text = "سند القبض";
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
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numAmount).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLocalAmount).EndInit();
            ((System.ComponentModel.ISupportInitialize)numForeignAmount).EndInit();
            ((System.ComponentModel.ISupportInitialize)numExchangeRate).EndInit();
            grpDistribution.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvVoucherDetails).EndInit();
            pnlTotals.ResumeLayout(false);
            pnlTotals.PerformLayout();
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
        private GroupBox grpVoucherInfo;
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
        private CheckBox chkIsActive;
        private ComboBox comboBox6;
        private DateTimePicker dtVoucherDate;
        private TextBox txtLastPrintedBy;
        private Label label29;
        private TextBox txtPrintCount;
        private Label label28;
        private Button btnUndo;
        private TextBox txtEditCount;
        private TextBox txtLastPrintDate;
        private Label label30;
        private Label label31;
        private GroupBox groupBox1;
        private NumericUpDown numLocalAmount;
        private NumericUpDown numForeignAmount;
        private TextBox txtHeaderNotes;
        private Label label27;
        private NumericUpDown numExchangeRate;
        private DateTimePicker dtReferenceDate;
        private ComboBox cmbPaymentMethod;
        private Label label5;
        private Label label8;
        private Label label10;
        private TextBox txtAgainst;
        private TextBox txtReference;
        private Label label12;
        private Label label32;
        private Label label33;
        private Label label34;
        private ComboBox cmbCostCenter;
        private Label label35;
        private Label label36;
        private Label label37;
        private ComboBox cmbCurrency;
        private Label label38;
        private ComboBox cmbParty;
        private ComboBox cmbCashAccount;
        private GroupBox grpDistribution;
        private DataGridView dgvVoucherDetails;
        private Panel pnlTotals;
        private CheckBox chkPosted;
        private CheckBox checkBox2;
        private TextBox txtTotalForeignAmount;
        private Label label16;
        private TextBox txtJournalNo;
        private Label label20;
        private TextBox txtDifference;
        private Label label21;
        private TextBox txtTotalAmount;
        private Label label22;
        private NumericUpDown numAmount;
        private Label label15;
        private Button btnViewJournalEntry;
        private DataGridViewTextBoxColumn colNo;
        private DataGridViewComboBoxColumn colAccountCode;
        private DataGridViewComboBoxColumn colAccountName;
        private DataGridViewTextBoxColumn colDescription;
        private DataGridViewComboBoxColumn colCostCenter;
        private DataGridViewTextBoxColumn colReferenceNo;
        private DataGridViewTextBoxColumn colReferenceName;
        private DataGridViewTextBoxColumn colReferenceType;
        private DataGridViewTextBoxColumn colReferenceDate;
        private DataGridViewTextBoxColumn colAmount;
        private DataGridViewComboBoxColumn colCurrency;
        private DataGridViewTextBoxColumn colExchangeRate;
        private DataGridViewTextBoxColumn colForeignAmount;
        private DataGridViewTextBoxColumn colLocalAmount;
        private DataGridViewTextBoxColumn colNotes;
        private Button button1;
    }
}
