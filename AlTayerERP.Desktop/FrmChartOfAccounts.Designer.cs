namespace AlTayerERP.Desktop
{
    partial class FrmChartOfAccounts
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
            components = new System.ComponentModel.Container();
            pnlToolbar = new Panel();
            button10 = new Button();
            btnClose = new Button();
            button9 = new Button();
            btnDelete = new Button();
            btnPrint = new Button();
            btnEdit = new Button();
            btnSearch = new Button();
            btnRefresh = new Button();
            btnSave = new Button();
            btnNew = new Button();
            pnlTreeContainer = new Panel();
            gbTreeContainer = new GroupBox();
            tvAccounts = new TreeView();
            imgAccountsTree = new ImageList(components);
            txtSearchTree = new TextBox();
            pnlMainDetails = new Panel();
            grpSystemInfo = new GroupBox();
            txtUpdatedAt = new TextBox();
            label17 = new Label();
            txtSystemStatus = new TextBox();
            label19 = new Label();
            txtCreatedBy = new TextBox();
            label16 = new Label();
            txtCreatedAt = new TextBox();
            label15 = new Label();
            gbBasicInfo = new GroupBox();
            label11 = new Label();
            label6 = new Label();
            cmbDefaultProject = new ComboBox();
            cmbDefaultCostCenter = new ComboBox();
            cmbAccountCategory = new ComboBox();
            label12 = new Label();
            label10 = new Label();
            label5 = new Label();
            cmbCurrency = new ComboBox();
            cmbAccountType = new ComboBox();
            label9 = new Label();
            label4 = new Label();
            cmbAccountStatus = new ComboBox();
            label14 = new Label();
            cmbNormalBalance = new ComboBox();
            label8 = new Label();
            cmbParentAccount = new ComboBox();
            txtNotes = new TextBox();
            label3 = new Label();
            txtAccountShortCode = new TextBox();
            txtAccountLevel = new TextBox();
            label7 = new Label();
            txtAccountNameEN = new TextBox();
            label2 = new Label();
            txtAccountCode = new TextBox();
            txtAccountNameAR = new TextBox();
            label13 = new Label();
            label1 = new Label();
            contextMenuStrip1 = new ContextMenuStrip(components);
            statusMain = new StatusStrip();
            lblAccountsCount = new ToolStripStatusLabel();
            lblSep1 = new ToolStripStatusLabel();
            lblCurrentAccount = new ToolStripStatusLabel();
            lblSep2 = new ToolStripStatusLabel();
            lblCurrentCompany = new ToolStripStatusLabel();
            lblSep3 = new ToolStripStatusLabel();
            lblCurrentUser = new ToolStripStatusLabel();
            chkAffectsIncomeStatement = new CheckBox();
            chkRequiresParty = new CheckBox();
            chkAffectsBalanceSheet = new CheckBox();
            chkRequiresProject = new CheckBox();
            chkIsSummaryAccount = new CheckBox();
            chkRequiresCostCenter = new CheckBox();
            chkMultiCurrency = new CheckBox();
            chkAllowManualEntry = new CheckBox();
            chkSystemAccount = new CheckBox();
            chkIsPostable = new CheckBox();
            gbSubAccounts = new GroupBox();
            dgvSubAccounts = new DataGridView();
            colAccountCode = new DataGridViewTextBoxColumn();
            colAccountNameAr = new DataGridViewTextBoxColumn();
            colAccountNameEn = new DataGridViewTextBoxColumn();
            colAccountType = new DataGridViewTextBoxColumn();
            colAccountNature = new DataGridViewTextBoxColumn();
            colCurrency = new DataGridViewTextBoxColumn();
            colAccountLevel = new DataGridViewTextBoxColumn();
            colAccountStatus = new DataGridViewTextBoxColumn();
            pnlToolbar.SuspendLayout();
            pnlTreeContainer.SuspendLayout();
            gbTreeContainer.SuspendLayout();
            pnlMainDetails.SuspendLayout();
            grpSystemInfo.SuspendLayout();
            gbBasicInfo.SuspendLayout();
            statusMain.SuspendLayout();
            gbSubAccounts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSubAccounts).BeginInit();
            SuspendLayout();
            // 
            // pnlToolbar
            // 
            pnlToolbar.BorderStyle = BorderStyle.FixedSingle;
            pnlToolbar.Controls.Add(button10);
            pnlToolbar.Controls.Add(btnClose);
            pnlToolbar.Controls.Add(button9);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Controls.Add(btnPrint);
            pnlToolbar.Controls.Add(btnEdit);
            pnlToolbar.Controls.Add(btnSearch);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(btnSave);
            pnlToolbar.Controls.Add(btnNew);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(1582, 85);
            pnlToolbar.TabIndex = 8;
            pnlToolbar.Paint += pnlToolbar_Paint;
            // 
            // button10
            // 
            button10.FlatStyle = FlatStyle.Flat;
            button10.Location = new Point(22, 3);
            button10.Name = "button10";
            button10.Size = new Size(75, 75);
            button10.TabIndex = 3;
            button10.Text = "استيراد";
            button10.TextImageRelation = TextImageRelation.ImageBeforeText;
            button10.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Location = new Point(611, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 75);
            btnClose.TabIndex = 3;
            btnClose.Text = "اغلاق";
            btnClose.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnClose.UseVisualStyleBackColor = true;
            // 
            // button9
            // 
            button9.FlatStyle = FlatStyle.Flat;
            button9.Location = new Point(122, 3);
            button9.Name = "button9";
            button9.Size = new Size(75, 75);
            button9.TabIndex = 3;
            button9.Text = "تصدير";
            button9.TextImageRelation = TextImageRelation.ImageBeforeText;
            button9.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Location = new Point(1044, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 75);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "حذف";
            btnDelete.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnPrint
            // 
            btnPrint.FlatStyle = FlatStyle.Flat;
            btnPrint.Location = new Point(711, 3);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(75, 75);
            btnPrint.TabIndex = 3;
            btnPrint.Text = "طباعة";
            btnPrint.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnPrint.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Location = new Point(1144, 3);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(75, 75);
            btnEdit.TabIndex = 3;
            btnEdit.Text = "تعديل";
            btnEdit.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnSearch
            // 
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Location = new Point(825, 3);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 75);
            btnSearch.TabIndex = 3;
            btnSearch.Text = "بحث";
            btnSearch.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Location = new Point(935, 3);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 75);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "تحديث";
            btnRefresh.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Location = new Point(1258, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 75);
            btnSave.TabIndex = 3;
            btnSave.Text = "حفط";
            btnSave.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            btnNew.FlatStyle = FlatStyle.Flat;
            btnNew.Location = new Point(1368, 3);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(75, 75);
            btnNew.TabIndex = 3;
            btnNew.Text = "جديد";
            btnNew.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnNew.UseVisualStyleBackColor = true;
            btnNew.Click += btnNew_Click;
            // 
            // pnlTreeContainer
            // 
            pnlTreeContainer.BorderStyle = BorderStyle.FixedSingle;
            pnlTreeContainer.Controls.Add(gbTreeContainer);
            pnlTreeContainer.Dock = DockStyle.Right;
            pnlTreeContainer.Location = new Point(1132, 85);
            pnlTreeContainer.Name = "pnlTreeContainer";
            pnlTreeContainer.Size = new Size(450, 768);
            pnlTreeContainer.TabIndex = 9;
            // 
            // gbTreeContainer
            // 
            gbTreeContainer.Controls.Add(tvAccounts);
            gbTreeContainer.Controls.Add(txtSearchTree);
            gbTreeContainer.Dock = DockStyle.Fill;
            gbTreeContainer.Location = new Point(0, 0);
            gbTreeContainer.Name = "gbTreeContainer";
            gbTreeContainer.Size = new Size(448, 766);
            gbTreeContainer.TabIndex = 0;
            gbTreeContainer.TabStop = false;
            gbTreeContainer.Text = "شجرة الحسابات";
            // 
            // tvAccounts
            // 
            tvAccounts.Dock = DockStyle.Fill;
            tvAccounts.FullRowSelect = true;
            tvAccounts.ImageIndex = 0;
            tvAccounts.ImageList = imgAccountsTree;
            tvAccounts.Location = new Point(3, 56);
            tvAccounts.Name = "tvAccounts";
            tvAccounts.SelectedImageIndex = 0;
            tvAccounts.Size = new Size(442, 707);
            tvAccounts.TabIndex = 2;
            // 
            // imgAccountsTree
            // 
            imgAccountsTree.ColorDepth = ColorDepth.Depth32Bit;
            imgAccountsTree.ImageSize = new Size(16, 16);
            imgAccountsTree.TransparentColor = Color.Transparent;
            // 
            // txtSearchTree
            // 
            txtSearchTree.Dock = DockStyle.Top;
            txtSearchTree.Location = new Point(3, 26);
            txtSearchTree.Name = "txtSearchTree";
            txtSearchTree.PlaceholderText = "بحث في الشجرة";
            txtSearchTree.Size = new Size(442, 30);
            txtSearchTree.TabIndex = 1;
            txtSearchTree.TextChanged += txtSearchTree_TextChanged;
            // 
            // pnlMainDetails
            // 
            pnlMainDetails.Controls.Add(gbSubAccounts);
            pnlMainDetails.Controls.Add(grpSystemInfo);
            pnlMainDetails.Controls.Add(gbBasicInfo);
            pnlMainDetails.Dock = DockStyle.Fill;
            pnlMainDetails.Location = new Point(0, 85);
            pnlMainDetails.Name = "pnlMainDetails";
            pnlMainDetails.Size = new Size(1132, 768);
            pnlMainDetails.TabIndex = 10;
            pnlMainDetails.Paint += pnlMainDetails_Paint;
            // 
            // grpSystemInfo
            // 
            grpSystemInfo.Controls.Add(chkAffectsIncomeStatement);
            grpSystemInfo.Controls.Add(chkRequiresParty);
            grpSystemInfo.Controls.Add(chkAffectsBalanceSheet);
            grpSystemInfo.Controls.Add(chkRequiresProject);
            grpSystemInfo.Controls.Add(chkIsSummaryAccount);
            grpSystemInfo.Controls.Add(chkRequiresCostCenter);
            grpSystemInfo.Controls.Add(chkMultiCurrency);
            grpSystemInfo.Controls.Add(chkAllowManualEntry);
            grpSystemInfo.Controls.Add(chkSystemAccount);
            grpSystemInfo.Controls.Add(chkIsPostable);
            grpSystemInfo.Controls.Add(txtUpdatedAt);
            grpSystemInfo.Controls.Add(label17);
            grpSystemInfo.Controls.Add(txtSystemStatus);
            grpSystemInfo.Controls.Add(label19);
            grpSystemInfo.Controls.Add(txtCreatedBy);
            grpSystemInfo.Controls.Add(label16);
            grpSystemInfo.Controls.Add(txtCreatedAt);
            grpSystemInfo.Controls.Add(label15);
            grpSystemInfo.Dock = DockStyle.Top;
            grpSystemInfo.Location = new Point(0, 307);
            grpSystemInfo.Name = "grpSystemInfo";
            grpSystemInfo.Size = new Size(1132, 176);
            grpSystemInfo.TabIndex = 4;
            grpSystemInfo.TabStop = false;
            grpSystemInfo.Text = "معلومات النظام";
            grpSystemInfo.Enter += grpSystemInfo_Enter;
            // 
            // txtUpdatedAt
            // 
            txtUpdatedAt.BackColor = Color.WhiteSmoke;
            txtUpdatedAt.Location = new Point(325, 30);
            txtUpdatedAt.Name = "txtUpdatedAt";
            txtUpdatedAt.ReadOnly = true;
            txtUpdatedAt.RightToLeft = RightToLeft.No;
            txtUpdatedAt.Size = new Size(217, 30);
            txtUpdatedAt.TabIndex = 1;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(544, 34);
            label17.Name = "label17";
            label17.Size = new Size(78, 23);
            label17.TabIndex = 0;
            label17.Text = "اخر تعديل";
            // 
            // txtSystemStatus
            // 
            txtSystemStatus.BackColor = Color.WhiteSmoke;
            txtSystemStatus.Location = new Point(325, 66);
            txtSystemStatus.Name = "txtSystemStatus";
            txtSystemStatus.ReadOnly = true;
            txtSystemStatus.RightToLeft = RightToLeft.No;
            txtSystemStatus.Size = new Size(218, 30);
            txtSystemStatus.TabIndex = 1;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(545, 70);
            label19.Name = "label19";
            label19.Size = new Size(49, 23);
            label19.TabIndex = 0;
            label19.Text = "الحالة";
            // 
            // txtCreatedBy
            // 
            txtCreatedBy.BackColor = Color.WhiteSmoke;
            txtCreatedBy.Location = new Point(666, 67);
            txtCreatedBy.Name = "txtCreatedBy";
            txtCreatedBy.ReadOnly = true;
            txtCreatedBy.RightToLeft = RightToLeft.No;
            txtCreatedBy.Size = new Size(258, 30);
            txtCreatedBy.TabIndex = 1;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(926, 71);
            label16.Name = "label16";
            label16.Size = new Size(105, 23);
            label16.TabIndex = 0;
            label16.Text = "انشئ بواسطة";
            // 
            // txtCreatedAt
            // 
            txtCreatedAt.BackColor = Color.WhiteSmoke;
            txtCreatedAt.Location = new Point(666, 29);
            txtCreatedAt.Name = "txtCreatedAt";
            txtCreatedAt.ReadOnly = true;
            txtCreatedAt.RightToLeft = RightToLeft.No;
            txtCreatedAt.Size = new Size(258, 30);
            txtCreatedAt.TabIndex = 1;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(926, 33);
            label15.Name = "label15";
            label15.Size = new Size(94, 23);
            label15.TabIndex = 0;
            label15.Text = "تاريخ الانشاء";
            // 
            // gbBasicInfo
            // 
            gbBasicInfo.Controls.Add(label11);
            gbBasicInfo.Controls.Add(label6);
            gbBasicInfo.Controls.Add(cmbDefaultProject);
            gbBasicInfo.Controls.Add(cmbDefaultCostCenter);
            gbBasicInfo.Controls.Add(cmbAccountCategory);
            gbBasicInfo.Controls.Add(label12);
            gbBasicInfo.Controls.Add(label10);
            gbBasicInfo.Controls.Add(label5);
            gbBasicInfo.Controls.Add(cmbCurrency);
            gbBasicInfo.Controls.Add(cmbAccountType);
            gbBasicInfo.Controls.Add(label9);
            gbBasicInfo.Controls.Add(label4);
            gbBasicInfo.Controls.Add(cmbAccountStatus);
            gbBasicInfo.Controls.Add(label14);
            gbBasicInfo.Controls.Add(cmbNormalBalance);
            gbBasicInfo.Controls.Add(label8);
            gbBasicInfo.Controls.Add(cmbParentAccount);
            gbBasicInfo.Controls.Add(txtNotes);
            gbBasicInfo.Controls.Add(label3);
            gbBasicInfo.Controls.Add(txtAccountShortCode);
            gbBasicInfo.Controls.Add(txtAccountLevel);
            gbBasicInfo.Controls.Add(label7);
            gbBasicInfo.Controls.Add(txtAccountNameEN);
            gbBasicInfo.Controls.Add(label2);
            gbBasicInfo.Controls.Add(txtAccountCode);
            gbBasicInfo.Controls.Add(txtAccountNameAR);
            gbBasicInfo.Controls.Add(label13);
            gbBasicInfo.Controls.Add(label1);
            gbBasicInfo.Dock = DockStyle.Top;
            gbBasicInfo.Location = new Point(0, 0);
            gbBasicInfo.Name = "gbBasicInfo";
            gbBasicInfo.RightToLeft = RightToLeft.Yes;
            gbBasicInfo.Size = new Size(1132, 307);
            gbBasicInfo.TabIndex = 1;
            gbBasicInfo.TabStop = false;
            gbBasicInfo.Text = "بيانات الحساب";
            gbBasicInfo.Enter += gbBasicInfo_Enter;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(549, 222);
            label11.Name = "label11";
            label11.Size = new Size(74, 23);
            label11.TabIndex = 0;
            label11.Text = "ملاحظات";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(991, 245);
            label6.Name = "label6";
            label6.Size = new Size(121, 23);
            label6.TabIndex = 0;
            label6.Text = "مستوى الحساب";
            // 
            // cmbDefaultProject
            // 
            cmbDefaultProject.FormattingEnabled = true;
            cmbDefaultProject.Location = new Point(356, 183);
            cmbDefaultProject.Name = "cmbDefaultProject";
            cmbDefaultProject.RightToLeft = RightToLeft.No;
            cmbDefaultProject.Size = new Size(187, 31);
            cmbDefaultProject.TabIndex = 2;
            // 
            // cmbDefaultCostCenter
            // 
            cmbDefaultCostCenter.FormattingEnabled = true;
            cmbDefaultCostCenter.Location = new Point(356, 148);
            cmbDefaultCostCenter.Name = "cmbDefaultCostCenter";
            cmbDefaultCostCenter.RightToLeft = RightToLeft.No;
            cmbDefaultCostCenter.Size = new Size(187, 31);
            cmbDefaultCostCenter.TabIndex = 2;
            // 
            // cmbAccountCategory
            // 
            cmbAccountCategory.FormattingEnabled = true;
            cmbAccountCategory.Location = new Point(744, 205);
            cmbAccountCategory.Name = "cmbAccountCategory";
            cmbAccountCategory.RightToLeft = RightToLeft.No;
            cmbAccountCategory.Size = new Size(195, 31);
            cmbAccountCategory.TabIndex = 2;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(549, 183);
            label12.Name = "label12";
            label12.Size = new Size(139, 23);
            label12.TabIndex = 0;
            label12.Text = "المشروع الافتراضي";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(549, 148);
            label10.Name = "label10";
            label10.Size = new Size(167, 23);
            label10.TabIndex = 0;
            label10.Text = "مركز التكلفة الافتراضي";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(1040, 200);
            label5.Name = "label5";
            label5.Size = new Size(72, 23);
            label5.TabIndex = 0;
            label5.Text = "التصنيف";
            // 
            // cmbCurrency
            // 
            cmbCurrency.FormattingEnabled = true;
            cmbCurrency.Location = new Point(356, 113);
            cmbCurrency.Name = "cmbCurrency";
            cmbCurrency.RightToLeft = RightToLeft.No;
            cmbCurrency.Size = new Size(187, 31);
            cmbCurrency.TabIndex = 2;
            // 
            // cmbAccountType
            // 
            cmbAccountType.FormattingEnabled = true;
            cmbAccountType.Location = new Point(744, 170);
            cmbAccountType.Name = "cmbAccountType";
            cmbAccountType.RightToLeft = RightToLeft.No;
            cmbAccountType.Size = new Size(195, 31);
            cmbAccountType.TabIndex = 2;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(549, 113);
            label9.Name = "label9";
            label9.Size = new Size(55, 23);
            label9.TabIndex = 0;
            label9.Text = "العملة";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(1018, 166);
            label4.Name = "label4";
            label4.Size = new Size(94, 23);
            label4.TabIndex = 0;
            label4.Text = "نوع الحساب";
            // 
            // cmbAccountStatus
            // 
            cmbAccountStatus.FormattingEnabled = true;
            cmbAccountStatus.Location = new Point(355, 42);
            cmbAccountStatus.Name = "cmbAccountStatus";
            cmbAccountStatus.RightToLeft = RightToLeft.No;
            cmbAccountStatus.Size = new Size(187, 31);
            cmbAccountStatus.TabIndex = 2;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(548, 42);
            label14.Name = "label14";
            label14.Size = new Size(98, 23);
            label14.TabIndex = 0;
            label14.Text = "حالة الحساب";
            // 
            // cmbNormalBalance
            // 
            cmbNormalBalance.FormattingEnabled = true;
            cmbNormalBalance.Location = new Point(356, 78);
            cmbNormalBalance.Name = "cmbNormalBalance";
            cmbNormalBalance.RightToLeft = RightToLeft.No;
            cmbNormalBalance.Size = new Size(187, 31);
            cmbNormalBalance.TabIndex = 2;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(549, 78);
            label8.Name = "label8";
            label8.Size = new Size(114, 23);
            label8.TabIndex = 0;
            label8.Text = "طبيعة الحساب";
            // 
            // cmbParentAccount
            // 
            cmbParentAccount.FormattingEnabled = true;
            cmbParentAccount.Location = new Point(744, 137);
            cmbParentAccount.Name = "cmbParentAccount";
            cmbParentAccount.RightToLeft = RightToLeft.No;
            cmbParentAccount.Size = new Size(195, 31);
            cmbParentAccount.TabIndex = 2;
            cmbParentAccount.SelectedIndexChanged += cmbParentAccount_SelectedIndexChanged;
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(356, 219);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.RightToLeft = RightToLeft.No;
            txtNotes.Size = new Size(187, 63);
            txtNotes.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(1013, 131);
            label3.Name = "label3";
            label3.Size = new Size(99, 23);
            label3.TabIndex = 0;
            label3.Text = "الحساب الأب";
            // 
            // txtAccountShortCode
            // 
            txtAccountShortCode.Location = new Point(382, 6);
            txtAccountShortCode.Name = "txtAccountShortCode";
            txtAccountShortCode.RightToLeft = RightToLeft.No;
            txtAccountShortCode.Size = new Size(161, 30);
            txtAccountShortCode.TabIndex = 1;
            // 
            // txtAccountLevel
            // 
            txtAccountLevel.Location = new Point(811, 246);
            txtAccountLevel.Name = "txtAccountLevel";
            txtAccountLevel.RightToLeft = RightToLeft.No;
            txtAccountLevel.Size = new Size(128, 30);
            txtAccountLevel.TabIndex = 1;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(549, 9);
            label7.Name = "label7";
            label7.Size = new Size(92, 23);
            label7.TabIndex = 0;
            label7.Text = "رمز الحساب";
            // 
            // txtAccountNameEN
            // 
            txtAccountNameEN.Location = new Point(719, 101);
            txtAccountNameEN.Name = "txtAccountNameEN";
            txtAccountNameEN.RightToLeft = RightToLeft.No;
            txtAccountNameEN.Size = new Size(220, 30);
            txtAccountNameEN.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(943, 101);
            label2.Name = "label2";
            label2.Size = new Size(169, 23);
            label2.TabIndex = 0;
            label2.Text = "اسم الحساب بالانجليزي";
            // 
            // txtAccountCode
            // 
            txtAccountCode.Location = new Point(770, 27);
            txtAccountCode.Name = "txtAccountCode";
            txtAccountCode.RightToLeft = RightToLeft.No;
            txtAccountCode.Size = new Size(169, 30);
            txtAccountCode.TabIndex = 1;
            // 
            // txtAccountNameAR
            // 
            txtAccountNameAR.Location = new Point(719, 63);
            txtAccountNameAR.Name = "txtAccountNameAR";
            txtAccountNameAR.RightToLeft = RightToLeft.No;
            txtAccountNameAR.Size = new Size(220, 30);
            txtAccountNameAR.TabIndex = 1;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(1018, 30);
            label13.Name = "label13";
            label13.Size = new Size(94, 23);
            label13.TabIndex = 0;
            label13.Text = "رقم الحساب";
            label13.Click += label13_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(957, 66);
            label1.Name = "label1";
            label1.Size = new Size(155, 23);
            label1.TabIndex = 0;
            label1.Text = "اسم الحساب بالعربي";
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.RightToLeft = RightToLeft.Yes;
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // statusMain
            // 
            statusMain.ImageScalingSize = new Size(20, 20);
            statusMain.Items.AddRange(new ToolStripItem[] { lblAccountsCount, lblSep1, lblCurrentAccount, lblSep2, lblCurrentCompany, lblSep3, lblCurrentUser });
            statusMain.Location = new Point(0, 827);
            statusMain.Name = "statusMain";
            statusMain.Size = new Size(1132, 26);
            statusMain.TabIndex = 11;
            statusMain.Text = "statusStrip1";
            // 
            // lblAccountsCount
            // 
            lblAccountsCount.Enabled = false;
            lblAccountsCount.Name = "lblAccountsCount";
            lblAccountsCount.Size = new Size(109, 20);
            lblAccountsCount.Text = "عدد الحسابات: 0";
            // 
            // lblSep1
            // 
            lblSep1.Name = "lblSep1";
            lblSep1.Size = new Size(13, 20);
            lblSep1.Text = "|";
            // 
            // lblCurrentAccount
            // 
            lblCurrentAccount.Name = "lblCurrentAccount";
            lblCurrentAccount.Size = new Size(115, 20);
            lblCurrentAccount.Text = "الحساب الحالي: -";
            // 
            // lblSep2
            // 
            lblSep2.Name = "lblSep2";
            lblSep2.Size = new Size(13, 20);
            lblSep2.Text = "|";
            // 
            // lblCurrentCompany
            // 
            lblCurrentCompany.Name = "lblCurrentCompany";
            lblCurrentCompany.Size = new Size(63, 20);
            lblCurrentCompany.Text = "الشركة: -";
            // 
            // lblSep3
            // 
            lblSep3.Name = "lblSep3";
            lblSep3.Size = new Size(13, 20);
            lblSep3.Text = "|";
            // 
            // lblCurrentUser
            // 
            lblCurrentUser.Name = "lblCurrentUser";
            lblCurrentUser.Size = new Size(84, 20);
            lblCurrentUser.Text = "المستخدم: -";
            // 
            // chkAffectsIncomeStatement
            // 
            chkAffectsIncomeStatement.AutoSize = true;
            chkAffectsIncomeStatement.Location = new Point(366, 136);
            chkAffectsIncomeStatement.Name = "chkAffectsIncomeStatement";
            chkAffectsIncomeStatement.RightToLeft = RightToLeft.No;
            chkAffectsIncomeStatement.Size = new Size(134, 27);
            chkAffectsIncomeStatement.TabIndex = 4;
            chkAffectsIncomeStatement.Text = "تاثير في الدخل";
            chkAffectsIncomeStatement.UseVisualStyleBackColor = true;
            // 
            // chkRequiresParty
            // 
            chkRequiresParty.AutoSize = true;
            chkRequiresParty.Location = new Point(1026, 131);
            chkRequiresParty.Name = "chkRequiresParty";
            chkRequiresParty.RightToLeft = RightToLeft.No;
            chkRequiresParty.Size = new Size(110, 27);
            chkRequiresParty.TabIndex = 5;
            chkRequiresParty.Text = "يحتاج طرف";
            chkRequiresParty.UseVisualStyleBackColor = true;
            // 
            // chkAffectsBalanceSheet
            // 
            chkAffectsBalanceSheet.AutoSize = true;
            chkAffectsBalanceSheet.Location = new Point(562, 136);
            chkAffectsBalanceSheet.Name = "chkAffectsBalanceSheet";
            chkAffectsBalanceSheet.RightToLeft = RightToLeft.No;
            chkAffectsBalanceSheet.Size = new Size(151, 27);
            chkAffectsBalanceSheet.TabIndex = 6;
            chkAffectsBalanceSheet.Text = "تاثير في الميزانية";
            chkAffectsBalanceSheet.UseVisualStyleBackColor = true;
            // 
            // chkRequiresProject
            // 
            chkRequiresProject.AutoSize = true;
            chkRequiresProject.Location = new Point(719, 131);
            chkRequiresProject.Name = "chkRequiresProject";
            chkRequiresProject.RightToLeft = RightToLeft.No;
            chkRequiresProject.Size = new Size(121, 27);
            chkRequiresProject.TabIndex = 7;
            chkRequiresProject.Text = "يحتاج مشروع";
            chkRequiresProject.UseVisualStyleBackColor = true;
            // 
            // chkIsSummaryAccount
            // 
            chkIsSummaryAccount.AutoSize = true;
            chkIsSummaryAccount.Location = new Point(562, 103);
            chkIsSummaryAccount.Name = "chkIsSummaryAccount";
            chkIsSummaryAccount.RightToLeft = RightToLeft.No;
            chkIsSummaryAccount.Size = new Size(137, 27);
            chkIsSummaryAccount.TabIndex = 8;
            chkIsSummaryAccount.Text = "حساب تجميعي";
            chkIsSummaryAccount.UseVisualStyleBackColor = true;
            // 
            // chkRequiresCostCenter
            // 
            chkRequiresCostCenter.AutoSize = true;
            chkRequiresCostCenter.Location = new Point(719, 103);
            chkRequiresCostCenter.Name = "chkRequiresCostCenter";
            chkRequiresCostCenter.RightToLeft = RightToLeft.No;
            chkRequiresCostCenter.Size = new Size(149, 27);
            chkRequiresCostCenter.TabIndex = 9;
            chkRequiresCostCenter.Text = "يحتاج مركز تكلفة";
            chkRequiresCostCenter.UseVisualStyleBackColor = true;
            // 
            // chkMultiCurrency
            // 
            chkMultiCurrency.AutoSize = true;
            chkMultiCurrency.Location = new Point(874, 136);
            chkMultiCurrency.Name = "chkMultiCurrency";
            chkMultiCurrency.RightToLeft = RightToLeft.No;
            chkMultiCurrency.Size = new Size(116, 27);
            chkMultiCurrency.TabIndex = 10;
            chkMultiCurrency.Text = "يرتبط بعملة";
            chkMultiCurrency.UseVisualStyleBackColor = true;
            // 
            // chkAllowManualEntry
            // 
            chkAllowManualEntry.AutoSize = true;
            chkAllowManualEntry.Location = new Point(366, 103);
            chkAllowManualEntry.Name = "chkAllowManualEntry";
            chkAllowManualEntry.RightToLeft = RightToLeft.No;
            chkAllowManualEntry.Size = new Size(169, 27);
            chkAllowManualEntry.TabIndex = 11;
            chkAllowManualEntry.Text = "يسمح بالقيد اليدوي";
            chkAllowManualEntry.UseVisualStyleBackColor = true;
            // 
            // chkSystemAccount
            // 
            chkSystemAccount.AutoSize = true;
            chkSystemAccount.Location = new Point(874, 103);
            chkSystemAccount.Name = "chkSystemAccount";
            chkSystemAccount.RightToLeft = RightToLeft.No;
            chkSystemAccount.Size = new Size(129, 27);
            chkSystemAccount.TabIndex = 12;
            chkSystemAccount.Text = "حساب نظامي";
            chkSystemAccount.UseVisualStyleBackColor = true;
            // 
            // chkIsPostable
            // 
            chkIsPostable.AutoSize = true;
            chkIsPostable.Location = new Point(1026, 103);
            chkIsPostable.Name = "chkIsPostable";
            chkIsPostable.RightToLeft = RightToLeft.No;
            chkIsPostable.Size = new Size(113, 27);
            chkIsPostable.TabIndex = 13;
            chkIsPostable.Text = "يقبل الحركة";
            chkIsPostable.UseVisualStyleBackColor = true;
            // 
            // gbSubAccounts
            // 
            gbSubAccounts.Controls.Add(dgvSubAccounts);
            gbSubAccounts.Dock = DockStyle.Fill;
            gbSubAccounts.Location = new Point(0, 483);
            gbSubAccounts.Name = "gbSubAccounts";
            gbSubAccounts.Size = new Size(1132, 285);
            gbSubAccounts.TabIndex = 5;
            gbSubAccounts.TabStop = false;
            gbSubAccounts.Text = "الحسابات الفرعية";
            // 
            // dgvSubAccounts
            // 
            dgvSubAccounts.AllowUserToResizeRows = false;
            dgvSubAccounts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSubAccounts.Columns.AddRange(new DataGridViewColumn[] { colAccountCode, colAccountNameAr, colAccountNameEn, colAccountType, colAccountNature, colCurrency, colAccountLevel, colAccountStatus });
            dgvSubAccounts.Dock = DockStyle.Fill;
            dgvSubAccounts.Location = new Point(3, 26);
            dgvSubAccounts.Name = "dgvSubAccounts";
            dgvSubAccounts.RowHeadersWidth = 51;
            dgvSubAccounts.Size = new Size(1126, 256);
            dgvSubAccounts.TabIndex = 0;
            // 
            // colAccountCode
            // 
            colAccountCode.HeaderText = "رقم الحساب";
            colAccountCode.MinimumWidth = 6;
            colAccountCode.Name = "colAccountCode";
            colAccountCode.Width = 125;
            // 
            // colAccountNameAr
            // 
            colAccountNameAr.HeaderText = "اسم الحساب بالعربي";
            colAccountNameAr.MinimumWidth = 6;
            colAccountNameAr.Name = "colAccountNameAr";
            colAccountNameAr.Width = 125;
            // 
            // colAccountNameEn
            // 
            colAccountNameEn.HeaderText = "اسم الحساب بالانجليزي";
            colAccountNameEn.MinimumWidth = 6;
            colAccountNameEn.Name = "colAccountNameEn";
            colAccountNameEn.Width = 125;
            // 
            // colAccountType
            // 
            colAccountType.HeaderText = "نوع الحساب";
            colAccountType.MinimumWidth = 6;
            colAccountType.Name = "colAccountType";
            colAccountType.Width = 125;
            // 
            // colAccountNature
            // 
            colAccountNature.HeaderText = "طبيعة الحساب";
            colAccountNature.MinimumWidth = 6;
            colAccountNature.Name = "colAccountNature";
            colAccountNature.Width = 125;
            // 
            // colCurrency
            // 
            colCurrency.HeaderText = "العملة";
            colCurrency.MinimumWidth = 6;
            colCurrency.Name = "colCurrency";
            colCurrency.Width = 125;
            // 
            // colAccountLevel
            // 
            colAccountLevel.HeaderText = "مستوى الحساب";
            colAccountLevel.MinimumWidth = 6;
            colAccountLevel.Name = "colAccountLevel";
            colAccountLevel.Width = 125;
            // 
            // colAccountStatus
            // 
            colAccountStatus.HeaderText = "حالة الحساب";
            colAccountStatus.MinimumWidth = 6;
            colAccountStatus.Name = "colAccountStatus";
            colAccountStatus.Width = 125;
            // 
            // FrmChartOfAccounts
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1582, 853);
            Controls.Add(statusMain);
            Controls.Add(pnlMainDetails);
            Controls.Add(pnlTreeContainer);
            Controls.Add(pnlToolbar);
            Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(4, 5, 4, 5);
            Name = "FrmChartOfAccounts";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "شجرة الحسابات";
            WindowState = FormWindowState.Maximized;
            Load += FrmChartOfAccounts_Load;
            pnlToolbar.ResumeLayout(false);
            pnlTreeContainer.ResumeLayout(false);
            gbTreeContainer.ResumeLayout(false);
            gbTreeContainer.PerformLayout();
            pnlMainDetails.ResumeLayout(false);
            grpSystemInfo.ResumeLayout(false);
            grpSystemInfo.PerformLayout();
            gbBasicInfo.ResumeLayout(false);
            gbBasicInfo.PerformLayout();
            statusMain.ResumeLayout(false);
            statusMain.PerformLayout();
            gbSubAccounts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvSubAccounts).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlToolbar;
        private Button btnSave;
        private Button btnEdit;
        private Button btnNew;
        private Button btnPrint;
        private Button btnSearch;
        private Button btnRefresh;
        private Button btnDelete;
        private Panel pnlTreeContainer;
        private Panel pnlMainDetails;
        private GroupBox gbSubAccounts;
        private DataGridView dgvSubAccounts;
        private ContextMenuStrip contextMenuStrip1;
        private GroupBox gbBasicInfo;
        private TextBox txtCreatedAt;
        private CheckBox checkBox6;
        private CheckBox checkBox5;
        private CheckBox checkBox4;
        private CheckBox checkBox3;
        private CheckBox checkBox2;
        private Button button10;
        private Button btnClose;
        private Button button9;
        private GroupBox gbTreeContainer;
        private TextBox txtSearchTree;
        private TreeView tvAccounts;
        private ImageList imgAccountsTree;
        private TextBox txtAccountNameAr;
        private GroupBox grpSystemInfo;
        private TextBox txtUpdatedAt;
        private Label label17;
        private TextBox txtSystemStatus;
        private Label label19;
        private TextBox txtCreatedBy;
        private Label label16;
        private Label label15;
        private DataGridViewTextBoxColumn colAccountCode;
        private DataGridViewTextBoxColumn colAccountNameAr;
        private DataGridViewTextBoxColumn colAccountNameEn;
        private DataGridViewTextBoxColumn colAccountType;
        private DataGridViewTextBoxColumn colAccountNature;
        private DataGridViewTextBoxColumn colCurrency;
        private DataGridViewTextBoxColumn colAccountLevel;
        private DataGridViewTextBoxColumn colAccountStatus;
        private StatusStrip statusMain;
        private ToolStripStatusLabel lblAccountsCount;
        private ToolStripStatusLabel lblSep1;
        private ToolStripStatusLabel lblCurrentAccount;
        private ToolStripStatusLabel lblSep2;
        private ToolStripStatusLabel lblCurrentCompany;
        private ToolStripStatusLabel lblCurrentUser;
        private ToolStripStatusLabel lblSep3;
        private Label label11;
        private Label label6;
        private ComboBox cmbDefaultProject;
        private ComboBox cmbDefaultCostCenter;
        private ComboBox cmbAccountCategory;
        private Label label12;
        private Label label10;
        private Label label5;
        private ComboBox cmbCurrency;
        private ComboBox cmbAccountType;
        private Label label9;
        private Label label4;
        private ComboBox cmbAccountStatus;
        private Label label14;
        private ComboBox cmbNormalBalance;
        private Label label8;
        private ComboBox cmbParentAccount;
        private TextBox txtNotes;
        private Label label3;
        private TextBox txtAccountShortCode;
        private TextBox txtAccountLevel;
        private Label label7;
        private TextBox txtAccountNameEN;
        private Label label2;
        private TextBox txtAccountCode;
        private TextBox txtAccountNameAR;
        private Label label13;
        private Label label1;
        private CheckBox chkAffectsIncomeStatement;
        private CheckBox chkRequiresParty;
        private CheckBox chkAffectsBalanceSheet;
        private CheckBox chkRequiresProject;
        private CheckBox chkIsSummaryAccount;
        private CheckBox chkRequiresCostCenter;
        private CheckBox chkMultiCurrency;
        private CheckBox chkAllowManualEntry;
        private CheckBox chkSystemAccount;
        private CheckBox chkIsPostable;
    }
}