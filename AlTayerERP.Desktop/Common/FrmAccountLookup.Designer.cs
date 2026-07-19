namespace AlTayerERP.Desktop.Common
{
    partial class FrmAccountLookup
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
            panel1 = new Panel();
            chkSearchAsYouType = new CheckBox();
            txtSearch = new TextBox();
            lblSearch = new Label();
            panel2 = new Panel();
            dgvAccounts = new DataGridView();
            colAccountId = new DataGridViewTextBoxColumn();
            colAccountCode = new DataGridViewTextBoxColumn();
            colAccountName = new DataGridViewTextBoxColumn();
            colAccountGroup = new DataGridViewTextBoxColumn();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAccounts).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(chkSearchAsYouType);
            panel1.Controls.Add(txtSearch);
            panel1.Controls.Add(lblSearch);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(880, 98);
            panel1.TabIndex = 0;
            // 
            // chkSearchAsYouType
            // 
            chkSearchAsYouType.AutoSize = true;
            chkSearchAsYouType.Location = new Point(586, 55);
            chkSearchAsYouType.Name = "chkSearchAsYouType";
            chkSearchAsYouType.RightToLeft = RightToLeft.Yes;
            chkSearchAsYouType.Size = new Size(130, 24);
            chkSearchAsYouType.TabIndex = 55;
            chkSearchAsYouType.Text = "بحث اثناء الكتابه";
            chkSearchAsYouType.UseVisualStyleBackColor = true;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(492, 22);
            txtSearch.Name = "txtSearch";
            txtSearch.RightToLeft = RightToLeft.No;
            txtSearch.Size = new Size(195, 27);
            txtSearch.TabIndex = 53;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(703, 25);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(37, 20);
            lblSearch.TabIndex = 52;
            lblSearch.Text = "بحث";
            // 
            // panel2
            // 
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 456);
            panel2.Name = "panel2";
            panel2.Size = new Size(880, 53);
            panel2.TabIndex = 1;
            // 
            // dgvAccounts
            // 
            dgvAccounts.AllowUserToAddRows = false;
            dgvAccounts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAccounts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAccounts.Columns.AddRange(new DataGridViewColumn[] { colAccountId, colAccountCode, colAccountName, colAccountGroup });
            dgvAccounts.Dock = DockStyle.Fill;
            dgvAccounts.Location = new Point(0, 98);
            dgvAccounts.MultiSelect = false;
            dgvAccounts.Name = "dgvAccounts";
            dgvAccounts.ReadOnly = true;
            dgvAccounts.RightToLeft = RightToLeft.Yes;
            dgvAccounts.RowHeadersVisible = false;
            dgvAccounts.RowHeadersWidth = 51;
            dgvAccounts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAccounts.Size = new Size(880, 358);
            dgvAccounts.TabIndex = 2;
          //  dgvAccounts.CellContentClick += dataGridView1_CellContentClick;
            // 
            // colAccountId
            // 
            colAccountId.HeaderText = "معرف الحساب";
            colAccountId.MinimumWidth = 6;
            colAccountId.Name = "colAccountId";
            colAccountId.ReadOnly = true;
            colAccountId.Visible = false;
            // 
            // colAccountCode
            // 
            colAccountCode.HeaderText = "رقم الحساب";
            colAccountCode.MinimumWidth = 6;
            colAccountCode.Name = "colAccountCode";
            colAccountCode.ReadOnly = true;
            // 
            // colAccountName
            // 
            colAccountName.HeaderText = "اسم الحساب";
            colAccountName.MinimumWidth = 6;
            colAccountName.Name = "colAccountName";
            colAccountName.ReadOnly = true;
            // 
            // colAccountGroup
            // 
            colAccountGroup.HeaderText = "مجموعه الحساب";
            colAccountGroup.MinimumWidth = 6;
            colAccountGroup.Name = "colAccountGroup";
            colAccountGroup.ReadOnly = true;
            // 
            // FrmAccountLookup
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(880, 509);
            Controls.Add(dgvAccounts);
            Controls.Add(panel2);
            Controls.Add(panel1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmAccountLookup";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "دليل الحسابات";
            WindowState = FormWindowState.Minimized;
       //     Load += FrmAccountLookup_Load;   

       //     Load += FrmAccountLookup_Shown;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAccounts).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel2;
        private DataGridView dgvAccounts;
        private TextBox txtSearch;
        private Label lblSearch;
        private CheckBox chkSearchAsYouType;
        private DataGridViewTextBoxColumn colAccountId;
        private DataGridViewTextBoxColumn colAccountCode;
        private DataGridViewTextBoxColumn colAccountName;
        private DataGridViewTextBoxColumn colAccountGroup;
    }
}