namespace AlTayerERP.Desktop
{
    partial class FiscalYearForm
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
            pnlGrid = new Panel();
            dgvFiscalYears = new DataGridView();
            pnlData = new Panel();
            cmbStatus = new ComboBox();
            chkIsClosed = new CheckBox();
            chkIsDefault = new CheckBox();
            dtpEndDate = new DateTimePicker();
            dtpStartDate = new DateTimePicker();
            label9 = new Label();
            label7 = new Label();
            label6 = new Label();
            txtYearName = new TextBox();
            label1 = new Label();
            btnSave = new Button();
            btnNew = new Button();
            btnPrint = new Button();
            btnEdit = new Button();
            btnClose = new Button();
            btnDelete = new Button();
            btnSearch = new Button();
            btnRefresh = new Button();
            pnlToolbar = new Panel();
            btnPreview = new Button();
            pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFiscalYears).BeginInit();
            pnlData.SuspendLayout();
            pnlToolbar.SuspendLayout();
            SuspendLayout();
            // 
            // pnlGrid
            // 
            pnlGrid.BackColor = Color.WhiteSmoke;
            pnlGrid.Controls.Add(dgvFiscalYears);
            pnlGrid.Dock = DockStyle.Bottom;
            pnlGrid.Location = new Point(0, 353);
            pnlGrid.Name = "pnlGrid";
            pnlGrid.Size = new Size(992, 255);
            pnlGrid.TabIndex = 1;
            pnlGrid.Paint += panel2_Paint;
            // 
            // dgvFiscalYears
            // 
            dgvFiscalYears.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvFiscalYears.Dock = DockStyle.Fill;
            dgvFiscalYears.Location = new Point(0, 0);
            dgvFiscalYears.Name = "dgvFiscalYears";
            dgvFiscalYears.RowHeadersWidth = 51;
            dgvFiscalYears.Size = new Size(992, 255);
            dgvFiscalYears.TabIndex = 0;
            // 
            // pnlData
            // 
            pnlData.Controls.Add(cmbStatus);
            pnlData.Controls.Add(chkIsClosed);
            pnlData.Controls.Add(chkIsDefault);
            pnlData.Controls.Add(dtpEndDate);
            pnlData.Controls.Add(dtpStartDate);
            pnlData.Controls.Add(label9);
            pnlData.Controls.Add(label7);
            pnlData.Controls.Add(label6);
            pnlData.Controls.Add(txtYearName);
            pnlData.Controls.Add(label1);
            pnlData.Dock = DockStyle.Fill;
            pnlData.Location = new Point(0, 90);
            pnlData.Name = "pnlData";
            pnlData.Size = new Size(992, 263);
            pnlData.TabIndex = 2;
            pnlData.Paint += panel3_Paint;
            // 
            // cmbStatus
            // 
            cmbStatus.FormattingEnabled = true;
            cmbStatus.Location = new Point(585, 219);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(251, 28);
            cmbStatus.TabIndex = 42;
            // 
            // chkIsClosed
            // 
            chkIsClosed.AutoSize = true;
            chkIsClosed.Location = new Point(845, 186);
            chkIsClosed.Name = "chkIsClosed";
            chkIsClosed.Size = new Size(108, 24);
            chkIsClosed.TabIndex = 41;
            chkIsClosed.Text = "السنه مقفله";
            chkIsClosed.UseVisualStyleBackColor = true;
            // 
            // chkIsDefault
            // 
            chkIsDefault.AutoSize = true;
            chkIsDefault.Location = new Point(845, 151);
            chkIsDefault.Name = "chkIsDefault";
            chkIsDefault.Size = new Size(133, 24);
            chkIsDefault.TabIndex = 40;
            chkIsDefault.Text = "السنه الإفتراضيه";
            chkIsDefault.UseVisualStyleBackColor = true;
            // 
            // dtpEndDate
            // 
            dtpEndDate.Location = new Point(586, 113);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(250, 27);
            dtpEndDate.TabIndex = 39;
            // 
            // dtpStartDate
            // 
            dtpStartDate.Location = new Point(586, 74);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(250, 27);
            dtpStartDate.TabIndex = 38;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(858, 222);
            label9.Name = "label9";
            label9.Size = new Size(44, 20);
            label9.TabIndex = 37;
            label9.Text = "الحاله";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(858, 113);
            label7.Name = "label7";
            label7.Size = new Size(82, 20);
            label7.TabIndex = 35;
            label7.Text = "تاريخ النهايه";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(858, 74);
            label6.Name = "label6";
            label6.Size = new Size(82, 20);
            label6.TabIndex = 34;
            label6.Text = "تاريخ البدايه";
            // 
            // txtYearName
            // 
            txtYearName.Location = new Point(586, 33);
            txtYearName.Name = "txtYearName";
            txtYearName.Size = new Size(250, 27);
            txtYearName.TabIndex = 33;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(858, 33);
            label1.Name = "label1";
            label1.Size = new Size(88, 20);
            label1.TabIndex = 32;
            label1.Text = "السنة المالية";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(242, 13);
            btnSave.Name = "btnSave";
            btnSave.RightToLeft = RightToLeft.No;
            btnSave.Size = new Size(94, 29);
            btnSave.TabIndex = 19;
            btnSave.Text = "حفظ";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(142, 13);
            btnNew.Name = "btnNew";
            btnNew.RightToLeft = RightToLeft.No;
            btnNew.Size = new Size(94, 29);
            btnNew.TabIndex = 31;
            btnNew.Text = "جديد";
            btnNew.UseVisualStyleBackColor = true;
            // 
            // btnPrint
            // 
            btnPrint.Location = new Point(142, 48);
            btnPrint.Name = "btnPrint";
            btnPrint.RightToLeft = RightToLeft.No;
            btnPrint.Size = new Size(94, 29);
            btnPrint.TabIndex = 30;
            btnPrint.Text = "طباعة";
            btnPrint.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(342, 13);
            btnEdit.Name = "btnEdit";
            btnEdit.RightToLeft = RightToLeft.No;
            btnEdit.Size = new Size(94, 29);
            btnEdit.TabIndex = 29;
            btnEdit.Text = "تعديل";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(242, 48);
            btnClose.Name = "btnClose";
            btnClose.RightToLeft = RightToLeft.No;
            btnClose.Size = new Size(94, 29);
            btnClose.TabIndex = 28;
            btnClose.Text = "اغلاق";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(442, 13);
            btnDelete.Name = "btnDelete";
            btnDelete.RightToLeft = RightToLeft.No;
            btnDelete.Size = new Size(94, 29);
            btnDelete.TabIndex = 27;
            btnDelete.Text = "حذف";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(542, 13);
            btnSearch.Name = "btnSearch";
            btnSearch.RightToLeft = RightToLeft.No;
            btnSearch.Size = new Size(94, 29);
            btnSearch.TabIndex = 24;
            btnSearch.Text = "بحث";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click_1;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(642, 13);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.RightToLeft = RightToLeft.No;
            btnRefresh.Size = new Size(94, 29);
            btnRefresh.TabIndex = 22;
            btnRefresh.Text = "تحديث";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = Color.WhiteSmoke;
            pnlToolbar.Controls.Add(btnPreview);
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
            pnlToolbar.Size = new Size(992, 90);
            pnlToolbar.TabIndex = 0;
            // 
            // btnPreview
            // 
            btnPreview.Location = new Point(12, 13);
            btnPreview.Name = "btnPreview";
            btnPreview.RightToLeft = RightToLeft.No;
            btnPreview.Size = new Size(94, 29);
            btnPreview.TabIndex = 32;
            btnPreview.Text = "معاينة";
            btnPreview.UseVisualStyleBackColor = true;
            btnPreview.Click += btnPreview_Click;
            // 
            // FiscalYearForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(992, 608);
            Controls.Add(pnlData);
            Controls.Add(pnlGrid);
            Controls.Add(pnlToolbar);
            Name = "FiscalYearForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "إدارة السنوات الماليه";
            WindowState = FormWindowState.Maximized;
            pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvFiscalYears).EndInit();
            pnlData.ResumeLayout(false);
            pnlData.PerformLayout();
            pnlToolbar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlToolbar;
        private Panel pnlGrid;
        private Panel pnlData;
        private Button btnUnApprove;
        private Button btnApprove;
        private Button btnRefresh;
        private Button btnPreview;
        private Button btnSearch;
        private Button btnImport;
        private Button btnExport;
        private Button btnDelete;
        private Button btnClose;
        private Button btnEdit;
        private Button btnPrint;
        private Button btnNew;
        private Button btnSave;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label6;
        private TextBox txtYearName;
        private Label label1;
        private CheckBox chkIsClosed;
        private CheckBox chkIsDefault;
        private DateTimePicker dtpEndDate;
        private DateTimePicker dtpStartDate;
        private DataGridView dgvFiscalYears;
        private ComboBox cmbStatus;
        private Button button1;
    }
}