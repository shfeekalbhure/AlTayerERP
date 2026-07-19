namespace AlTayerERP.Desktop
{
    partial class FrmCashBoxes
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
            pnlStatusBar = new Panel();
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
            btnRefresh = new Button();
            btnSearch = new Button();
            btnNew = new Button();
            btnDelete = new Button();
            btnSave = new Button();
            btnEdit = new Button();
            groupBox1 = new GroupBox();
            txtSearch = new TextBox();
            panel1 = new Panel();
            dgvCashBoxes = new DataGridView();
            colCashBoxCode = new DataGridViewTextBoxColumn();
            colCashBoxNameAR = new DataGridViewTextBoxColumn();
            colCashBoxNameEN = new DataGridViewTextBoxColumn();
            colAccount = new DataGridViewTextBoxColumn();
            colCurrency = new DataGridViewTextBoxColumn();
            colBranch = new DataGridViewTextBoxColumn();
            colOpeningBalance = new DataGridViewTextBoxColumn();
            colMaximumLimit = new DataGridViewTextBoxColumn();
            colMinimumLimit = new DataGridViewTextBoxColumn();
            colIsActive = new DataGridViewTextBoxColumn();
            label9 = new Label();
            grpActions = new GroupBox();
            btnPrint = new Button();
            grpAdditionalData = new GroupBox();
            numOpeningBalance = new NumericUpDown();
            numMinimumLimit = new NumericUpDown();
            numMaximumLimit = new NumericUpDown();
            dgvCashBoxCurrencies = new DataGridView();
            label2 = new Label();
            label13 = new Label();
            label1 = new Label();
            grpCashBox = new GroupBox();
            label11 = new Label();
            chkIsActive = new CheckBox();
            label8 = new Label();
            txtCashBoxNameEN = new TextBox();
            label5 = new Label();
            txtNotes = new TextBox();
            txtCashBoxCode = new TextBox();
            txtCashBoxNameAR = new TextBox();
            label6 = new Label();
            label7 = new Label();
            cmbBranch = new ComboBox();
            label4 = new Label();
            cmbAccount = new ComboBox();
            cmbCurrency = new ComboBox();
            label3 = new Label();
            pnlStatusBar.SuspendLayout();
            pnlTopBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).BeginInit();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCashBoxes).BeginInit();
            grpActions.SuspendLayout();
            grpAdditionalData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numOpeningBalance).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMinimumLimit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaximumLimit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCashBoxCurrencies).BeginInit();
            grpCashBox.SuspendLayout();
            SuspendLayout();
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.BackColor = Color.White;
            pnlStatusBar.Controls.Add(lblStatusTime);
            pnlStatusBar.Controls.Add(lblStatusLicense);
            pnlStatusBar.Controls.Add(lblStatusApi);
            pnlStatusBar.Controls.Add(lblVersion);
            pnlStatusBar.Controls.Add(lblStatusDatabase);
            pnlStatusBar.Dock = DockStyle.Bottom;
            pnlStatusBar.Location = new Point(0, 677);
            pnlStatusBar.Name = "pnlStatusBar";
            pnlStatusBar.Size = new Size(997, 35);
            pnlStatusBar.TabIndex = 26;
            // 
            // lblStatusTime
            // 
            lblStatusTime.AutoSize = true;
            lblStatusTime.Location = new Point(82, 7);
            lblStatusTime.Name = "lblStatusTime";
            lblStatusTime.Size = new Size(99, 20);
            lblStatusTime.TabIndex = 6;
            lblStatusTime.Text = "الوقت والتاريخ\r\n";
            // 
            // lblStatusLicense
            // 
            lblStatusLicense.AutoSize = true;
            lblStatusLicense.Location = new Point(285, 3);
            lblStatusLicense.Name = "lblStatusLicense";
            lblStatusLicense.Size = new Size(101, 20);
            lblStatusLicense.TabIndex = 5;
            lblStatusLicense.Text = "الترخيص: فعال";
            // 
            // lblStatusApi
            // 
            lblStatusApi.AutoSize = true;
            lblStatusApi.Location = new Point(482, 7);
            lblStatusApi.Name = "lblStatusApi";
            lblStatusApi.Size = new Size(116, 20);
            lblStatusApi.TabIndex = 4;
            lblStatusApi.Text = "API: غير مفحوص";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Location = new Point(876, 3);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(55, 20);
            lblVersion.TabIndex = 2;
            lblVersion.Text = "الإصدار\t";
            // 
            // lblStatusDatabase
            // 
            lblStatusDatabase.AutoSize = true;
            lblStatusDatabase.Location = new Point(633, 6);
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
            pnlTopBar.Size = new Size(997, 58);
            pnlTopBar.TabIndex = 25;
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
            picCompanyLogo.Location = new Point(1077, 3);
            picCompanyLogo.Name = "picCompanyLogo";
            picCompanyLogo.Size = new Size(125, 52);
            picCompanyLogo.TabIndex = 1;
            picCompanyLogo.TabStop = false;
            // 
            // lblCurrentUser
            // 
            lblCurrentUser.AutoSize = true;
            lblCurrentUser.Location = new Point(462, 8);
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new Size(85, 40);
            lblCurrentUser.TabIndex = 0;
            lblCurrentUser.Text = "المستخدم:\r\n مدير النظام";
            // 
            // lblCompanyName
            // 
            lblCompanyName.AutoSize = true;
            lblCompanyName.Location = new Point(803, 4);
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
            lblCurrentBranch.Location = new Point(669, 3);
            lblCurrentBranch.Name = "lblCurrentBranch";
            lblCurrentBranch.Size = new Size(101, 40);
            lblCurrentBranch.TabIndex = 0;
            lblCurrentBranch.Text = "الفرع \r\nعدن_المنصورة";
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(50, 251);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(58, 29);
            btnRefresh.TabIndex = 20;
            btnRefresh.Text = "تحديث";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(50, 216);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(58, 29);
            btnSearch.TabIndex = 21;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(50, 41);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(58, 29);
            btnNew.TabIndex = 24;
            btnNew.Text = "جديد";
            btnNew.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(50, 171);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(58, 29);
            btnDelete.TabIndex = 22;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(50, 88);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(58, 29);
            btnSave.TabIndex = 19;
            btnSave.Text = "حفظ";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(50, 136);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(58, 29);
            btnEdit.TabIndex = 23;
            btnEdit.Text = "تعديل";
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtSearch);
            groupBox1.Controls.Add(panel1);
            groupBox1.Controls.Add(label9);
            groupBox1.Dock = DockStyle.Bottom;
            groupBox1.Location = new Point(0, 454);
            groupBox1.Name = "groupBox1";
            groupBox1.RightToLeft = RightToLeft.Yes;
            groupBox1.Size = new Size(997, 223);
            groupBox1.TabIndex = 27;
            groupBox1.TabStop = false;
            groupBox1.Text = "جدول البيانات";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(719, 19);
            txtSearch.Name = "txtSearch";
            txtSearch.RightToLeft = RightToLeft.No;
            txtSearch.Size = new Size(195, 27);
            txtSearch.TabIndex = 53;
            // 
            // panel1
            // 
            panel1.Controls.Add(dgvCashBoxes);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(3, 54);
            panel1.Name = "panel1";
            panel1.Size = new Size(991, 166);
            panel1.TabIndex = 43;
            // 
            // dgvCashBoxes
            // 
            dgvCashBoxes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCashBoxes.Columns.AddRange(new DataGridViewColumn[] { colCashBoxCode, colCashBoxNameAR, colCashBoxNameEN, colAccount, colCurrency, colBranch, colOpeningBalance, colMaximumLimit, colMinimumLimit, colIsActive });
            dgvCashBoxes.Dock = DockStyle.Fill;
            dgvCashBoxes.Location = new Point(0, 0);
            dgvCashBoxes.Name = "dgvCashBoxes";
            dgvCashBoxes.RowHeadersWidth = 51;
            dgvCashBoxes.Size = new Size(991, 166);
            dgvCashBoxes.TabIndex = 0;
            // 
            // colCashBoxCode
            // 
            colCashBoxCode.DataPropertyName = "CashBox_Code";
            colCashBoxCode.HeaderText = "كود الصندوق";
            colCashBoxCode.MinimumWidth = 6;
            colCashBoxCode.Name = "colCashBoxCode";
            colCashBoxCode.Width = 125;
            // 
            // colCashBoxNameAR
            // 
            colCashBoxNameAR.DataPropertyName = "Box_Name_AR";
            colCashBoxNameAR.HeaderText = "اسم الصندوق عربي";
            colCashBoxNameAR.MinimumWidth = 6;
            colCashBoxNameAR.Name = "colCashBoxNameAR";
            colCashBoxNameAR.Width = 125;
            // 
            // colCashBoxNameEN
            // 
            colCashBoxNameEN.DataPropertyName = "Box_Name_EN";
            colCashBoxNameEN.HeaderText = "اسم الصندوق انجليزي";
            colCashBoxNameEN.MinimumWidth = 6;
            colCashBoxNameEN.Name = "colCashBoxNameEN";
            colCashBoxNameEN.Width = 125;
            // 
            // colAccount
            // 
            colAccount.DataPropertyName = "Account_Name_AR";
            colAccount.HeaderText = "الحساب المالي";
            colAccount.MinimumWidth = 6;
            colAccount.Name = "colAccount";
            colAccount.Width = 125;
            // 
            // colCurrency
            // 
            colCurrency.DataPropertyName = "Currency_Code";
            colCurrency.HeaderText = "العملة";
            colCurrency.MinimumWidth = 6;
            colCurrency.Name = "colCurrency";
            colCurrency.Width = 125;
            // 
            // colBranch
            // 
            colBranch.DataPropertyName = "Branch_ID";
            colBranch.HeaderText = "الفرع";
            colBranch.MinimumWidth = 6;
            colBranch.Name = "colBranch";
            colBranch.Width = 125;
            // 
            // colOpeningBalance
            // 
            colOpeningBalance.DataPropertyName = "Opening_Balance";
            colOpeningBalance.HeaderText = "الرصيد الافتتاحي";
            colOpeningBalance.MinimumWidth = 6;
            colOpeningBalance.Name = "colOpeningBalance";
            colOpeningBalance.Width = 125;
            // 
            // colMaximumLimit
            // 
            colMaximumLimit.DataPropertyName = "Max_Limit";
            colMaximumLimit.HeaderText = "الحد الاعلى";
            colMaximumLimit.MinimumWidth = 6;
            colMaximumLimit.Name = "colMaximumLimit";
            colMaximumLimit.Width = 125;
            // 
            // colMinimumLimit
            // 
            colMinimumLimit.DataPropertyName = "Min_Limit";
            colMinimumLimit.HeaderText = "الحد الادنى";
            colMinimumLimit.MinimumWidth = 6;
            colMinimumLimit.Name = "colMinimumLimit";
            colMinimumLimit.Width = 125;
            // 
            // colIsActive
            // 
            colIsActive.DataPropertyName = "Is_Active";
            colIsActive.HeaderText = "الحاله";
            colIsActive.MinimumWidth = 6;
            colIsActive.Name = "colIsActive";
            colIsActive.Width = 125;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(935, 22);
            label9.Name = "label9";
            label9.Size = new Size(37, 20);
            label9.TabIndex = 41;
            label9.Text = "بحث";
            // 
            // grpActions
            // 
            grpActions.Controls.Add(btnRefresh);
            grpActions.Controls.Add(btnNew);
            grpActions.Controls.Add(btnSave);
            grpActions.Controls.Add(btnEdit);
            grpActions.Controls.Add(btnPrint);
            grpActions.Controls.Add(btnSearch);
            grpActions.Controls.Add(btnDelete);
            grpActions.Dock = DockStyle.Right;
            grpActions.Location = new Point(845, 58);
            grpActions.Name = "grpActions";
            grpActions.RightToLeft = RightToLeft.Yes;
            grpActions.Size = new Size(152, 396);
            grpActions.TabIndex = 28;
            grpActions.TabStop = false;
            grpActions.Text = "إجراءات";
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(50, 298);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(58, 29);
            btnPrint.TabIndex = 21;
            btnPrint.Text = "طباعه";
            btnPrint.UseVisualStyleBackColor = true;
            // 
            // grpAdditionalData
            // 
            grpAdditionalData.Controls.Add(numOpeningBalance);
            grpAdditionalData.Controls.Add(numMinimumLimit);
            grpAdditionalData.Controls.Add(numMaximumLimit);
            grpAdditionalData.Controls.Add(dgvCashBoxCurrencies);
            grpAdditionalData.Controls.Add(label2);
            grpAdditionalData.Controls.Add(label13);
            grpAdditionalData.Controls.Add(label1);
            grpAdditionalData.Dock = DockStyle.Left;
            grpAdditionalData.Location = new Point(0, 58);
            grpAdditionalData.Name = "grpAdditionalData";
            grpAdditionalData.RightToLeft = RightToLeft.Yes;
            grpAdditionalData.Size = new Size(359, 396);
            grpAdditionalData.TabIndex = 30;
            grpAdditionalData.TabStop = false;
            grpAdditionalData.Text = "بيانات إضافيه";
            // 
            // numOpeningBalance
            // 
            numOpeningBalance.DecimalPlaces = 2;
            numOpeningBalance.Location = new Point(59, 27);
            numOpeningBalance.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numOpeningBalance.Name = "numOpeningBalance";
            numOpeningBalance.Size = new Size(150, 27);
            numOpeningBalance.TabIndex = 55;
            numOpeningBalance.TextAlign = HorizontalAlignment.Right;
            numOpeningBalance.ThousandsSeparator = true;
            // 
            // numMinimumLimit
            // 
            numMinimumLimit.DecimalPlaces = 2;
            numMinimumLimit.Location = new Point(59, 100);
            numMinimumLimit.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numMinimumLimit.Name = "numMinimumLimit";
            numMinimumLimit.Size = new Size(150, 27);
            numMinimumLimit.TabIndex = 54;
            numMinimumLimit.TextAlign = HorizontalAlignment.Right;
            numMinimumLimit.ThousandsSeparator = true;
            // 
            // numMaximumLimit
            // 
            numMaximumLimit.DecimalPlaces = 2;
            numMaximumLimit.Location = new Point(59, 65);
            numMaximumLimit.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numMaximumLimit.Name = "numMaximumLimit";
            numMaximumLimit.Size = new Size(150, 27);
            numMaximumLimit.TabIndex = 53;
            numMaximumLimit.TextAlign = HorizontalAlignment.Right;
            numMaximumLimit.ThousandsSeparator = true;
            // 
            // dgvCashBoxCurrencies
            // 
            dgvCashBoxCurrencies.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCashBoxCurrencies.Dock = DockStyle.Bottom;
            dgvCashBoxCurrencies.Location = new Point(3, 205);
            dgvCashBoxCurrencies.Name = "dgvCashBoxCurrencies";
            dgvCashBoxCurrencies.RowHeadersWidth = 51;
            dgvCashBoxCurrencies.Size = new Size(353, 188);
            dgvCashBoxCurrencies.TabIndex = 46;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(246, 100);
            label2.Name = "label2";
            label2.Size = new Size(78, 20);
            label2.TabIndex = 40;
            label2.Text = "الحد الأدنى";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(233, 29);
            label13.Name = "label13";
            label13.Size = new Size(112, 20);
            label13.TabIndex = 41;
            label13.Text = "الرصيد الافتتاحي";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(247, 65);
            label1.Name = "label1";
            label1.Size = new Size(79, 20);
            label1.TabIndex = 42;
            label1.Text = "الحد الأعلى";
            // 
            // grpCashBox
            // 
            grpCashBox.Controls.Add(label11);
            grpCashBox.Controls.Add(chkIsActive);
            grpCashBox.Controls.Add(label8);
            grpCashBox.Controls.Add(txtCashBoxNameEN);
            grpCashBox.Controls.Add(label5);
            grpCashBox.Controls.Add(txtNotes);
            grpCashBox.Controls.Add(txtCashBoxCode);
            grpCashBox.Controls.Add(txtCashBoxNameAR);
            grpCashBox.Controls.Add(label6);
            grpCashBox.Controls.Add(label7);
            grpCashBox.Controls.Add(cmbBranch);
            grpCashBox.Controls.Add(label4);
            grpCashBox.Controls.Add(cmbAccount);
            grpCashBox.Controls.Add(cmbCurrency);
            grpCashBox.Controls.Add(label3);
            grpCashBox.Dock = DockStyle.Fill;
            grpCashBox.Location = new Point(359, 58);
            grpCashBox.Name = "grpCashBox";
            grpCashBox.RightToLeft = RightToLeft.Yes;
            grpCashBox.Size = new Size(486, 396);
            grpCashBox.TabIndex = 31;
            grpCashBox.TabStop = false;
            grpCashBox.Text = "بيانات الصندوق";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(337, 338);
            label11.Name = "label11";
            label11.Size = new Size(67, 20);
            label11.TabIndex = 30;
            label11.Text = "ملاحظات";
            // 
            // chkIsActive
            // 
            chkIsActive.AutoSize = true;
            chkIsActive.Location = new Point(348, 299);
            chkIsActive.Name = "chkIsActive";
            chkIsActive.RightToLeft = RightToLeft.No;
            chkIsActive.Size = new Size(62, 24);
            chkIsActive.TabIndex = 42;
            chkIsActive.Text = "نشط";
            chkIsActive.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(317, 167);
            label8.Name = "label8";
            label8.Size = new Size(102, 20);
            label8.TabIndex = 52;
            label8.Text = "الحساب المالي";
            // 
            // txtCashBoxNameEN
            // 
            txtCashBoxNameEN.Location = new Point(118, 114);
            txtCashBoxNameEN.Name = "txtCashBoxNameEN";
            txtCashBoxNameEN.RightToLeft = RightToLeft.No;
            txtCashBoxNameEN.Size = new Size(195, 27);
            txtCashBoxNameEN.TabIndex = 49;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(317, 114);
            label5.Name = "label5";
            label5.Size = new Size(143, 20);
            label5.TabIndex = 46;
            label5.Text = "اسم الصندوق انجليزي";
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(122, 323);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.RightToLeft = RightToLeft.No;
            txtNotes.Size = new Size(187, 63);
            txtNotes.TabIndex = 36;
            // 
            // txtCashBoxCode
            // 
            txtCashBoxCode.Location = new Point(118, 40);
            txtCashBoxCode.Name = "txtCashBoxCode";
            txtCashBoxCode.ReadOnly = true;
            txtCashBoxCode.RightToLeft = RightToLeft.No;
            txtCashBoxCode.Size = new Size(195, 27);
            txtCashBoxCode.TabIndex = 50;
            // 
            // txtCashBoxNameAR
            // 
            txtCashBoxNameAR.Location = new Point(118, 76);
            txtCashBoxNameAR.Name = "txtCashBoxNameAR";
            txtCashBoxNameAR.RightToLeft = RightToLeft.No;
            txtCashBoxNameAR.Size = new Size(195, 27);
            txtCashBoxNameAR.TabIndex = 51;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(337, 43);
            label6.Name = "label6";
            label6.Size = new Size(93, 20);
            label6.TabIndex = 47;
            label6.Text = "كود الصندوق";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(331, 79);
            label7.Name = "label7";
            label7.Size = new Size(132, 20);
            label7.TabIndex = 48;
            label7.Text = "اسم الصندوق عربي";
            // 
            // cmbBranch
            // 
            cmbBranch.FormattingEnabled = true;
            cmbBranch.Location = new Point(118, 264);
            cmbBranch.Name = "cmbBranch";
            cmbBranch.RightToLeft = RightToLeft.No;
            cmbBranch.Size = new Size(195, 28);
            cmbBranch.TabIndex = 40;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(369, 266);
            label4.Name = "label4";
            label4.Size = new Size(41, 20);
            label4.TabIndex = 31;
            label4.Text = "الفرع";
            // 
            // cmbAccount
            // 
            cmbAccount.FormattingEnabled = true;
            cmbAccount.Location = new Point(116, 167);
            cmbAccount.Name = "cmbAccount";
            cmbAccount.RightToLeft = RightToLeft.No;
            cmbAccount.Size = new Size(195, 28);
            cmbAccount.TabIndex = 41;
            // 
            // cmbCurrency
            // 
            cmbCurrency.FormattingEnabled = true;
            cmbCurrency.Location = new Point(118, 218);
            cmbCurrency.Name = "cmbCurrency";
            cmbCurrency.RightToLeft = RightToLeft.No;
            cmbCurrency.Size = new Size(195, 28);
            cmbCurrency.TabIndex = 41;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(364, 218);
            label3.Name = "label3";
            label3.Size = new Size(49, 20);
            label3.TabIndex = 32;
            label3.Text = "العمله";
            // 
            // FrmCashBoxes
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(997, 712);
            Controls.Add(grpCashBox);
            Controls.Add(grpAdditionalData);
            Controls.Add(grpActions);
            Controls.Add(groupBox1);
            Controls.Add(pnlStatusBar);
            Controls.Add(pnlTopBar);
            Name = "FrmCashBoxes";
            Text = "الصناديق";
            Load += FrmCashBoxes_Load;
            pnlStatusBar.ResumeLayout(false);
            pnlStatusBar.PerformLayout();
            pnlTopBar.ResumeLayout(false);
            pnlTopBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCashBoxes).EndInit();
            grpActions.ResumeLayout(false);
            grpAdditionalData.ResumeLayout(false);
            grpAdditionalData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numOpeningBalance).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMinimumLimit).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaximumLimit).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCashBoxCurrencies).EndInit();
            grpCashBox.ResumeLayout(false);
            grpCashBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlStatusBar;
        private Label lblStatusTime;
        private Label lblStatusLicense;
        private Label lblStatusApi;
        private Label lblVersion;
        private Label lblStatusDatabase;
        private Panel pnlTopBar;
        private Button btnRefresh;
        private Button btnClose;
        private PictureBox picCompanyLogo;
        private Button btnSearch;
        private Label lblCurrentUser;
        private Button btnNew;
        private Label lblCompanyName;
        private Button btnDelete;
        private Label lblFiscalYear;
        private Button btnSave;
        private Label lblCurrentBranch;
        private Button btnEdit;
        private GroupBox groupBox1;
        private GroupBox grpActions;
        private Button btnPrint;
        private TextBox txtOpeningBalance;
        private TextBox txtMaximumLimit;
        private Label label9;
        private Panel panel1;
        private DataGridView dgvCashBoxes;
        private TextBox txtSearch;
        private GroupBox grpAdditionalData;
        private NumericUpDown numOpeningBalance;
        private NumericUpDown numMinimumLimit;
        private NumericUpDown numMaximumLimit;
        private DataGridView dgvCashBoxCurrencies;
        private Label label2;
        private Label label13;
        private Label label1;
        private GroupBox grpCashBox;
        private Label label11;
        private CheckBox chkIsActive;
        private Label label8;
        private TextBox txtCashBoxNameEN;
        private Label label5;
        private TextBox txtNotes;
        private TextBox txtCashBoxCode;
        private TextBox txtCashBoxNameAR;
        private Label label6;
        private Label label7;
        private ComboBox cmbBranch;
        private Label label4;
        private ComboBox cmbAccount;
        private ComboBox cmbCurrency;
        private Label label3;
        private DataGridViewTextBoxColumn colCashBoxCode;
        private DataGridViewTextBoxColumn colCashBoxNameAR;
        private DataGridViewTextBoxColumn colCashBoxNameEN;
        private DataGridViewTextBoxColumn colAccount;
        private DataGridViewTextBoxColumn colCurrency;
        private DataGridViewTextBoxColumn colBranch;
        private DataGridViewTextBoxColumn colOpeningBalance;
        private DataGridViewTextBoxColumn colMaximumLimit;
        private DataGridViewTextBoxColumn colMinimumLimit;
        private DataGridViewTextBoxColumn colIsActive;
    }
}