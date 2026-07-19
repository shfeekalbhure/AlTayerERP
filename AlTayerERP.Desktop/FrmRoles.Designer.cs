namespace AlTayerERP.Desktop
{
    partial class FrmRoles
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
            btnClose = new Button();
            btnRefresh = new Button();
            btnSearch = new Button();
            btnDelete = new Button();
            btnEdit = new Button();
            btnNew = new Button();
            btnSave = new Button();
            pnlGrid = new Panel();
            dgvRoles = new DataGridView();
            Role_ID = new DataGridViewTextBoxColumn();
            Role_Code = new DataGridViewTextBoxColumn();
            Role_Name = new DataGridViewTextBoxColumn();
            Description = new DataGridViewTextBoxColumn();
            Is_Active = new DataGridViewTextBoxColumn();
            pnlData = new Panel();
            grpRoleData = new GroupBox();
            label5 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            cmbStatus = new ComboBox();
            txtDescription = new TextBox();
            txtRoleName = new TextBox();
            txtRoleCode = new TextBox();
            pnlToolbar.SuspendLayout();
            pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRoles).BeginInit();
            pnlData.SuspendLayout();
            grpRoleData.SuspendLayout();
            SuspendLayout();
            // 
            // pnlToolbar
            // 
            pnlToolbar.Controls.Add(btnClose);
            pnlToolbar.Controls.Add(btnRefresh);
            pnlToolbar.Controls.Add(btnSearch);
            pnlToolbar.Controls.Add(btnDelete);
            pnlToolbar.Controls.Add(btnEdit);
            pnlToolbar.Controls.Add(btnNew);
            pnlToolbar.Controls.Add(btnSave);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(1064, 70);
            pnlToolbar.TabIndex = 0;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(244, 21);
            btnClose.Name = "btnClose";
            btnClose.RightToLeft = RightToLeft.No;
            btnClose.Size = new Size(94, 29);
            btnClose.TabIndex = 51;
            btnClose.Text = "إغلاق";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(372, 21);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.RightToLeft = RightToLeft.No;
            btnRefresh.Size = new Size(94, 29);
            btnRefresh.TabIndex = 46;
            btnRefresh.Text = "تحديث";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(485, 21);
            btnSearch.Name = "btnSearch";
            btnSearch.RightToLeft = RightToLeft.No;
            btnSearch.Size = new Size(94, 29);
            btnSearch.TabIndex = 47;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(609, 21);
            btnDelete.Name = "btnDelete";
            btnDelete.RightToLeft = RightToLeft.No;
            btnDelete.Size = new Size(94, 29);
            btnDelete.TabIndex = 48;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(737, 21);
            btnEdit.Name = "btnEdit";
            btnEdit.RightToLeft = RightToLeft.No;
            btnEdit.Size = new Size(94, 29);
            btnEdit.TabIndex = 49;
            btnEdit.Text = "تعديل";
            btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(958, 21);
            btnNew.Name = "btnNew";
            btnNew.RightToLeft = RightToLeft.No;
            btnNew.Size = new Size(94, 29);
            btnNew.TabIndex = 50;
            btnNew.Text = "جديد";
            btnNew.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(858, 21);
            btnSave.Name = "btnSave";
            btnSave.RightToLeft = RightToLeft.No;
            btnSave.Size = new Size(94, 29);
            btnSave.TabIndex = 45;
            btnSave.Text = "حفظ";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // pnlGrid
            // 
            pnlGrid.Controls.Add(dgvRoles);
            pnlGrid.Dock = DockStyle.Bottom;
            pnlGrid.Location = new Point(0, 350);
            pnlGrid.Name = "pnlGrid";
            pnlGrid.Size = new Size(1064, 250);
            pnlGrid.TabIndex = 1;
            // 
            // dgvRoles
            // 
            dgvRoles.AllowUserToAddRows = false;
            dgvRoles.AllowUserToDeleteRows = false;
            dgvRoles.AllowUserToResizeRows = false;
            dgvRoles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRoles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRoles.Columns.AddRange(new DataGridViewColumn[] { Role_ID, Role_Code, Role_Name, Description, Is_Active });
            dgvRoles.Dock = DockStyle.Fill;
            dgvRoles.Location = new Point(0, 0);
            dgvRoles.Name = "dgvRoles";
            dgvRoles.ReadOnly = true;
            dgvRoles.RightToLeft = RightToLeft.Yes;
            dgvRoles.RowHeadersVisible = false;
            dgvRoles.RowHeadersWidth = 51;
            dgvRoles.RowTemplate.Height = 30;
            dgvRoles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRoles.Size = new Size(1064, 250);
            dgvRoles.TabIndex = 0;
            // 
            // Role_ID
            // 
            Role_ID.FillWeight = 0.6593294F;
            Role_ID.HeaderText = "رقم الدور";
            Role_ID.MinimumWidth = 6;
            Role_ID.Name = "Role_ID";
            Role_ID.ReadOnly = true;
            // 
            // Role_Code
            // 
            Role_Code.FillWeight = 2.661217F;
            Role_Code.HeaderText = "كود الدور";
            Role_Code.MinimumWidth = 6;
            Role_Code.Name = "Role_Code";
            Role_Code.ReadOnly = true;
            // 
            // Role_Name
            // 
            Role_Name.FillWeight = 26.2943363F;
            Role_Name.HeaderText = "اسم الدور";
            Role_Name.MinimumWidth = 6;
            Role_Name.Name = "Role_Name";
            Role_Name.ReadOnly = true;
            // 
            // Description
            // 
            Description.FillWeight = 203.005447F;
            Description.HeaderText = "الوصف";
            Description.MinimumWidth = 6;
            Description.Name = "Description";
            Description.ReadOnly = true;
            // 
            // Is_Active
            // 
            Is_Active.FillWeight = 267.379669F;
            Is_Active.HeaderText = "الحالة";
            Is_Active.MinimumWidth = 6;
            Is_Active.Name = "Is_Active";
            Is_Active.ReadOnly = true;
            // 
            // pnlData
            // 
            pnlData.Controls.Add(grpRoleData);
            pnlData.Dock = DockStyle.Fill;
            pnlData.Location = new Point(0, 70);
            pnlData.Name = "pnlData";
            pnlData.Size = new Size(1064, 280);
            pnlData.TabIndex = 2;
            // 
            // grpRoleData
            // 
            grpRoleData.Controls.Add(label5);
            grpRoleData.Controls.Add(label3);
            grpRoleData.Controls.Add(label2);
            grpRoleData.Controls.Add(label1);
            grpRoleData.Controls.Add(cmbStatus);
            grpRoleData.Controls.Add(txtDescription);
            grpRoleData.Controls.Add(txtRoleName);
            grpRoleData.Controls.Add(txtRoleCode);
            grpRoleData.Dock = DockStyle.Fill;
            grpRoleData.Location = new Point(0, 0);
            grpRoleData.Name = "grpRoleData";
            grpRoleData.RightToLeft = RightToLeft.Yes;
            grpRoleData.Size = new Size(1064, 280);
            grpRoleData.TabIndex = 0;
            grpRoleData.TabStop = false;
            grpRoleData.Text = "بينات الادوار";
            grpRoleData.Enter += grpRoleData_Enter;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(925, 191);
            label5.Name = "label5";
            label5.Size = new Size(44, 20);
            label5.TabIndex = 2;
            label5.Text = "الحالة";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(925, 114);
            label3.Name = "label3";
            label3.Size = new Size(56, 20);
            label3.TabIndex = 2;
            label3.Text = "الوصف";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(925, 81);
            label2.Name = "label2";
            label2.Size = new Size(70, 20);
            label2.TabIndex = 2;
            label2.Text = "اسم الدور";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(925, 48);
            label1.Name = "label1";
            label1.Size = new Size(68, 20);
            label1.TabIndex = 2;
            label1.Text = "كود الدور";
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(768, 187);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(151, 28);
            cmbStatus.TabIndex = 1;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(624, 111);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(295, 60);
            txtDescription.TabIndex = 0;
            // 
            // txtRoleName
            // 
            txtRoleName.Location = new Point(624, 78);
            txtRoleName.Name = "txtRoleName";
            txtRoleName.Size = new Size(295, 27);
            txtRoleName.TabIndex = 0;
            // 
            // txtRoleCode
            // 
            txtRoleCode.Location = new Point(794, 45);
            txtRoleCode.Name = "txtRoleCode";
            txtRoleCode.Size = new Size(125, 27);
            txtRoleCode.TabIndex = 0;
            // 
            // FrmRoles
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1064, 600);
            Controls.Add(pnlData);
            Controls.Add(pnlGrid);
            Controls.Add(pnlToolbar);
            Name = "FrmRoles";
            Text = "شاشة الأدوار";
            pnlToolbar.ResumeLayout(false);
            pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvRoles).EndInit();
            pnlData.ResumeLayout(false);
            grpRoleData.ResumeLayout(false);
            grpRoleData.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlToolbar;
        private Panel pnlGrid;
        private Panel pnlData;
        private GroupBox grpRoleData;
        private ComboBox cmbStatus;
        private TextBox txtRoleCode;
        private Label label5;
        private Label label3;
        private Label label2;
        private Label label1;
        private TextBox txtDescription;
        private TextBox txtRoleName;
        private DataGridView dgvRoles;
        private Button btnRefresh;
        private Button btnSearch;
        private Button btnDelete;
        private Button btnEdit;
        private Button btnNew;
        private Button btnSave;
        private Button btnClose;
        private DataGridViewTextBoxColumn Role_ID;
        private DataGridViewTextBoxColumn Role_Code;
        private DataGridViewTextBoxColumn Role_Name;
        private DataGridViewTextBoxColumn Description;
        private DataGridViewTextBoxColumn Is_Active;
    }
}