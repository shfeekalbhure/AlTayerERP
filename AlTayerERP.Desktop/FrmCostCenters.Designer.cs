namespace AlTayerERP.Desktop
{
    partial class FrmCostCenters
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
            grpCostCenterTree = new GroupBox();
            tvCostCenters = new TreeView();
            txtSearchTree = new TextBox();
            grpCostCenter = new GroupBox();
            chkIsPostable = new CheckBox();
            chkIsActive = new CheckBox();
            label11 = new Label();
            label6 = new Label();
            label5 = new Label();
            cmbCostCenterType = new ComboBox();
            label4 = new Label();
            cmbParentCostCenter = new ComboBox();
            txtNotes = new TextBox();
            label3 = new Label();
            txtLevel = new TextBox();
            txtCostCenterNameEN = new TextBox();
            label2 = new Label();
            txtCostCenterCode = new TextBox();
            txtCostCenterNameAR = new TextBox();
            label13 = new Label();
            label1 = new Label();
            grpSystemInfo = new GroupBox();
            txtUpdatedAt = new TextBox();
            label17 = new Label();
            txtSystemStatus = new TextBox();
            label19 = new Label();
            txtCreatedBy = new TextBox();
            label16 = new Label();
            txtCreatedAt = new TextBox();
            label15 = new Label();
            gbSubAccounts = new GroupBox();
            dgvCostCenters = new DataGridView();
            colCostCenterID = new DataGridViewTextBoxColumn();
            colCostCenterCode = new DataGridViewTextBoxColumn();
            colCostCenterNameAR = new DataGridViewTextBoxColumn();
            colCostCenterNameEN = new DataGridViewTextBoxColumn();
            colParentCostCenter = new DataGridViewTextBoxColumn();
            colCostCenterType = new DataGridViewTextBoxColumn();
            colLevel = new DataGridViewTextBoxColumn();
            colIsActive = new DataGridViewCheckBoxColumn();
            pnlStatusBar.SuspendLayout();
            pnlTopBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).BeginInit();
            grpCostCenterTree.SuspendLayout();
            grpCostCenter.SuspendLayout();
            grpSystemInfo.SuspendLayout();
            gbSubAccounts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCostCenters).BeginInit();
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
            pnlStatusBar.Location = new Point(0, 657);
            pnlStatusBar.Name = "pnlStatusBar";
            pnlStatusBar.Size = new Size(1227, 35);
            pnlStatusBar.TabIndex = 24;
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
            pnlTopBar.Size = new Size(1227, 58);
            pnlTopBar.TabIndex = 23;
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
            picCompanyLogo.Location = new Point(1077, 3);
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
            // grpCostCenterTree
            // 
            grpCostCenterTree.Controls.Add(tvCostCenters);
            grpCostCenterTree.Controls.Add(txtSearchTree);
            grpCostCenterTree.Dock = DockStyle.Left;
            grpCostCenterTree.Location = new Point(0, 58);
            grpCostCenterTree.Name = "grpCostCenterTree";
            grpCostCenterTree.Size = new Size(448, 599);
            grpCostCenterTree.TabIndex = 25;
            grpCostCenterTree.TabStop = false;
            grpCostCenterTree.Text = "مراكز التكلفه";
            // 
            // tvCostCenters
            // 
            tvCostCenters.Dock = DockStyle.Fill;
            tvCostCenters.FullRowSelect = true;
            tvCostCenters.Location = new Point(3, 50);
            tvCostCenters.Name = "tvCostCenters";
            tvCostCenters.Size = new Size(442, 546);
            tvCostCenters.TabIndex = 2;
            // 
            // txtSearchTree
            // 
            txtSearchTree.Dock = DockStyle.Top;
            txtSearchTree.Location = new Point(3, 23);
            txtSearchTree.Name = "txtSearchTree";
            txtSearchTree.PlaceholderText = "بحث في الشجرة";
            txtSearchTree.Size = new Size(442, 27);
            txtSearchTree.TabIndex = 1;
            // 
            // grpCostCenter
            // 
            grpCostCenter.Controls.Add(chkIsPostable);
            grpCostCenter.Controls.Add(chkIsActive);
            grpCostCenter.Controls.Add(label11);
            grpCostCenter.Controls.Add(label6);
            grpCostCenter.Controls.Add(label5);
            grpCostCenter.Controls.Add(cmbCostCenterType);
            grpCostCenter.Controls.Add(label4);
            grpCostCenter.Controls.Add(cmbParentCostCenter);
            grpCostCenter.Controls.Add(txtNotes);
            grpCostCenter.Controls.Add(label3);
            grpCostCenter.Controls.Add(txtLevel);
            grpCostCenter.Controls.Add(txtCostCenterNameEN);
            grpCostCenter.Controls.Add(label2);
            grpCostCenter.Controls.Add(txtCostCenterCode);
            grpCostCenter.Controls.Add(txtCostCenterNameAR);
            grpCostCenter.Controls.Add(label13);
            grpCostCenter.Controls.Add(label1);
            grpCostCenter.Dock = DockStyle.Top;
            grpCostCenter.Location = new Point(448, 58);
            grpCostCenter.Name = "grpCostCenter";
            grpCostCenter.RightToLeft = RightToLeft.Yes;
            grpCostCenter.Size = new Size(779, 307);
            grpCostCenter.TabIndex = 26;
            grpCostCenter.TabStop = false;
            grpCostCenter.Text = "بيانات مركز التكلفه";
            // 
            // chkIsPostable
            // 
            chkIsPostable.AutoSize = true;
            chkIsPostable.Location = new Point(149, 122);
            chkIsPostable.Name = "chkIsPostable";
            chkIsPostable.RightToLeft = RightToLeft.No;
            chkIsPostable.Size = new Size(101, 24);
            chkIsPostable.TabIndex = 30;
            chkIsPostable.Text = "يقبل القيود";
            chkIsPostable.UseVisualStyleBackColor = true;
            // 
            // chkIsActive
            // 
            chkIsActive.AutoSize = true;
            chkIsActive.Location = new Point(572, 265);
            chkIsActive.Name = "chkIsActive";
            chkIsActive.RightToLeft = RightToLeft.No;
            chkIsActive.Size = new Size(66, 24);
            chkIsActive.TabIndex = 29;
            chkIsActive.Text = "الحاله";
            chkIsActive.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(247, 229);
            label11.Name = "label11";
            label11.Size = new Size(67, 20);
            label11.TabIndex = 0;
            label11.Text = "ملاحظات";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(636, 215);
            label6.Name = "label6";
            label6.Size = new Size(64, 20);
            label6.TabIndex = 0;
            label6.Text = "المستوى";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(1040, 200);
            label5.Name = "label5";
            label5.Size = new Size(63, 20);
            label5.TabIndex = 0;
            label5.Text = "التصنيف";
            // 
            // cmbCostCenterType
            // 
            cmbCostCenterType.FormattingEnabled = true;
            cmbCostCenterType.Location = new Point(388, 177);
            cmbCostCenterType.Name = "cmbCostCenterType";
            cmbCostCenterType.RightToLeft = RightToLeft.No;
            cmbCostCenterType.Size = new Size(195, 28);
            cmbCostCenterType.TabIndex = 2;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(662, 173);
            label4.Name = "label4";
            label4.Size = new Size(72, 20);
            label4.TabIndex = 0;
            label4.Text = "نوع المركز";
            // 
            // cmbParentCostCenter
            // 
            cmbParentCostCenter.FormattingEnabled = true;
            cmbParentCostCenter.Location = new Point(388, 144);
            cmbParentCostCenter.Name = "cmbParentCostCenter";
            cmbParentCostCenter.RightToLeft = RightToLeft.No;
            cmbParentCostCenter.Size = new Size(195, 28);
            cmbParentCostCenter.TabIndex = 2;
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(54, 226);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.RightToLeft = RightToLeft.No;
            txtNotes.Size = new Size(187, 63);
            txtNotes.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(657, 138);
            label3.Name = "label3";
            label3.Size = new Size(77, 20);
            label3.TabIndex = 0;
            label3.Text = "المركز الأب";
            // 
            // txtLevel
            // 
            txtLevel.Location = new Point(456, 216);
            txtLevel.Name = "txtLevel";
            txtLevel.RightToLeft = RightToLeft.No;
            txtLevel.Size = new Size(128, 27);
            txtLevel.TabIndex = 1;
            // 
            // txtCostCenterNameEN
            // 
            txtCostCenterNameEN.Location = new Point(363, 108);
            txtCostCenterNameEN.Name = "txtCostCenterNameEN";
            txtCostCenterNameEN.RightToLeft = RightToLeft.No;
            txtCostCenterNameEN.Size = new Size(220, 27);
            txtCostCenterNameEN.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(587, 108);
            label2.Name = "label2";
            label2.Size = new Size(124, 20);
            label2.TabIndex = 0;
            label2.Text = "اسم المركز إنجليزي";
            // 
            // txtCostCenterCode
            // 
            txtCostCenterCode.Location = new Point(414, 34);
            txtCostCenterCode.Name = "txtCostCenterCode";
            txtCostCenterCode.RightToLeft = RightToLeft.No;
            txtCostCenterCode.Size = new Size(169, 27);
            txtCostCenterCode.TabIndex = 1;
            // 
            // txtCostCenterNameAR
            // 
            txtCostCenterNameAR.Location = new Point(363, 70);
            txtCostCenterNameAR.Name = "txtCostCenterNameAR";
            txtCostCenterNameAR.RightToLeft = RightToLeft.No;
            txtCostCenterNameAR.Size = new Size(220, 27);
            txtCostCenterNameAR.TabIndex = 1;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(662, 37);
            label13.Name = "label13";
            label13.Size = new Size(112, 20);
            label13.TabIndex = 0;
            label13.Text = "كود مركز التكلفه";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(601, 73);
            label1.Name = "label1";
            label1.Size = new Size(113, 20);
            label1.TabIndex = 0;
            label1.Text = "اسم المركز عربي";
            // 
            // grpSystemInfo
            // 
            grpSystemInfo.Controls.Add(txtUpdatedAt);
            grpSystemInfo.Controls.Add(label17);
            grpSystemInfo.Controls.Add(txtSystemStatus);
            grpSystemInfo.Controls.Add(label19);
            grpSystemInfo.Controls.Add(txtCreatedBy);
            grpSystemInfo.Controls.Add(label16);
            grpSystemInfo.Controls.Add(txtCreatedAt);
            grpSystemInfo.Controls.Add(label15);
            grpSystemInfo.Dock = DockStyle.Bottom;
            grpSystemInfo.Location = new Point(448, 581);
            grpSystemInfo.Name = "grpSystemInfo";
            grpSystemInfo.Size = new Size(779, 76);
            grpSystemInfo.TabIndex = 27;
            grpSystemInfo.TabStop = false;
            grpSystemInfo.Text = "معلومات النظام";
            // 
            // txtUpdatedAt
            // 
            txtUpdatedAt.BackColor = Color.WhiteSmoke;
            txtUpdatedAt.Location = new Point(319, 26);
            txtUpdatedAt.Name = "txtUpdatedAt";
            txtUpdatedAt.ReadOnly = true;
            txtUpdatedAt.RightToLeft = RightToLeft.No;
            txtUpdatedAt.Size = new Size(97, 27);
            txtUpdatedAt.TabIndex = 1;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(422, 31);
            label17.Name = "label17";
            label17.Size = new Size(69, 20);
            label17.TabIndex = 0;
            label17.Text = "اخر تعديل";
            // 
            // txtSystemStatus
            // 
            txtSystemStatus.BackColor = Color.WhiteSmoke;
            txtSystemStatus.Location = new Point(77, 27);
            txtSystemStatus.Name = "txtSystemStatus";
            txtSystemStatus.ReadOnly = true;
            txtSystemStatus.RightToLeft = RightToLeft.No;
            txtSystemStatus.Size = new Size(154, 27);
            txtSystemStatus.TabIndex = 1;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(233, 31);
            label19.Name = "label19";
            label19.Size = new Size(44, 20);
            label19.TabIndex = 0;
            label19.Text = "الحالة";
            // 
            // txtCreatedBy
            // 
            txtCreatedBy.BackColor = Color.WhiteSmoke;
            txtCreatedBy.Location = new Point(527, 31);
            txtCreatedBy.Name = "txtCreatedBy";
            txtCreatedBy.ReadOnly = true;
            txtCreatedBy.RightToLeft = RightToLeft.No;
            txtCreatedBy.Size = new Size(157, 27);
            txtCreatedBy.TabIndex = 1;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(690, 34);
            label16.Name = "label16";
            label16.Size = new Size(94, 20);
            label16.TabIndex = 0;
            label16.Text = "انشئ بواسطة";
            // 
            // txtCreatedAt
            // 
            txtCreatedAt.BackColor = Color.WhiteSmoke;
            txtCreatedAt.Location = new Point(789, 35);
            txtCreatedAt.Name = "txtCreatedAt";
            txtCreatedAt.ReadOnly = true;
            txtCreatedAt.RightToLeft = RightToLeft.No;
            txtCreatedAt.Size = new Size(157, 27);
            txtCreatedAt.TabIndex = 1;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(952, 38);
            label15.Name = "label15";
            label15.Size = new Size(84, 20);
            label15.TabIndex = 0;
            label15.Text = "تاريخ الانشاء";
            // 
            // gbSubAccounts
            // 
            gbSubAccounts.Controls.Add(dgvCostCenters);
            gbSubAccounts.Dock = DockStyle.Fill;
            gbSubAccounts.Location = new Point(448, 365);
            gbSubAccounts.Name = "gbSubAccounts";
            gbSubAccounts.RightToLeft = RightToLeft.Yes;
            gbSubAccounts.Size = new Size(779, 216);
            gbSubAccounts.TabIndex = 28;
            gbSubAccounts.TabStop = false;
            gbSubAccounts.Text = "المراكز الفرعيه";
            // 
            // dgvCostCenters
            // 
            dgvCostCenters.AllowUserToResizeRows = false;
            dgvCostCenters.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCostCenters.Columns.AddRange(new DataGridViewColumn[] { colCostCenterID, colCostCenterCode, colCostCenterNameAR, colCostCenterNameEN, colParentCostCenter, colCostCenterType, colLevel, colIsActive });
            dgvCostCenters.Dock = DockStyle.Fill;
            dgvCostCenters.Location = new Point(3, 23);
            dgvCostCenters.Name = "dgvCostCenters";
            dgvCostCenters.RightToLeft = RightToLeft.Yes;
            dgvCostCenters.RowHeadersWidth = 51;
            dgvCostCenters.Size = new Size(773, 190);
            dgvCostCenters.TabIndex = 0;
            // 
            // colCostCenterID
            // 
            colCostCenterID.DataPropertyName = "Cost_Center_ID";
            colCostCenterID.HeaderText = "رقم";
            colCostCenterID.MinimumWidth = 6;
            colCostCenterID.Name = "colCostCenterID";
            colCostCenterID.Width = 125;
            // 
            // colCostCenterCode
            // 
            colCostCenterCode.DataPropertyName = "Center_Code";
            colCostCenterCode.HeaderText = "الكود";
            colCostCenterCode.MinimumWidth = 6;
            colCostCenterCode.Name = "colCostCenterCode";
            colCostCenterCode.Width = 125;
            // 
            // colCostCenterNameAR
            // 
            colCostCenterNameAR.DataPropertyName = "Center_Name_AR";
            colCostCenterNameAR.HeaderText = "اسم المركز عربي";
            colCostCenterNameAR.MinimumWidth = 6;
            colCostCenterNameAR.Name = "colCostCenterNameAR";
            colCostCenterNameAR.Width = 125;
            // 
            // colCostCenterNameEN
            // 
            colCostCenterNameEN.DataPropertyName = "Center_Name_EN";
            colCostCenterNameEN.HeaderText = "اسم المركز انجليزي";
            colCostCenterNameEN.MinimumWidth = 6;
            colCostCenterNameEN.Name = "colCostCenterNameEN";
            colCostCenterNameEN.Width = 125;
            // 
            // colParentCostCenter
            // 
            colParentCostCenter.DataPropertyName = "Parent_Cost_Center_ID";
            colParentCostCenter.HeaderText = "المركز الاب";
            colParentCostCenter.MinimumWidth = 6;
            colParentCostCenter.Name = "colParentCostCenter";
            colParentCostCenter.Width = 125;
            // 
            // colCostCenterType
            // 
            colCostCenterType.HeaderText = "النوع";
            colCostCenterType.MinimumWidth = 6;
            colCostCenterType.Name = "colCostCenterType";
            colCostCenterType.Width = 125;
            // 
            // colLevel
            // 
            colLevel.DataPropertyName = "Center_Level";
            colLevel.HeaderText = "المستوى";
            colLevel.MinimumWidth = 6;
            colLevel.Name = "colLevel";
            colLevel.Width = 125;
            // 
            // colIsActive
            // 
            colIsActive.DataPropertyName = "Is_Active";
            colIsActive.HeaderText = "الحالة";
            colIsActive.MinimumWidth = 6;
            colIsActive.Name = "colIsActive";
            colIsActive.Resizable = DataGridViewTriState.True;
            colIsActive.SortMode = DataGridViewColumnSortMode.Automatic;
            colIsActive.Width = 125;
            // 
            // FrmCostCenters
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1227, 692);
            Controls.Add(gbSubAccounts);
            Controls.Add(grpSystemInfo);
            Controls.Add(grpCostCenter);
            Controls.Add(grpCostCenterTree);
            Controls.Add(pnlStatusBar);
            Controls.Add(pnlTopBar);
            Name = "FrmCostCenters";
            Text = "شاشة مركز التكلفة";
            pnlStatusBar.ResumeLayout(false);
            pnlStatusBar.PerformLayout();
            pnlTopBar.ResumeLayout(false);
            pnlTopBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picCompanyLogo).EndInit();
            grpCostCenterTree.ResumeLayout(false);
            grpCostCenterTree.PerformLayout();
            grpCostCenter.ResumeLayout(false);
            grpCostCenter.PerformLayout();
            grpSystemInfo.ResumeLayout(false);
            grpSystemInfo.PerformLayout();
            gbSubAccounts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCostCenters).EndInit();
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
        private GroupBox grpCostCenterTree;
        private TreeView tvCostCenters;
        private TextBox txtSearchTree;
        private GroupBox grpCostCenter;
        private Label label11;
        private Label label6;
        private Label label5;
        private ComboBox cmbCostCenterType;
        private Label label4;
        private ComboBox cmbParentCostCenter;
        private TextBox txtNotes;
        private Label label3;
        private TextBox txtLevel;
        private TextBox txtCostCenterNameEN;
        private Label label2;
        private TextBox txtCostCenterCode;
        private TextBox txtCostCenterNameAR;
        private Label label13;
        private Label label1;
        private GroupBox grpSystemInfo;
        private TextBox txtUpdatedAt;
        private Label label17;
        private TextBox txtSystemStatus;
        private Label label19;
        private TextBox txtCreatedBy;
        private Label label16;
        private TextBox txtCreatedAt;
        private Label label15;
        private GroupBox gbSubAccounts;
        private DataGridView dgvCostCenters;
        private CheckBox chkIsActive;
        private CheckBox chkIsPostable;
        private DataGridViewTextBoxColumn colCostCenterID;
        private DataGridViewTextBoxColumn colCostCenterCode;
        private DataGridViewTextBoxColumn colCostCenterNameAR;
        private DataGridViewTextBoxColumn colCostCenterNameEN;
        private DataGridViewTextBoxColumn colParentCostCenter;
        private DataGridViewTextBoxColumn colCostCenterType;
        private DataGridViewTextBoxColumn colLevel;
        private DataGridViewCheckBoxColumn colIsActive;
    }
}