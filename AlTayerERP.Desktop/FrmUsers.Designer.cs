namespace AlTayerERP.Desktop
{
    partial class FrmUsers
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
            pnlToolbar = new Panel();
            btnRefresh = new Button();
            btnSearch = new Button();
            btnDelete = new Button();
            btnClose = new Button();
            btnEdit = new Button();
            btnPrint = new Button();
            btnNew = new Button();
            btnSave = new Button();
            pnlGrid = new Panel();
            dgvUsers = new DataGridView();
            pnlData = new Panel();
            grpPermissions = new GroupBox();
            tabPermissions = new TabControl();
            tabExtraPermissions = new TabPage();
            dataGridView1 = new DataGridView();
            colExtraNo = new DataGridViewTextBoxColumn();
            colExtraPermission = new DataGridViewTextBoxColumn();
            colAllow = new DataGridViewCheckBoxColumn();
            tabReportPermissions = new TabPage();
            dataGridView2 = new DataGridView();
            colReportNo = new DataGridViewTextBoxColumn();
            colReportName = new DataGridViewTextBoxColumn();
            colReportView = new DataGridViewCheckBoxColumn();
            colReportPrint = new DataGridViewCheckBoxColumn();
            colExportExcel = new DataGridViewCheckBoxColumn();
            colExportPdf = new DataGridViewCheckBoxColumn();
            colViewProfit = new DataGridViewCheckBoxColumn();
            colViewCost = new DataGridViewCheckBoxColumn();
            tabDataPermissions = new TabPage();
            dgvDataPermissions = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            colDataName = new DataGridViewTextBoxColumn();
            colDataView = new DataGridViewCheckBoxColumn();
            colDataAdd = new DataGridViewCheckBoxColumn();
            colDataEdit = new DataGridViewCheckBoxColumn();
            colDataDelete = new DataGridViewCheckBoxColumn();
            colEditAfterApprove = new DataGridViewCheckBoxColumn();
            colDeleteAfterApprove = new DataGridViewCheckBoxColumn();
            colChangeDate = new DataGridViewCheckBoxColumn();
            tabFunctionPermissions = new TabPage();
            dgvFunctionPermissions = new DataGridView();
            colNo = new DataGridViewTextBoxColumn();
            colFunction = new DataGridViewTextBoxColumn();
            colView = new DataGridViewCheckBoxColumn();
            colAdd = new DataGridViewCheckBoxColumn();
            colEdit = new DataGridViewCheckBoxColumn();
            colDelete = new DataGridViewCheckBoxColumn();
            colPrint = new DataGridViewCheckBoxColumn();
            colApprove = new DataGridViewCheckBoxColumn();
            pnlPermissionHeader = new Panel();
            chkSelectAll = new CheckBox();
            cmbPermissionScreen = new ComboBox();
            label15 = new Label();
            cmbPermissionType = new ComboBox();
            label16 = new Label();
            cmbPermissionModule = new ComboBox();
            label13 = new Label();
            cmbPermissionSearch = new ComboBox();
            cmbPermissionRole = new ComboBox();
            label14 = new Label();
            label12 = new Label();
            grpUserData = new GroupBox();
            panel1 = new Panel();
            label11 = new Label();
            txtFullName = new TextBox();
            chkIsActive = new CheckBox();
            chkChangePassword = new CheckBox();
            label8 = new Label();
            label9 = new Label();
            label10 = new Label();
            txtNotes = new TextBox();
            txtEmail = new TextBox();
            txtPhone = new TextBox();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            cmbStatus = new ComboBox();
            cmbRole = new ComboBox();
            cmbBranch = new ComboBox();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            txtConfirmPassword = new TextBox();
            txtPassword = new TextBox();
            txtLoginName = new TextBox();
            txtUserName = new TextBox();
            pnlToolbar.SuspendLayout();
            pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUsers).BeginInit();
            pnlData.SuspendLayout();
            grpPermissions.SuspendLayout();
            tabPermissions.SuspendLayout();
            tabExtraPermissions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tabReportPermissions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            tabDataPermissions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDataPermissions).BeginInit();
            tabFunctionPermissions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFunctionPermissions).BeginInit();
            pnlPermissionHeader.SuspendLayout();
            grpUserData.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = Color.WhiteSmoke;
            pnlToolbar.BorderStyle = BorderStyle.FixedSingle;
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(btnSearch);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Controls.Add(btnClose);
            pnlToolbar.Controls.Add(btnEdit);
            pnlToolbar.Controls.Add(btnPrint);
            pnlToolbar.Controls.Add(btnNew);
            pnlToolbar.Controls.Add(btnSave);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(1315, 70);
            pnlToolbar.TabIndex = 0;
            pnlToolbar.Paint += pnlToolbar_Paint;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(236, 2);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.RightToLeft = RightToLeft.No;
            btnRefresh.Size = new Size(94, 29);
            btnRefresh.TabIndex = 35;
            btnRefresh.Text = "تحديث";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(336, 2);
            btnSearch.Name = "btnSearch";
            btnSearch.RightToLeft = RightToLeft.No;
            btnSearch.Size = new Size(94, 29);
            btnSearch.TabIndex = 37;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(433, 2);
            btnDelete.Name = "btnDelete";
            btnDelete.RightToLeft = RightToLeft.No;
            btnDelete.Size = new Size(94, 29);
            btnDelete.TabIndex = 40;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(33, 3);
            btnClose.Name = "btnClose";
            btnClose.RightToLeft = RightToLeft.No;
            btnClose.Size = new Size(94, 29);
            btnClose.TabIndex = 41;
            btnClose.Text = "اغلاق";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(533, 3);
            btnEdit.Name = "btnEdit";
            btnEdit.RightToLeft = RightToLeft.No;
            btnEdit.Size = new Size(94, 29);
            btnEdit.TabIndex = 42;
            btnEdit.Text = "تعديل";
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(133, 3);
            btnPrint.Name = "btnPrint";
            btnPrint.RightToLeft = RightToLeft.No;
            btnPrint.Size = new Size(94, 29);
            btnPrint.TabIndex = 43;
            btnPrint.Text = "طباعة";
            btnPrint.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(735, 3);
            btnNew.Name = "btnNew";
            btnNew.RightToLeft = RightToLeft.No;
            btnNew.Size = new Size(94, 29);
            btnNew.TabIndex = 44;
            btnNew.Text = "جديد";
            btnNew.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(635, 3);
            btnSave.Name = "btnSave";
            btnSave.RightToLeft = RightToLeft.No;
            btnSave.Size = new Size(94, 29);
            btnSave.TabIndex = 32;
            btnSave.Text = "حفظ";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // pnlGrid
            // 
            pnlGrid.BackColor = Color.White;
            pnlGrid.BorderStyle = BorderStyle.FixedSingle;
            pnlGrid.Controls.Add(dgvUsers);
            pnlGrid.Dock = DockStyle.Bottom;
            pnlGrid.Location = new Point(0, 592);
            pnlGrid.Name = "pnlGrid";
            pnlGrid.Size = new Size(1315, 200);
            pnlGrid.TabIndex = 0;
            // 
            // dgvUsers
            // 
            dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUsers.Dock = DockStyle.Fill;
            dgvUsers.Location = new Point(0, 0);
            dgvUsers.Name = "dgvUsers";
            dgvUsers.RowHeadersWidth = 51;
            dgvUsers.Size = new Size(1313, 198);
            dgvUsers.TabIndex = 0;
            // 
            // pnlData
            // 
            pnlData.Controls.Add(grpPermissions);
            pnlData.Controls.Add(grpUserData);
            pnlData.Dock = DockStyle.Fill;
            pnlData.Location = new Point(0, 70);
            pnlData.Name = "pnlData";
            pnlData.Size = new Size(1315, 522);
            pnlData.TabIndex = 1;
            // 
            // grpPermissions
            // 
            grpPermissions.BackColor = Color.FromArgb(0, 0, 192);
            grpPermissions.Controls.Add(tabPermissions);
            grpPermissions.Controls.Add(pnlPermissionHeader);
            grpPermissions.Dock = DockStyle.Fill;
            grpPermissions.Font = new Font("Tahoma", 12F);
            grpPermissions.ForeColor = Color.White;
            grpPermissions.Location = new Point(0, 0);
            grpPermissions.Name = "grpPermissions";
            grpPermissions.RightToLeft = RightToLeft.Yes;
            grpPermissions.Size = new Size(852, 522);
            grpPermissions.TabIndex = 3;
            grpPermissions.TabStop = false;
            grpPermissions.Text = "صلاحيات المستخدمين";
            // 
            // tabPermissions
            // 
            tabPermissions.Controls.Add(tabExtraPermissions);
            tabPermissions.Controls.Add(tabReportPermissions);
            tabPermissions.Controls.Add(tabDataPermissions);
            tabPermissions.Controls.Add(tabFunctionPermissions);
            tabPermissions.Dock = DockStyle.Fill;
            tabPermissions.Location = new Point(3, 112);
            tabPermissions.Name = "tabPermissions";
            tabPermissions.SelectedIndex = 0;
            tabPermissions.Size = new Size(846, 407);
            tabPermissions.TabIndex = 2;
            // 
            // tabExtraPermissions
            // 
            tabExtraPermissions.Controls.Add(dataGridView1);
            tabExtraPermissions.Location = new Point(4, 33);
            tabExtraPermissions.Name = "tabExtraPermissions";
            tabExtraPermissions.Padding = new Padding(3);
            tabExtraPermissions.Size = new Size(838, 370);
            tabExtraPermissions.TabIndex = 0;
            tabExtraPermissions.Text = "صلاحيات إضافيه";
            tabExtraPermissions.UseVisualStyleBackColor = true;
            tabExtraPermissions.Click += tabPage1_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colExtraNo, colExtraPermission, colAllow });
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(3, 3);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(832, 364);
            dataGridView1.TabIndex = 5;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            // 
            // colExtraNo
            // 
            colExtraNo.HeaderText = "م";
            colExtraNo.MinimumWidth = 6;
            colExtraNo.Name = "colExtraNo";
            // 
            // colExtraPermission
            // 
            colExtraPermission.HeaderText = "الصلاحية";
            colExtraPermission.MinimumWidth = 6;
            colExtraPermission.Name = "colExtraPermission";
            // 
            // colAllow
            // 
            colAllow.HeaderText = "السماح";
            colAllow.MinimumWidth = 6;
            colAllow.Name = "colAllow";
            colAllow.Resizable = DataGridViewTriState.True;
            colAllow.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // tabReportPermissions
            // 
            tabReportPermissions.Controls.Add(dataGridView2);
            tabReportPermissions.Location = new Point(4, 33);
            tabReportPermissions.Name = "tabReportPermissions";
            tabReportPermissions.Padding = new Padding(3);
            tabReportPermissions.Size = new Size(838, 370);
            tabReportPermissions.TabIndex = 1;
            tabReportPermissions.Text = "صلاحيات التقارير";
            tabReportPermissions.UseVisualStyleBackColor = true;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Columns.AddRange(new DataGridViewColumn[] { colReportNo, colReportName, colReportView, colReportPrint, colExportExcel, colExportPdf, colViewProfit, colViewCost });
            dataGridView2.Dock = DockStyle.Fill;
            dataGridView2.Location = new Point(3, 3);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.RowHeadersWidth = 51;
            dataGridView2.Size = new Size(832, 364);
            dataGridView2.TabIndex = 5;
            // 
            // colReportNo
            // 
            colReportNo.HeaderText = "م";
            colReportNo.MinimumWidth = 6;
            colReportNo.Name = "colReportNo";
            // 
            // colReportName
            // 
            colReportName.HeaderText = "التقرير";
            colReportName.MinimumWidth = 6;
            colReportName.Name = "colReportName";
            // 
            // colReportView
            // 
            colReportView.HeaderText = "عرض";
            colReportView.MinimumWidth = 6;
            colReportView.Name = "colReportView";
            colReportView.Resizable = DataGridViewTriState.True;
            colReportView.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colReportPrint
            // 
            colReportPrint.HeaderText = "طباعة";
            colReportPrint.MinimumWidth = 6;
            colReportPrint.Name = "colReportPrint";
            colReportPrint.Resizable = DataGridViewTriState.True;
            colReportPrint.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colExportExcel
            // 
            colExportExcel.HeaderText = "تصدير Excel";
            colExportExcel.MinimumWidth = 6;
            colExportExcel.Name = "colExportExcel";
            colExportExcel.Resizable = DataGridViewTriState.True;
            colExportExcel.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colExportPdf
            // 
            colExportPdf.HeaderText = "تصدير PDF";
            colExportPdf.MinimumWidth = 6;
            colExportPdf.Name = "colExportPdf";
            colExportPdf.Resizable = DataGridViewTriState.True;
            colExportPdf.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colViewProfit
            // 
            colViewProfit.HeaderText = "عرض الأرباح";
            colViewProfit.MinimumWidth = 6;
            colViewProfit.Name = "colViewProfit";
            // 
            // colViewCost
            // 
            colViewCost.HeaderText = "عرض التكاليف";
            colViewCost.MinimumWidth = 6;
            colViewCost.Name = "colViewCost";
            // 
            // tabDataPermissions
            // 
            tabDataPermissions.Controls.Add(dgvDataPermissions);
            tabDataPermissions.Location = new Point(4, 33);
            tabDataPermissions.Name = "tabDataPermissions";
            tabDataPermissions.Padding = new Padding(3);
            tabDataPermissions.Size = new Size(838, 370);
            tabDataPermissions.TabIndex = 2;
            tabDataPermissions.Text = "صلاحيات البيانات";
            tabDataPermissions.UseVisualStyleBackColor = true;
            // 
            // dgvDataPermissions
            // 
            dgvDataPermissions.AllowUserToAddRows = false;
            dgvDataPermissions.AllowUserToDeleteRows = false;
            dgvDataPermissions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDataPermissions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDataPermissions.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, colDataName, colDataView, colDataAdd, colDataEdit, colDataDelete, colEditAfterApprove, colDeleteAfterApprove, colChangeDate });
            dgvDataPermissions.Dock = DockStyle.Fill;
            dgvDataPermissions.Location = new Point(3, 3);
            dgvDataPermissions.Name = "dgvDataPermissions";
            dgvDataPermissions.RowHeadersVisible = false;
            dgvDataPermissions.RowHeadersWidth = 51;
            dgvDataPermissions.Size = new Size(832, 364);
            dgvDataPermissions.TabIndex = 4;
            dgvDataPermissions.CellContentClick += dgvDataPermissions_CellContentClick;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "م";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // colDataName
            // 
            colDataName.HeaderText = "البيان";
            colDataName.MinimumWidth = 6;
            colDataName.Name = "colDataName";
            // 
            // colDataView
            // 
            colDataView.HeaderText = "عرض";
            colDataView.MinimumWidth = 6;
            colDataView.Name = "colDataView";
            colDataView.Resizable = DataGridViewTriState.True;
            colDataView.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colDataAdd
            // 
            colDataAdd.HeaderText = "اضافة";
            colDataAdd.MinimumWidth = 6;
            colDataAdd.Name = "colDataAdd";
            colDataAdd.Resizable = DataGridViewTriState.True;
            colDataAdd.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colDataEdit
            // 
            colDataEdit.HeaderText = "تعديل";
            colDataEdit.MinimumWidth = 6;
            colDataEdit.Name = "colDataEdit";
            colDataEdit.Resizable = DataGridViewTriState.True;
            colDataEdit.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colDataDelete
            // 
            colDataDelete.HeaderText = "حذف";
            colDataDelete.MinimumWidth = 6;
            colDataDelete.Name = "colDataDelete";
            colDataDelete.Resizable = DataGridViewTriState.True;
            colDataDelete.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colEditAfterApprove
            // 
            colEditAfterApprove.HeaderText = "تعديل بعد الاعتماد";
            colEditAfterApprove.MinimumWidth = 6;
            colEditAfterApprove.Name = "colEditAfterApprove";
            // 
            // colDeleteAfterApprove
            // 
            colDeleteAfterApprove.HeaderText = "حذف بعد الاعتماد";
            colDeleteAfterApprove.MinimumWidth = 6;
            colDeleteAfterApprove.Name = "colDeleteAfterApprove";
            // 
            // colChangeDate
            // 
            colChangeDate.HeaderText = "تغيير التاريخ";
            colChangeDate.MinimumWidth = 6;
            colChangeDate.Name = "colChangeDate";
            // 
            // tabFunctionPermissions
            // 
            tabFunctionPermissions.Controls.Add(dgvFunctionPermissions);
            tabFunctionPermissions.Location = new Point(4, 33);
            tabFunctionPermissions.Name = "tabFunctionPermissions";
            tabFunctionPermissions.Padding = new Padding(3);
            tabFunctionPermissions.Size = new Size(838, 370);
            tabFunctionPermissions.TabIndex = 3;
            tabFunctionPermissions.Text = "صلاحيات الوظائف";
            tabFunctionPermissions.UseVisualStyleBackColor = true;
            // 
            // dgvFunctionPermissions
            // 
            dgvFunctionPermissions.AllowUserToAddRows = false;
            dgvFunctionPermissions.AllowUserToDeleteRows = false;
            dgvFunctionPermissions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFunctionPermissions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvFunctionPermissions.Columns.AddRange(new DataGridViewColumn[] { colNo, colFunction, colView, colAdd, colEdit, colDelete, colPrint, colApprove });
            dgvFunctionPermissions.Dock = DockStyle.Fill;
            dgvFunctionPermissions.Location = new Point(3, 3);
            dgvFunctionPermissions.Name = "dgvFunctionPermissions";
            dgvFunctionPermissions.RowHeadersVisible = false;
            dgvFunctionPermissions.RowHeadersWidth = 51;
            dgvFunctionPermissions.Size = new Size(832, 364);
            dgvFunctionPermissions.TabIndex = 3;
            // 
            // colNo
            // 
            colNo.HeaderText = "م";
            colNo.MinimumWidth = 6;
            colNo.Name = "colNo";
            // 
            // colFunction
            // 
            colFunction.HeaderText = "الوظيفه";
            colFunction.MinimumWidth = 6;
            colFunction.Name = "colFunction";
            // 
            // colView
            // 
            colView.HeaderText = "عرض";
            colView.MinimumWidth = 6;
            colView.Name = "colView";
            colView.Resizable = DataGridViewTriState.True;
            colView.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colAdd
            // 
            colAdd.HeaderText = "اضافة";
            colAdd.MinimumWidth = 6;
            colAdd.Name = "colAdd";
            colAdd.Resizable = DataGridViewTriState.True;
            colAdd.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colEdit
            // 
            colEdit.HeaderText = "تعديل";
            colEdit.MinimumWidth = 6;
            colEdit.Name = "colEdit";
            colEdit.Resizable = DataGridViewTriState.True;
            colEdit.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colDelete
            // 
            colDelete.HeaderText = "حذف";
            colDelete.MinimumWidth = 6;
            colDelete.Name = "colDelete";
            colDelete.Resizable = DataGridViewTriState.True;
            colDelete.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // colPrint
            // 
            colPrint.HeaderText = "طباعه";
            colPrint.MinimumWidth = 6;
            colPrint.Name = "colPrint";
            // 
            // colApprove
            // 
            colApprove.HeaderText = "اعتماد";
            colApprove.MinimumWidth = 6;
            colApprove.Name = "colApprove";
            // 
            // pnlPermissionHeader
            // 
            pnlPermissionHeader.BackColor = Color.White;
            pnlPermissionHeader.Controls.Add(chkSelectAll);
            pnlPermissionHeader.Controls.Add(cmbPermissionScreen);
            pnlPermissionHeader.Controls.Add(label15);
            pnlPermissionHeader.Controls.Add(cmbPermissionType);
            pnlPermissionHeader.Controls.Add(label16);
            pnlPermissionHeader.Controls.Add(cmbPermissionModule);
            pnlPermissionHeader.Controls.Add(label13);
            pnlPermissionHeader.Controls.Add(cmbPermissionSearch);
            pnlPermissionHeader.Controls.Add(cmbPermissionRole);
            pnlPermissionHeader.Controls.Add(label14);
            pnlPermissionHeader.Controls.Add(label12);
            pnlPermissionHeader.Dock = DockStyle.Top;
            pnlPermissionHeader.ForeColor = Color.Black;
            pnlPermissionHeader.Location = new Point(3, 28);
            pnlPermissionHeader.Name = "pnlPermissionHeader";
            pnlPermissionHeader.Size = new Size(846, 84);
            pnlPermissionHeader.TabIndex = 0;
            pnlPermissionHeader.Paint += pnlPermissionHeader_Paint;
            // 
            // chkSelectAll
            // 
            chkSelectAll.AutoSize = true;
            chkSelectAll.Location = new Point(358, 30);
            chkSelectAll.Name = "chkSelectAll";
            chkSelectAll.Size = new Size(119, 28);
            chkSelectAll.TabIndex = 70;
            chkSelectAll.Text = "اختيار الكل";
            chkSelectAll.UseVisualStyleBackColor = true;
            // 
            // cmbPermissionScreen
            // 
            cmbPermissionScreen.FormattingEnabled = true;
            cmbPermissionScreen.Location = new Point(3, 26);
            cmbPermissionScreen.Name = "cmbPermissionScreen";
            cmbPermissionScreen.Size = new Size(158, 32);
            cmbPermissionScreen.TabIndex = 56;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(16, -1);
            label15.Name = "label15";
            label15.Size = new Size(85, 24);
            label15.TabIndex = 59;
            label15.Text = "الشاشة ";
            // 
            // cmbPermissionType
            // 
            cmbPermissionType.FormattingEnabled = true;
            cmbPermissionType.Location = new Point(-171, 26);
            cmbPermissionType.Name = "cmbPermissionType";
            cmbPermissionType.Size = new Size(158, 32);
            cmbPermissionType.TabIndex = 56;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(-146, -1);
            label16.Name = "label16";
            label16.Size = new Size(114, 24);
            label16.TabIndex = 59;
            label16.Text = "نوع الصلاحية";
            // 
            // cmbPermissionModule
            // 
            cmbPermissionModule.FormattingEnabled = true;
            cmbPermissionModule.Location = new Point(182, 26);
            cmbPermissionModule.Name = "cmbPermissionModule";
            cmbPermissionModule.Size = new Size(158, 32);
            cmbPermissionModule.TabIndex = 56;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(212, -1);
            label13.Name = "label13";
            label13.Size = new Size(63, 24);
            label13.TabIndex = 59;
            label13.Text = "الوحده";
            // 
            // cmbPermissionSearch
            // 
            cmbPermissionSearch.FormattingEnabled = true;
            cmbPermissionSearch.Location = new Point(653, 27);
            cmbPermissionSearch.Name = "cmbPermissionSearch";
            cmbPermissionSearch.Size = new Size(158, 32);
            cmbPermissionSearch.TabIndex = 56;
            // 
            // cmbPermissionRole
            // 
            cmbPermissionRole.FormattingEnabled = true;
            cmbPermissionRole.Location = new Point(483, 26);
            cmbPermissionRole.Name = "cmbPermissionRole";
            cmbPermissionRole.Size = new Size(158, 32);
            cmbPermissionRole.TabIndex = 56;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(656, 0);
            label14.Name = "label14";
            label14.Size = new Size(171, 24);
            label14.TabIndex = 59;
            label14.Text = "بحث في الصلاحيات";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(508, -1);
            label12.Name = "label12";
            label12.Size = new Size(47, 24);
            label12.TabIndex = 59;
            label12.Text = "الدور";
            // 
            // grpUserData
            // 
            grpUserData.BackColor = Color.FromArgb(0, 0, 192);
            grpUserData.Controls.Add(panel1);
            grpUserData.Dock = DockStyle.Right;
            grpUserData.Font = new Font("Tahoma", 12F);
            grpUserData.ForeColor = Color.White;
            grpUserData.Location = new Point(852, 0);
            grpUserData.Name = "grpUserData";
            grpUserData.RightToLeft = RightToLeft.Yes;
            grpUserData.Size = new Size(463, 522);
            grpUserData.TabIndex = 0;
            grpUserData.TabStop = false;
            grpUserData.Text = "بيانات المستخدمين";
            grpUserData.Enter += grpUserData_Enter;
            // 
            // panel1
            // 
            panel1.BackColor = Color.WhiteSmoke;
            panel1.Controls.Add(label11);
            panel1.Controls.Add(txtFullName);
            panel1.Controls.Add(chkIsActive);
            panel1.Controls.Add(chkChangePassword);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(label9);
            panel1.Controls.Add(label10);
            panel1.Controls.Add(txtNotes);
            panel1.Controls.Add(txtEmail);
            panel1.Controls.Add(txtPhone);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(cmbStatus);
            panel1.Controls.Add(cmbRole);
            panel1.Controls.Add(cmbBranch);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(txtConfirmPassword);
            panel1.Controls.Add(txtPassword);
            panel1.Controls.Add(txtLoginName);
            panel1.Controls.Add(txtUserName);
            panel1.Dock = DockStyle.Fill;
            panel1.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            panel1.ForeColor = Color.Black;
            panel1.Location = new Point(3, 28);
            panel1.Name = "panel1";
            panel1.Size = new Size(457, 491);
            panel1.TabIndex = 0;
            panel1.Paint += panel1_Paint;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(296, 14);
            label11.Name = "label11";
            label11.Size = new Size(112, 25);
            label11.TabIndex = 71;
            label11.Text = "اسم الموظف";
            // 
            // txtFullName
            // 
            txtFullName.Location = new Point(72, 10);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(203, 31);
            txtFullName.TabIndex = 70;
            // 
            // chkIsActive
            // 
            chkIsActive.AutoSize = true;
            chkIsActive.Location = new Point(182, 420);
            chkIsActive.Name = "chkIsActive";
            chkIsActive.Size = new Size(149, 29);
            chkIsActive.TabIndex = 69;
            chkIsActive.Text = "المستخدم نشط";
            chkIsActive.UseVisualStyleBackColor = true;
            // 
            // chkChangePassword
            // 
            chkChangePassword.AutoSize = true;
            chkChangePassword.Location = new Point(62, 385);
            chkChangePassword.Name = "chkChangePassword";
            chkChangePassword.Size = new Size(269, 29);
            chkChangePassword.TabIndex = 68;
            chkChangePassword.Text = "تغيير كلمة المرور عند أول دخول";
            chkChangePassword.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(296, 356);
            label8.Name = "label8";
            label8.Size = new Size(93, 25);
            label8.TabIndex = 67;
            label8.Text = "الملاحظات";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(296, 326);
            label9.Name = "label9";
            label9.Size = new Size(132, 25);
            label9.TabIndex = 66;
            label9.Text = "البريد الإلكتروني";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(296, 290);
            label10.Name = "label10";
            label10.Size = new Size(61, 25);
            label10.TabIndex = 65;
            label10.Text = "الهاتف";
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(72, 352);
            txtNotes.Name = "txtNotes";
            txtNotes.Size = new Size(203, 31);
            txtNotes.TabIndex = 64;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(72, 319);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(203, 31);
            txtEmail.TabIndex = 63;
            // 
            // txtPhone
            // 
            txtPhone.Location = new Point(72, 286);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(203, 31);
            txtPhone.TabIndex = 62;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(286, 255);
            label5.Name = "label5";
            label5.Size = new Size(54, 25);
            label5.TabIndex = 61;
            label5.Text = "الحاله";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(290, 221);
            label6.Name = "label6";
            label6.Size = new Size(49, 25);
            label6.TabIndex = 60;
            label6.Text = "الدور";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(294, 184);
            label7.Name = "label7";
            label7.Size = new Size(51, 25);
            label7.TabIndex = 59;
            label7.Text = "الفرع";
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(72, 252);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(203, 33);
            cmbStatus.TabIndex = 58;
            // 
            // cmbRole
            // 
            cmbRole.FormattingEnabled = true;
            cmbRole.Location = new Point(72, 218);
            cmbRole.Name = "cmbRole";
            cmbRole.Size = new Size(203, 33);
            cmbRole.TabIndex = 57;
            // 
            // cmbBranch
            // 
            cmbBranch.FormattingEnabled = true;
            cmbBranch.Location = new Point(72, 179);
            cmbBranch.Name = "cmbBranch";
            cmbBranch.Size = new Size(203, 33);
            cmbBranch.TabIndex = 56;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(296, 149);
            label4.Name = "label4";
            label4.Size = new Size(137, 25);
            label4.TabIndex = 55;
            label4.Text = "تأكيد كلمه المرور";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(296, 113);
            label3.Name = "label3";
            label3.Size = new Size(97, 25);
            label3.TabIndex = 54;
            label3.Text = "كلمه المرور";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(296, 83);
            label2.Name = "label2";
            label2.Size = new Size(105, 25);
            label2.TabIndex = 53;
            label2.Text = "اسم الدخول ";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(296, 47);
            label1.Name = "label1";
            label1.Size = new Size(122, 25);
            label1.TabIndex = 52;
            label1.Text = "اسم المستخدم";
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.Location = new Point(72, 142);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(203, 31);
            txtConfirmPassword.TabIndex = 51;
            txtConfirmPassword.UseSystemPasswordChar = true;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(72, 109);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(203, 31);
            txtPassword.TabIndex = 50;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtLoginName
            // 
            txtLoginName.Location = new Point(72, 76);
            txtLoginName.Name = "txtLoginName";
            txtLoginName.Size = new Size(203, 31);
            txtLoginName.TabIndex = 49;
            // 
            // txtUserName
            // 
            txtUserName.Location = new Point(72, 43);
            txtUserName.Name = "txtUserName";
            txtUserName.Size = new Size(203, 31);
            txtUserName.TabIndex = 48;
            // 
            // FrmUsers
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1315, 792);
            Controls.Add(pnlData);
            Controls.Add(pnlGrid);
            Controls.Add(pnlToolbar);
            Name = "FrmUsers";
            Text = "إدارة المستخدمين والصلاحيات";
            pnlToolbar.ResumeLayout(false);
            pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvUsers).EndInit();
            pnlData.ResumeLayout(false);
            grpPermissions.ResumeLayout(false);
            tabPermissions.ResumeLayout(false);
            tabExtraPermissions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tabReportPermissions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            tabDataPermissions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDataPermissions).EndInit();
            tabFunctionPermissions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvFunctionPermissions).EndInit();
            pnlPermissionHeader.ResumeLayout(false);
            pnlPermissionHeader.PerformLayout();
            grpUserData.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlToolbar;
        private Panel pnlGrid;
        private Panel pnlData;
        private GroupBox grpUserData;
        private Button btnRefresh;
        private Button btnSearch;
        private Button btnDelete;
        private Button btnClose;
        private Button btnEdit;
        private Button btnPrint;
        private Button btnNew;
        private Button btnSave;
        private DataGridView dgvUsers;
        private GroupBox grpPermissions;
        private Panel panel1;
        private Label label11;
        private TextBox txtFullName;
        private CheckBox chkIsActive;
        private CheckBox chkChangePassword;
        private Label label8;
        private Label label9;
        private Label label10;
        private TextBox txtNotes;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private Label label5;
        private Label label6;
        private Label label7;
        private ComboBox cmbStatus;
        private ComboBox cmbRole;
        private ComboBox cmbBranch;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private TextBox txtConfirmPassword;
        private TextBox txtPassword;
        private TextBox txtLoginName;
        private TextBox txtUserName;
        private Panel pnlPermissionHeader;
        private ComboBox cmbPermissionRole;
        private Label label12;
        private ComboBox cmbPermissionScreen;
        private Label label15;
        private ComboBox cmbPermissionType;
        private Label label16;
        private ComboBox cmbPermissionModule;
        private Label label13;
        private CheckBox chkSelectAll;
        private ComboBox cmbPermissionSearch;
        private Label label14;
        private TabControl tabPermissions;
        private TabPage tabExtraPermissions;
        private TabPage tabReportPermissions;
        private TabPage tabDataPermissions;
        private TabPage tabFunctionPermissions;
        private DataGridView dgvFunctionPermissions;
        private DataGridViewTextBoxColumn colNo;
        private DataGridViewTextBoxColumn colFunction;
        private DataGridViewCheckBoxColumn colView;
        private DataGridViewCheckBoxColumn colAdd;
        private DataGridViewCheckBoxColumn colEdit;
        private DataGridViewCheckBoxColumn colDelete;
        private DataGridViewCheckBoxColumn colPrint;
        private DataGridViewCheckBoxColumn colApprove;
        private DataGridView dgvDataPermissions;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn colDataName;
        private DataGridViewCheckBoxColumn colDataView;
        private DataGridViewCheckBoxColumn colDataAdd;
        private DataGridViewCheckBoxColumn colDataEdit;
        private DataGridViewCheckBoxColumn colDataDelete;
        private DataGridViewCheckBoxColumn colEditAfterApprove;
        private DataGridViewCheckBoxColumn colDeleteAfterApprove;
        private DataGridViewCheckBoxColumn colChangeDate;
        private DataGridView dataGridView1;
        private DataGridView dataGridView2;
        private DataGridViewTextBoxColumn colReportNo;
        private DataGridViewTextBoxColumn colReportName;
        private DataGridViewCheckBoxColumn colReportView;
        private DataGridViewCheckBoxColumn colReportPrint;
        private DataGridViewCheckBoxColumn colExportExcel;
        private DataGridViewCheckBoxColumn colExportPdf;
        private DataGridViewCheckBoxColumn colViewProfit;
        private DataGridViewCheckBoxColumn colViewCost;
        private DataGridViewTextBoxColumn colExtraNo;
        private DataGridViewTextBoxColumn colExtraPermission;
        private DataGridViewCheckBoxColumn colAllow;

        private void pnlToolbar_Paint(object? sender, PaintEventArgs e) { }
        private void pnlPermissionHeader_Paint(object? sender, PaintEventArgs e) { }
    }
}