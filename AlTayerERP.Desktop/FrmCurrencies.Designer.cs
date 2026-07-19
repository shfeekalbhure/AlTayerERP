namespace AlTayerERP.Desktop
{
    partial class FrmCurrencies
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
            pnlTopBar = new Panel();
            btnRefresh = new Button();
            btnClose = new Button();
            picCompanyLogo = new PictureBox();
            btnSearch = new Button();
            lblCurrentUser = new Label();
            btnNew = new Button();
            lblCompanyName = new Label();
            btnDelete = new Button();
            lblFiscalYear = new Label();
            btnSave = new Button();
            lblCurrentBranch = new Label();
            btnEdit = new Button();
            pnlStatusBar = new Panel();
            lblStatusTime = new Label();
            lblStatusLicense = new Label();
            lblStatusApi = new Label();
            lblVersion = new Label();
            lblStatusDatabase = new Label();
            groupBox1 = new GroupBox();
            numMinExchangeRate = new NumericUpDown();
            label6 = new Label();
            numMaxExchangeRate = new NumericUpDown();
            label5 = new Label();
            chkIsLocalCurrency = new CheckBox();
            chkIsActive = new CheckBox();
            chkIsDefault = new CheckBox();
            numExchangeRate = new NumericUpDown();
            numDecimalPlaces = new NumericUpDown();
            label11 = new Label();
            txtCurrencyCode = new TextBox();
            label10 = new Label();
            txtNotes = new TextBox();
            label7 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            txtCurrencyNameEN = new TextBox();
            txtCurrencySymbol = new TextBox();
            txtCurrencyNameAR = new TextBox();
            dgvCurrencies = new DataGridView();
            Currency_ID = new DataGridViewTextBoxColumn();
            Currency_Code = new DataGridViewTextBoxColumn();
            Currency_Name_AR = new DataGridViewTextBoxColumn();
            Currency_Name_EN = new DataGridViewTextBoxColumn();
            Currency_Symbol = new DataGridViewTextBoxColumn();
            Decimal_Places = new DataGridViewTextBoxColumn();
            colMaxExchangeRate = new DataGridViewTextBoxColumn();
            Exchange_Rate = new DataGridViewTextBoxColumn();
            colMinExchangeRate = new DataGridViewTextBoxColumn();
            colIsLocalCurrency = new DataGridViewCheckBoxColumn();
            Is_Default = new DataGridViewCheckBoxColumn();
            Is_Active = new DataGridViewCheckBoxColumn();
            pnlTopBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).BeginInit();
            pnlStatusBar.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMinExchangeRate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxExchangeRate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numExchangeRate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDecimalPlaces).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCurrencies).BeginInit();
            SuspendLayout();
            // 
            // pnlTopBar
            // 
            pnlTopBar.BackColor = Color.White;
            pnlTopBar.Controls.Add(btnRefresh);
            pnlTopBar.Controls.Add(btnClose);
            pnlTopBar.Controls.Add(picCompanyLogo);
            pnlTopBar.Controls.Add(btnSearch);
            pnlTopBar.Controls.Add(lblCurrentUser);
            pnlTopBar.Controls.Add(btnNew);
            pnlTopBar.Controls.Add(lblCompanyName);
            pnlTopBar.Controls.Add(btnDelete);
            pnlTopBar.Controls.Add(lblFiscalYear);
            pnlTopBar.Controls.Add(btnSave);
            pnlTopBar.Controls.Add(lblCurrentBranch);
            pnlTopBar.Controls.Add(btnEdit);
            pnlTopBar.Dock = DockStyle.Top;
            pnlTopBar.Location = new Point(0, 0);
            pnlTopBar.Name = "pnlTopBar";
            pnlTopBar.Size = new Size(1164, 58);
            pnlTopBar.TabIndex = 21;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(69, 9);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(58, 29);
            btnRefresh.TabIndex = 20;
            btnRefresh.Text = "تحديث";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(3, 8);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(62, 30);
            btnClose.TabIndex = 4;
            btnClose.Text = "اغلاق";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // picCompanyLogo
            // 
            picCompanyLogo.Location = new Point(956, 3);
            picCompanyLogo.Name = "picCompanyLogo";
            picCompanyLogo.Size = new Size(125, 52);
            picCompanyLogo.TabIndex = 1;
            picCompanyLogo.TabStop = false;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(133, 9);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(58, 29);
            btnSearch.TabIndex = 21;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = true;
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
            // btnNew
            // 
            btnNew.Location = new Point(389, 9);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(58, 29);
            btnNew.TabIndex = 24;
            btnNew.Text = "جديد";
            btnNew.UseVisualStyleBackColor = true;
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
            // btnDelete
            // 
            btnDelete.Location = new Point(197, 9);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(58, 29);
            btnDelete.TabIndex = 22;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = true;
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
            // btnSave
            // 
            btnSave.Location = new Point(325, 9);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(58, 29);
            btnSave.TabIndex = 19;
            btnSave.Text = "حفظ";
            btnSave.UseVisualStyleBackColor = true;
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
            // btnEdit
            // 
            btnEdit.Location = new Point(261, 9);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(58, 29);
            btnEdit.TabIndex = 23;
            btnEdit.Text = "تعديل";
            btnEdit.UseVisualStyleBackColor = true;
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
            pnlStatusBar.Location = new Point(0, 593);
            pnlStatusBar.Name = "pnlStatusBar";
            pnlStatusBar.Size = new Size(1164, 35);
            pnlStatusBar.TabIndex = 22;
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
            // groupBox1
            // 
            groupBox1.Controls.Add(numMinExchangeRate);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(numMaxExchangeRate);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(chkIsLocalCurrency);
            groupBox1.Controls.Add(chkIsActive);
            groupBox1.Controls.Add(chkIsDefault);
            groupBox1.Controls.Add(numExchangeRate);
            groupBox1.Controls.Add(numDecimalPlaces);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(txtCurrencyCode);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(txtNotes);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(txtCurrencyNameEN);
            groupBox1.Controls.Add(txtCurrencySymbol);
            groupBox1.Controls.Add(txtCurrencyNameAR);
            groupBox1.Dock = DockStyle.Right;
            groupBox1.Location = new Point(727, 58);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(437, 535);
            groupBox1.TabIndex = 23;
            groupBox1.TabStop = false;
            groupBox1.Text = "بينات العملة";
          //  groupBox1.Enter += groupBox1_Enter;
            // 
            // numMinExchangeRate
            // 
            numMinExchangeRate.DecimalPlaces = 6;
            numMinExchangeRate.Location = new Point(92, 287);
            numMinExchangeRate.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numMinExchangeRate.Name = "numMinExchangeRate";
            numMinExchangeRate.Size = new Size(150, 27);
            numMinExchangeRate.TabIndex = 96;
            numMinExchangeRate.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(261, 290);
            label6.Name = "label6";
            label6.Size = new Size(110, 20);
            label6.TabIndex = 95;
            label6.Text = "اقل سعر الصرف";
            // 
            // numMaxExchangeRate
            // 
            numMaxExchangeRate.DecimalPlaces = 6;
            numMaxExchangeRate.Location = new Point(92, 238);
            numMaxExchangeRate.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numMaxExchangeRate.Name = "numMaxExchangeRate";
            numMaxExchangeRate.Size = new Size(150, 27);
            numMaxExchangeRate.TabIndex = 94;
            numMaxExchangeRate.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(261, 241);
            label5.Name = "label5";
            label5.Size = new Size(114, 20);
            label5.TabIndex = 93;
            label5.Text = " اعلا سعر الصرف";
            // 
            // chkIsLocalCurrency
            // 
            chkIsLocalCurrency.AutoSize = true;
            chkIsLocalCurrency.Checked = true;
            chkIsLocalCurrency.CheckState = CheckState.Checked;
            chkIsLocalCurrency.Location = new Point(239, 394);
            chkIsLocalCurrency.Name = "chkIsLocalCurrency";
            chkIsLocalCurrency.Size = new Size(120, 24);
            chkIsLocalCurrency.TabIndex = 92;
            chkIsLocalCurrency.Text = "العملة المحلية";
            chkIsLocalCurrency.UseVisualStyleBackColor = true;
            // 
            // chkIsActive
            // 
            chkIsActive.AutoSize = true;
            chkIsActive.Checked = true;
            chkIsActive.CheckState = CheckState.Checked;
            chkIsActive.Location = new Point(239, 361);
            chkIsActive.Name = "chkIsActive";
            chkIsActive.Size = new Size(62, 24);
            chkIsActive.TabIndex = 91;
            chkIsActive.Text = "نشط";
            chkIsActive.UseVisualStyleBackColor = true;
            // 
            // chkIsDefault
            // 
            chkIsDefault.AutoSize = true;
            chkIsDefault.Checked = true;
            chkIsDefault.CheckState = CheckState.Checked;
            chkIsDefault.Location = new Point(239, 331);
            chkIsDefault.Name = "chkIsDefault";
            chkIsDefault.Size = new Size(138, 24);
            chkIsDefault.TabIndex = 91;
            chkIsDefault.Text = "العملة الافتراضية";
            chkIsDefault.UseVisualStyleBackColor = true;
            // 
            // numExchangeRate
            // 
            numExchangeRate.DecimalPlaces = 6;
            numExchangeRate.Location = new Point(102, 198);
            numExchangeRate.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numExchangeRate.Name = "numExchangeRate";
            numExchangeRate.Size = new Size(150, 27);
            numExchangeRate.TabIndex = 90;
            numExchangeRate.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // numDecimalPlaces
            // 
            numDecimalPlaces.Location = new Point(102, 159);
            numDecimalPlaces.Name = "numDecimalPlaces";
            numDecimalPlaces.Size = new Size(150, 27);
            numDecimalPlaces.TabIndex = 90;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(273, 31);
            label11.Name = "label11";
            label11.Size = new Size(77, 20);
            label11.TabIndex = 89;
            label11.Text = "كود العملة";
            // 
            // txtCurrencyCode
            // 
            txtCurrencyCode.Location = new Point(49, 27);
            txtCurrencyCode.Name = "txtCurrencyCode";
            txtCurrencyCode.Size = new Size(203, 27);
            txtCurrencyCode.TabIndex = 88;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(273, 438);
            label10.Name = "label10";
            label10.Size = new Size(67, 20);
            label10.TabIndex = 87;
            label10.Text = "ملاحظات";
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(49, 434);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.Size = new Size(203, 73);
            txtNotes.TabIndex = 86;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(271, 201);
            label7.Name = "label7";
            label7.Size = new Size(84, 20);
            label7.TabIndex = 83;
            label7.Text = "سعر الصرف";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(273, 166);
            label4.Name = "label4";
            label4.Size = new Size(85, 20);
            label4.TabIndex = 79;
            label4.Text = "عداد المنازل";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(273, 130);
            label3.Name = "label3";
            label3.Size = new Size(38, 20);
            label3.TabIndex = 78;
            label3.Text = "الرمز";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(273, 100);
            label2.Name = "label2";
            label2.Size = new Size(127, 20);
            label2.TabIndex = 77;
            label2.Text = "اسم العملة انجليزي";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(273, 64);
            label1.Name = "label1";
            label1.Size = new Size(116, 20);
            label1.TabIndex = 76;
            label1.Text = "اسم العملة عربي";
            // 
            // txtCurrencyNameEN
            // 
            txtCurrencyNameEN.Location = new Point(49, 93);
            txtCurrencyNameEN.Name = "txtCurrencyNameEN";
            txtCurrencyNameEN.Size = new Size(203, 27);
            txtCurrencyNameEN.TabIndex = 73;
            // 
            // txtCurrencySymbol
            // 
            txtCurrencySymbol.Location = new Point(49, 123);
            txtCurrencySymbol.Name = "txtCurrencySymbol";
            txtCurrencySymbol.Size = new Size(203, 27);
            txtCurrencySymbol.TabIndex = 72;
            // 
            // txtCurrencyNameAR
            // 
            txtCurrencyNameAR.Location = new Point(49, 60);
            txtCurrencyNameAR.Name = "txtCurrencyNameAR";
            txtCurrencyNameAR.Size = new Size(203, 27);
            txtCurrencyNameAR.TabIndex = 72;
            // 
            // dgvCurrencies
            // 
            dgvCurrencies.AllowUserToAddRows = false;
            dgvCurrencies.AllowUserToDeleteRows = false;
            dgvCurrencies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCurrencies.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCurrencies.Columns.AddRange(new DataGridViewColumn[] { Currency_ID, Currency_Code, Currency_Name_AR, Currency_Name_EN, Currency_Symbol, Decimal_Places, colMaxExchangeRate, Exchange_Rate, colMinExchangeRate, colIsLocalCurrency, Is_Default, Is_Active });
            dgvCurrencies.Dock = DockStyle.Fill;
            dgvCurrencies.Location = new Point(0, 58);
            dgvCurrencies.MultiSelect = false;
            dgvCurrencies.Name = "dgvCurrencies";
            dgvCurrencies.ReadOnly = true;
            dgvCurrencies.RightToLeft = RightToLeft.Yes;
            dgvCurrencies.RowHeadersWidth = 51;
            dgvCurrencies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCurrencies.Size = new Size(727, 535);
            dgvCurrencies.TabIndex = 24;
            // 
            // Currency_ID
            // 
            Currency_ID.HeaderText = "رقم العملة";
            Currency_ID.MinimumWidth = 6;
            Currency_ID.Name = "Currency_ID";
            Currency_ID.ReadOnly = true;
            // 
            // Currency_Code
            // 
            Currency_Code.HeaderText = "كود العملة";
            Currency_Code.MinimumWidth = 6;
            Currency_Code.Name = "Currency_Code";
            Currency_Code.ReadOnly = true;
            // 
            // Currency_Name_AR
            // 
            Currency_Name_AR.HeaderText = "اسم العملة عربي";
            Currency_Name_AR.MinimumWidth = 6;
            Currency_Name_AR.Name = "Currency_Name_AR";
            Currency_Name_AR.ReadOnly = true;
            // 
            // Currency_Name_EN
            // 
            Currency_Name_EN.HeaderText = "العملة انجليزي";
            Currency_Name_EN.MinimumWidth = 6;
            Currency_Name_EN.Name = "Currency_Name_EN";
            Currency_Name_EN.ReadOnly = true;
            // 
            // Currency_Symbol
            // 
            Currency_Symbol.HeaderText = "الرمز";
            Currency_Symbol.MinimumWidth = 6;
            Currency_Symbol.Name = "Currency_Symbol";
            Currency_Symbol.ReadOnly = true;
            // 
            // Decimal_Places
            // 
            Decimal_Places.HeaderText = "عدد المنازل";
            Decimal_Places.MinimumWidth = 6;
            Decimal_Places.Name = "Decimal_Places";
            Decimal_Places.ReadOnly = true;
            // 
            // colMaxExchangeRate
            // 
            colMaxExchangeRate.HeaderText = "أعلى سعر الصرف";
            colMaxExchangeRate.MinimumWidth = 6;
            colMaxExchangeRate.Name = "colMaxExchangeRate";
            colMaxExchangeRate.ReadOnly = true;
            // 
            // Exchange_Rate
            // 
            Exchange_Rate.HeaderText = "سعر الصرف";
            Exchange_Rate.MinimumWidth = 6;
            Exchange_Rate.Name = "Exchange_Rate";
            Exchange_Rate.ReadOnly = true;
            // 
            // colMinExchangeRate
            // 
            colMinExchangeRate.HeaderText = "أقل سعر الصرف";
            colMinExchangeRate.MinimumWidth = 6;
            colMinExchangeRate.Name = "colMinExchangeRate";
            colMinExchangeRate.ReadOnly = true;
            // 
            // colIsLocalCurrency
            // 
            colIsLocalCurrency.HeaderText = "العملة المحلية";
            colIsLocalCurrency.MinimumWidth = 6;
            colIsLocalCurrency.Name = "colIsLocalCurrency";
            colIsLocalCurrency.ReadOnly = true;
            colIsLocalCurrency.Resizable = DataGridViewTriState.True;
            colIsLocalCurrency.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // Is_Default
            // 
            Is_Default.HeaderText = "العملة الافتراضية";
            Is_Default.MinimumWidth = 6;
            Is_Default.Name = "Is_Default";
            Is_Default.ReadOnly = true;
            Is_Default.Resizable = DataGridViewTriState.True;
            Is_Default.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // Is_Active
            // 
            Is_Active.HeaderText = "الحالة";
            Is_Active.MinimumWidth = 6;
            Is_Active.Name = "Is_Active";
            Is_Active.ReadOnly = true;
            Is_Active.Resizable = DataGridViewTriState.True;
            Is_Active.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // FrmCurrencies
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1164, 628);
            Controls.Add(dgvCurrencies);
            Controls.Add(groupBox1);
            Controls.Add(pnlStatusBar);
            Controls.Add(pnlTopBar);
            Name = "FrmCurrencies";
            Text = "شاشة العملات";
            pnlTopBar.ResumeLayout(false);
            pnlTopBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).EndInit();
            pnlStatusBar.ResumeLayout(false);
            pnlStatusBar.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMinExchangeRate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxExchangeRate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numExchangeRate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDecimalPlaces).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCurrencies).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTopBar;
        private Button btnClose;
        private PictureBox picCompanyLogo;
        private Label lblCurrentUser;
        private Label lblCompanyName;
        private Label lblFiscalYear;
        private Label lblCurrentBranch;
        private Button btnRefresh;
        private Button btnSearch;
        private Button btnDelete;
        private Button btnEdit;
        private Button btnSave;
        private Button btnNew;
        private Panel pnlStatusBar;
        private Label lblStatusTime;
        private Label lblStatusLicense;
        private Label lblStatusApi;
        private Label lblVersion;
        private Label lblStatusDatabase;
        private GroupBox groupBox1;
        private Label label11;
        private TextBox txtCurrencyCode;
        private Label label10;
        private TextBox txtNotes;
        private Label label7;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private TextBox txtCurrencyNameEN;
        private TextBox txtCurrencyNameAR;
        private TextBox txtCurrencySymbol;
        private NumericUpDown numExchangeRate;
        private NumericUpDown numDecimalPlaces;
        private DataGridView dgvCurrencies;
        private CheckBox chkIsActive;
        private CheckBox chkIsDefault;
        private NumericUpDown numericUpDown2;
        private Label label6;
        private NumericUpDown numericUpDown1;
        private Label label5;
        private CheckBox checkBox1;
        private NumericUpDown Min_Exchange_Rate;
        private NumericUpDown Max_Exchange_Rate;
        private CheckBox chkIsLocalCurrency;
        private DataGridViewTextBoxColumn Currency_ID;
        private DataGridViewTextBoxColumn Currency_Code;
        private DataGridViewTextBoxColumn Currency_Name_AR;
        private DataGridViewTextBoxColumn Currency_Name_EN;
        private DataGridViewTextBoxColumn Currency_Symbol;
        private DataGridViewTextBoxColumn Decimal_Places;
        private DataGridViewTextBoxColumn colMaxExchangeRate;
        private DataGridViewTextBoxColumn Exchange_Rate;
        private DataGridViewTextBoxColumn colMinExchangeRate;
        private DataGridViewCheckBoxColumn colIsLocalCurrency;
        private DataGridViewCheckBoxColumn Is_Default;
        private DataGridViewCheckBoxColumn Is_Active;
        private NumericUpDown numMinExchangeRate;
        private NumericUpDown numMaxExchangeRate;
    }
}